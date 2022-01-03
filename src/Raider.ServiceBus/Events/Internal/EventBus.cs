using Raider.ServiceBus.BusLogger;
using Raider.ServiceBus.Events.Config;
using Raider.ServiceBus.Events.Storage.Model;
using Raider.Trace;
using Raider.Transactions;
using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Raider.ServiceBus.Events.Internal
{
	public abstract class EventBus : IEventBus
	{
		protected IServiceProvider ServiceProvider { get; }

		private static readonly ConcurrentDictionary<Type, EventHandlerProcessorBase> _eventHandlerProcessors = new();
		private static readonly ConcurrentDictionary<Type, EventHandlerProcessorBase> _asyncEventHandlerProcessors = new();

		protected abstract IEventBusOptions Options { get; }
		protected abstract Guid IdHost { get; }

		public EventBus(IServiceProvider serviceProvider)
		{
			ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
		}

		public virtual IResult Publish(IEvent @event,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
			=> Publish(@event, new EventOptions(), CreateTransactionContext(), true, TraceInfo.Create((ITraceInfo?)null, null, memberName, sourceFilePath, sourceLineNumber));

		public virtual IResult Publish(IEvent @event, ITransactionContext transactionContext, ITraceInfo traceInfo)
			=> Publish(@event, new EventOptions(), transactionContext, false, traceInfo);

		public virtual IResult Publish(IEvent @event, EventOptions options, ITransactionContext transactionContext, ITraceInfo traceInfo)
			=> Publish(@event, options, transactionContext, false, traceInfo);

		protected virtual IResult Publish(IEvent @event, EventOptions options, ITransactionContext transactionContext, bool isLocalTransactionContext, ITraceInfo traceInfo)
		{
			try
			{
				if (@event == null)
					throw new ArgumentNullException(nameof(@event));
				if (options == null)
					throw new ArgumentNullException(nameof(options));
				if (transactionContext == null)
					throw new ArgumentNullException(nameof(transactionContext));
				if (traceInfo == null)
					throw new ArgumentNullException(nameof(traceInfo));

				var eventType = @event.GetType();
				var handlerProcessor = (EventHandlerProcessor)_eventHandlerProcessors.GetOrAdd(eventType,
					evtType => (EventHandlerProcessorBase)Activator.CreateInstance(typeof(EventHandlerProcessor<,>).MakeGenericType(evtType, Options.EventHandlerContextType))!
					?? throw new InvalidOperationException($"Could not create handlerProcessor type for {evtType}"));

				var savedEvent = SaveEvent(@event, options);
				if (savedEvent == null)
					throw new InvalidOperationException($"{nameof(savedEvent)} == null | {nameof(eventType)} = {eventType.FullName}");
				if (savedEvent.Event == null)
					throw new InvalidOperationException($"{nameof(savedEvent)}.{nameof(savedEvent.Event)} == null | {nameof(eventType)} = {eventType.FullName}");

				var handlerContext = Options.EventHandlerContextFactory(ServiceProvider);

				if (handlerContext == null || !Options.EventHandlerContextType.IsAssignableFrom(handlerContext.GetType()))
					throw new InvalidOperationException($"{nameof(Options.EventHandlerContextFactory)} returns invalid context type of {handlerContext?.GetType()?.FullName}. Context type must be assignable to {Options.EventHandlerContextType.FullName}");

				handlerContext.TransactionContext = transactionContext;
				handlerContext.PreviousTraceInfo = TraceInfo.Create(traceInfo);
				handlerContext.IdHost = IdHost;
				handlerContext.IdEvent = savedEvent.IdSavedEvent;
				handlerContext.IdSession = options.IdSession;
				handlerContext.Timeout = options.Timeout;
				handlerContext.HandlerEventLogger = GetHandlerEventLogger();

				var result = handlerProcessor.Handle(savedEvent.Event, handlerContext, ServiceProvider);

				if (isLocalTransactionContext)
					transactionContext.Commit();

				return result;
			}
			catch (Exception exHost)
			{
				GetHostLogger().LogError(traceInfo ?? TraceInfo.Create(), IdHost, HostStatus.Unchanged, x => x.ExceptionInfo(exHost), "Publish<IEvent> error", null);

				if (isLocalTransactionContext)
				{
					try
					{
						transactionContext.Rollback();
					}
					catch (Exception rollbackEx)
					{
						GetHostLogger().LogError(traceInfo ?? TraceInfo.Create(), IdHost, HostStatus.Unchanged, x => x.ExceptionInfo(rollbackEx), "Publish<IEvent> rollback error", null);
					}
				}

				throw;
			}
		}

		public virtual async Task<IResult> PublishAsync(IEvent @event,
			CancellationToken cancellationToken = default,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
			=> await PublishAsync(@event, new EventOptions(), await CreateTransactionContextAsync(cancellationToken), true, TraceInfo.Create((ITraceInfo?)null, null, memberName, sourceFilePath, sourceLineNumber), cancellationToken);

		public virtual Task<IResult> PublishAsync(IEvent @event, ITransactionContext transactionContext, ITraceInfo traceInfo, CancellationToken cancellationToken = default)
			=> PublishAsync(@event, new EventOptions(), transactionContext, false, traceInfo, cancellationToken);

		public virtual Task<IResult> PublishAsync(IEvent @event, EventOptions options, ITransactionContext transactionContext, ITraceInfo traceInfo, CancellationToken cancellationToken = default)
			=> PublishAsync(@event, options, transactionContext, false, traceInfo, cancellationToken);

		protected virtual async Task<IResult> PublishAsync(IEvent @event, EventOptions options, ITransactionContext transactionContext, bool isLocalTransactionContext, ITraceInfo traceInfo, CancellationToken cancellationToken = default)
		{
			try
			{
				if (@event == null)
					throw new ArgumentNullException(nameof(@event));
				if (options == null)
					throw new ArgumentNullException(nameof(options));
				if (transactionContext == null)
					throw new ArgumentNullException(nameof(transactionContext));
				if (traceInfo == null)
					throw new ArgumentNullException(nameof(traceInfo));

				var eventType = @event.GetType();
				var handlerProcessor = (AsyncEventHandlerProcessor)_asyncEventHandlerProcessors.GetOrAdd(eventType,
					eventType => (EventHandlerProcessorBase)Activator.CreateInstance(typeof(AsyncEventHandlerProcessor<,>).MakeGenericType(eventType, Options.EventHandlerContextType))!
					?? throw new InvalidOperationException($"Could not create handlerProcessor type for {eventType}"));

				var savedEvent = await SaveEventAsync(@event, options, cancellationToken);
				if (savedEvent == null)
					throw new InvalidOperationException($"{nameof(savedEvent)} == null | {nameof(eventType)} = {eventType.FullName}");
				if (savedEvent.Event == null)
					throw new InvalidOperationException($"{nameof(savedEvent)}.{nameof(savedEvent.Event)} == null | {nameof(eventType)} = {eventType.FullName}");

				var handlerContext = Options.EventHandlerContextFactory(ServiceProvider);

				if (handlerContext == null || !Options.EventHandlerContextType.IsAssignableFrom(handlerContext.GetType()))
					throw new InvalidOperationException($"{nameof(Options.EventHandlerContextFactory)} returns invalid context type of {handlerContext?.GetType()?.FullName}. Context type must be assignable to {Options.EventHandlerContextType.FullName}");

				handlerContext.TransactionContext = transactionContext;
				handlerContext.PreviousTraceInfo = TraceInfo.Create(traceInfo);
				handlerContext.IdHost = IdHost;
				handlerContext.IdEvent = savedEvent.IdSavedEvent;
				handlerContext.IdSession = options.IdSession;
				handlerContext.Timeout = options.Timeout;
				handlerContext.HandlerEventLogger = GetHandlerEventLogger();

				var result = await handlerProcessor.HandleAsync(savedEvent.Event, handlerContext, ServiceProvider, cancellationToken);

				if (isLocalTransactionContext)
					await transactionContext.CommitAsync(cancellationToken);

				return result;
			}
			catch (Exception exHost)
			{
				await GetHostLogger().LogErrorAsync(traceInfo ?? TraceInfo.Create(), IdHost, HostStatus.Unchanged, x => x.ExceptionInfo(exHost), "PublishAsync<IEvent> error", null, cancellationToken);

				if (isLocalTransactionContext)
				{
					try
					{
						await transactionContext.RollbackAsync(cancellationToken);
					}
					catch (Exception rollbackEx)
					{
						await GetHostLogger().LogErrorAsync(traceInfo ?? TraceInfo.Create(), IdHost, HostStatus.Unchanged, x => x.ExceptionInfo(rollbackEx), "PublishAsync<IEvent> rollback error", null);
					}
				}

				throw;
			}
		}


		protected abstract ITransactionContext CreateTransactionContext();

		protected abstract Task<ITransactionContext> CreateTransactionContextAsync(CancellationToken cancellationToken = default);

		protected abstract IHostLogger GetHostLogger();

		protected abstract IHandlerMessageLogger GetHandlerEventLogger();

		protected abstract SavedEvent<TEvent> SaveEvent<TEvent>(TEvent @event, EventOptions? options = null)
			where TEvent : IEvent;

		protected abstract Task<SavedEvent<TEvent>> SaveEventAsync<TEvent>(TEvent @event, EventOptions? options = null, CancellationToken cancellation = default)
			where TEvent : IEvent;
	}
}

using Raider.ServiceBus.BusLogger;
using Raider.ServiceBus.Messages.Config;
using Raider.ServiceBus.Messages.Storage.Model;
using Raider.Trace;
using Raider.Transactions;
using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Raider.ServiceBus.Messages.Internal
{
	public abstract class MessageBus : IMessageBus
	{
		protected IServiceProvider ServiceProvider { get; }

		private static readonly ConcurrentDictionary<Type, MessageHandlerProcessorBase> _voidMessageHandlerProcessors = new();
		private static readonly ConcurrentDictionary<Type, MessageHandlerProcessorBase> _messageHandlerProcessors = new();
		private static readonly ConcurrentDictionary<Type, MessageHandlerProcessorBase> _asyncMessageHandlerProcessors = new();
		private static readonly ConcurrentDictionary<Type, MessageHandlerProcessorBase> _asyncVoidMessageHandlerProcessors = new();

		protected abstract IMessageBusOptions Options { get; }
		protected abstract Guid IdHost { get; }

		public MessageBus(IServiceProvider serviceProvider)
		{
			ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
		}

		public virtual IResult Send(Messages.IRequestMessage message,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
			=> Send(message, new Messages.MessageOptions(), CreateTransactionContext(), true, TraceInfo.Create((ITraceInfo?)null, null, memberName, sourceFilePath, sourceLineNumber));

		public virtual IResult Send(Messages.IRequestMessage message, ITransactionContext transactionContext, ITraceInfo traceInfo)
			=> Send(message, new Messages.MessageOptions(), transactionContext, false, traceInfo);

		public virtual IResult Send(Messages.IRequestMessage message, Messages.MessageOptions options, ITransactionContext transactionContext, ITraceInfo traceInfo)
			=> Send(message, options, transactionContext, false, traceInfo);

		protected virtual IResult Send(Messages.IRequestMessage message, Messages.MessageOptions options, ITransactionContext transactionContext, bool isLocalTransactionContext, ITraceInfo traceInfo)
		{
			try
			{
				if (message == null)
					throw new ArgumentNullException(nameof(message));
				if (options == null)
					throw new ArgumentNullException(nameof(options));
				if (transactionContext == null)
					throw new ArgumentNullException(nameof(transactionContext));
				if (traceInfo == null)
					throw new ArgumentNullException(nameof(traceInfo));

				var requestMessageType = message.GetType();
				var handlerProcessor = (VoidMessageHandlerProcessor)_voidMessageHandlerProcessors.GetOrAdd(requestMessageType,
					messageType => (MessageHandlerProcessorBase)Activator.CreateInstance(typeof(VoidMessageHandlerProcessor<,>).MakeGenericType(messageType, Options.MessageHandlerContextType))!
					?? throw new InvalidOperationException($"Could not create handlerProcessor type for {messageType}"));

				var savedMessage = SaveRequestMessage(message, options);
				if (savedMessage == null)
					throw new InvalidOperationException($"{nameof(savedMessage)} == null | {nameof(requestMessageType)} = {requestMessageType.FullName}");
				if (savedMessage.Message == null)
					throw new InvalidOperationException($"{nameof(savedMessage)}.{nameof(savedMessage.Message)} == null | {nameof(requestMessageType)} = {requestMessageType.FullName}");

				var handlerContext = Options.MessageHandlerContextFactory(ServiceProvider);

				if (handlerContext == null || !Options.MessageHandlerContextType.IsAssignableFrom(handlerContext.GetType()))
					throw new InvalidOperationException($"{nameof(Options.MessageHandlerContextFactory)} returns invalid context type of {handlerContext?.GetType()?.FullName}. Context type must be assignable to {Options.MessageHandlerContextType.FullName}");

				handlerContext.TransactionContext = transactionContext;
				handlerContext.PreviousTraceInfo = TraceInfo.Create(traceInfo);
				handlerContext.IdHost = IdHost;
				handlerContext.IdMessage = savedMessage.IdSavedMessage;
				handlerContext.IdSession = options.IdSession;
				handlerContext.Timeout = options.Timeout;
				handlerContext.HandlerMessageLogger = GetHandlerMessageLogger();

				var result = handlerProcessor.Handle(savedMessage.Message, handlerContext, ServiceProvider);

				if (isLocalTransactionContext)
					transactionContext.Commit();

				return result;
			}
			catch (Exception exHost)
			{
				GetHostLogger().LogError(traceInfo ?? TraceInfo.Create(), IdHost, HostStatus.Unchanged, x => x.ExceptionInfo(exHost), "Send<Messages.IRequestMessage> error", null);

				if (isLocalTransactionContext)
				{
					try
					{
						transactionContext.Rollback();
					}
					catch (Exception rollbackEx)
					{
						GetHostLogger().LogError(traceInfo ?? TraceInfo.Create(), IdHost, HostStatus.Unchanged, x => x.ExceptionInfo(rollbackEx), "Send<Messages.IRequestMessage> rollback error", null);
					}
				}

				throw;
			}
		}

		public virtual IResult<TResponse> Send<TResponse>(Messages.IRequestMessage<TResponse> message,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
			=> Send(message, new Messages.MessageOptions(), CreateTransactionContext(), true, TraceInfo.Create((ITraceInfo?)null, null, memberName, sourceFilePath, sourceLineNumber));

		public virtual IResult<TResponse> Send<TResponse>(Messages.IRequestMessage<TResponse> message, ITransactionContext transactionContext, ITraceInfo traceInfo)
			=> Send(message, new Messages.MessageOptions(), transactionContext, false, traceInfo);

		public virtual IResult<TResponse> Send<TResponse>(Messages.IRequestMessage<TResponse> message, Messages.MessageOptions options, ITransactionContext transactionContext, ITraceInfo traceInfo)
			=> Send(message, options, transactionContext, false, traceInfo);

		protected virtual IResult<TResponse> Send<TResponse>(Messages.IRequestMessage<TResponse> message, Messages.MessageOptions options, ITransactionContext transactionContext, bool isLocalTransactionContext, ITraceInfo traceInfo)
		{
			try
			{
				if (message == null)
					throw new ArgumentNullException(nameof(message));
				if (options == null)
					throw new ArgumentNullException(nameof(options));
				if (transactionContext == null)
					throw new ArgumentNullException(nameof(transactionContext));
				if (traceInfo == null)
					throw new ArgumentNullException(nameof(traceInfo));

				var requestMessageType = message.GetType();
				var handlerProcessor = (MessageHandlerProcessor<TResponse>)_messageHandlerProcessors.GetOrAdd(requestMessageType,
					messageType => (MessageHandlerProcessorBase)(Activator.CreateInstance(typeof(MessageHandlerProcessor<,,>).MakeGenericType(messageType, typeof(TResponse), Options.MessageHandlerContextType))
					?? throw new InvalidOperationException($"Could not create handlerProcessor type for {messageType}")));

				var savedMessage = SaveRequestMessage(message, options);
				if (savedMessage == null)
					throw new InvalidOperationException($"{nameof(savedMessage)} == null | {nameof(requestMessageType)} = {requestMessageType.FullName}");
				if (savedMessage.Message == null)
					throw new InvalidOperationException($"{nameof(savedMessage)}.{nameof(savedMessage.Message)} == null | {nameof(requestMessageType)} = {requestMessageType.FullName}");

				var handlerContext = Options.MessageHandlerContextFactory(ServiceProvider);

				if (handlerContext == null || !Options.MessageHandlerContextType.IsAssignableFrom(handlerContext.GetType()))
					throw new InvalidOperationException($"{nameof(Options.MessageHandlerContextFactory)} returns invalid context type of {handlerContext?.GetType()?.FullName}. Context type must be assignable to {Options.MessageHandlerContextType.FullName}");

				handlerContext.TransactionContext = transactionContext;
				handlerContext.PreviousTraceInfo = TraceInfo.Create(traceInfo);
				handlerContext.IdHost = IdHost;
				handlerContext.IdMessage = savedMessage.IdSavedMessage;
				handlerContext.IdSession = options.IdSession;
				handlerContext.Timeout = options.Timeout;
				handlerContext.HandlerMessageLogger = GetHandlerMessageLogger();

				var result = handlerProcessor.Handle(savedMessage.Message, handlerContext, ServiceProvider, SaveResponseMessage);

				if (isLocalTransactionContext)
					transactionContext.Commit();

				return result;
			}
			catch (Exception exHost)
			{
				GetHostLogger().LogError(traceInfo ?? TraceInfo.Create(), IdHost, HostStatus.Unchanged, x => x.ExceptionInfo(exHost), "Send<Messages.IRequestMessage<TResponse>> error", null);

				if (isLocalTransactionContext)
				{
					try
					{
						transactionContext.Rollback();
					}
					catch (Exception rollbackEx)
					{
						GetHostLogger().LogError(traceInfo ?? TraceInfo.Create(), IdHost, HostStatus.Unchanged, x => x.ExceptionInfo(rollbackEx), "Send<Messages.IRequestMessage<TResponse>> rollback error", null);
					}
				}

				throw;
			}
		}

		public virtual async Task<IResult> SendAsync(Messages.IRequestMessage message,
			CancellationToken cancellationToken = default,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
			=> await SendAsync(message, new Messages.MessageOptions(), await CreateTransactionContextAsync(cancellationToken), true, TraceInfo.Create((ITraceInfo?)null, null, memberName, sourceFilePath, sourceLineNumber), cancellationToken);

		public virtual Task<IResult> SendAsync(Messages.IRequestMessage message, ITransactionContext transactionContext, ITraceInfo traceInfo, CancellationToken cancellationToken = default)
			=> SendAsync(message, new Messages.MessageOptions(), transactionContext, false, traceInfo, cancellationToken);

		public virtual Task<IResult> SendAsync(Messages.IRequestMessage message, Messages.MessageOptions options, ITransactionContext transactionContext, ITraceInfo traceInfo, CancellationToken cancellationToken = default)
			=> SendAsync(message, options, transactionContext, false, traceInfo, cancellationToken);

		protected virtual async Task<IResult> SendAsync(Messages.IRequestMessage message, Messages.MessageOptions options, ITransactionContext transactionContext, bool isLocalTransactionContext, ITraceInfo traceInfo, CancellationToken cancellationToken = default)
		{
			try
			{
				if (message == null)
					throw new ArgumentNullException(nameof(message));
				if (options == null)
					throw new ArgumentNullException(nameof(options));
				if (transactionContext == null)
					throw new ArgumentNullException(nameof(transactionContext));
				if (traceInfo == null)
					throw new ArgumentNullException(nameof(traceInfo));

				var requestMessageType = message.GetType();
				var handlerProcessor = (AsyncVoidMessageHandlerProcessor)_asyncMessageHandlerProcessors.GetOrAdd(requestMessageType,
					messageType => (MessageHandlerProcessorBase)Activator.CreateInstance(typeof(AsyncVoidMessageHandlerProcessor<,>).MakeGenericType(messageType, Options.MessageHandlerContextType))!
					?? throw new InvalidOperationException($"Could not create handlerProcessor type for {messageType}"));

				var savedMessage = await SaveRequestMessageAsync(message, options, cancellationToken);
				if (savedMessage == null)
					throw new InvalidOperationException($"{nameof(savedMessage)} == null | {nameof(requestMessageType)} = {requestMessageType.FullName}");
				if (savedMessage.Message == null)
					throw new InvalidOperationException($"{nameof(savedMessage)}.{nameof(savedMessage.Message)} == null | {nameof(requestMessageType)} = {requestMessageType.FullName}");

				var handlerContext = Options.MessageHandlerContextFactory(ServiceProvider);

				if (handlerContext == null || !Options.MessageHandlerContextType.IsAssignableFrom(handlerContext.GetType()))
					throw new InvalidOperationException($"{nameof(Options.MessageHandlerContextFactory)} returns invalid context type of {handlerContext?.GetType()?.FullName}. Context type must be assignable to {Options.MessageHandlerContextType.FullName}");

				handlerContext.TransactionContext = transactionContext;
				handlerContext.PreviousTraceInfo = TraceInfo.Create(traceInfo);
				handlerContext.IdHost = IdHost;
				handlerContext.IdMessage = savedMessage.IdSavedMessage;
				handlerContext.IdSession = options.IdSession;
				handlerContext.Timeout = options.Timeout;
				handlerContext.HandlerMessageLogger = GetHandlerMessageLogger();

				var result = await handlerProcessor.HandleAsync(savedMessage.Message, handlerContext, ServiceProvider, cancellationToken);

				if (isLocalTransactionContext)
					await transactionContext.CommitAsync(cancellationToken);

				return result;
			}
			catch (Exception exHost)
			{
				await GetHostLogger().LogErrorAsync(traceInfo ?? TraceInfo.Create(), IdHost, HostStatus.Unchanged, x => x.ExceptionInfo(exHost), "SendAsync<Messages.IRequestMessage> error", null, cancellationToken);

				if (isLocalTransactionContext)
				{
					try
					{
						await transactionContext.RollbackAsync(cancellationToken);
					}
					catch (Exception rollbackEx)
					{
						await GetHostLogger().LogErrorAsync(traceInfo ?? TraceInfo.Create(), IdHost, HostStatus.Unchanged, x => x.ExceptionInfo(rollbackEx), "SendAsync<Messages.IRequestMessage> rollback error", null);
					}
				}

				throw;
			}
		}

		public virtual async Task<IResult<TResponse>> SendAsync<TResponse>(Messages.IRequestMessage<TResponse> message,
			CancellationToken cancellationToken = default,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
			=> await SendAsync (message, new Messages.MessageOptions(), await CreateTransactionContextAsync(cancellationToken), true, TraceInfo.Create((ITraceInfo?)null, null, memberName, sourceFilePath, sourceLineNumber), cancellationToken);

		public virtual Task<IResult<TResponse>> SendAsync<TResponse>(Messages.IRequestMessage<TResponse> message, ITransactionContext transactionContext, ITraceInfo traceInfo, CancellationToken cancellationToken = default)
			=> SendAsync(message, new Messages.MessageOptions(), transactionContext, false, traceInfo, cancellationToken);

		public virtual Task<IResult<TResponse>> SendAsync<TResponse>(Messages.IRequestMessage<TResponse> message, Messages.MessageOptions options, ITransactionContext transactionContext, ITraceInfo traceInfo, CancellationToken cancellationToken = default)
			=> SendAsync(message, options, transactionContext, false, traceInfo, cancellationToken);

		protected virtual async Task<IResult<TResponse>> SendAsync<TResponse>(Messages.IRequestMessage<TResponse> message, Messages.MessageOptions options, ITransactionContext transactionContext, bool isLocalTransactionContext, ITraceInfo traceInfo, CancellationToken cancellationToken = default)
		{
			try
			{
				if (message == null)
					throw new ArgumentNullException(nameof(message));
				if (options == null)
					throw new ArgumentNullException(nameof(options));
				if (transactionContext == null)
					throw new ArgumentNullException(nameof(transactionContext));
				if (traceInfo == null)
					throw new ArgumentNullException(nameof(traceInfo));

				var requestMessageType = message.GetType();
				var handlerProcessor = (AsyncMessageHandlerProcessor<TResponse>)_asyncVoidMessageHandlerProcessors.GetOrAdd(requestMessageType,
					messageType => (MessageHandlerProcessorBase)(Activator.CreateInstance(typeof(AsyncMessageHandlerProcessor<,,>).MakeGenericType(messageType, typeof(TResponse), Options.MessageHandlerContextType))
					?? throw new InvalidOperationException($"Could not create handlerProcessor type for {messageType}")));

				var savedMessage = await SaveRequestMessageAsync(message, options, cancellationToken);
				if (savedMessage == null)
					throw new InvalidOperationException($"{nameof(savedMessage)} == null | {nameof(requestMessageType)} = {requestMessageType.FullName}");
				if (savedMessage.Message == null)
					throw new InvalidOperationException($"{nameof(savedMessage)}.{nameof(savedMessage.Message)} == null | {nameof(requestMessageType)} = {requestMessageType.FullName}");

				var handlerContext = Options.MessageHandlerContextFactory(ServiceProvider);

				if (handlerContext == null || !Options.MessageHandlerContextType.IsAssignableFrom(handlerContext.GetType()))
					throw new InvalidOperationException($"{nameof(Options.MessageHandlerContextFactory)} returns invalid context type of {handlerContext?.GetType()?.FullName}. Context type must be assignable to {Options.MessageHandlerContextType.FullName}");

				handlerContext.TransactionContext = transactionContext;
				handlerContext.PreviousTraceInfo = TraceInfo.Create(traceInfo);
				handlerContext.IdHost = IdHost;
				handlerContext.IdMessage = savedMessage.IdSavedMessage;
				handlerContext.IdSession = options.IdSession;
				handlerContext.Timeout = options.Timeout;
				handlerContext.HandlerMessageLogger = GetHandlerMessageLogger();

				var result = await handlerProcessor.HandleAsync(savedMessage.Message, handlerContext, ServiceProvider, SaveResponseMessageAsync, cancellationToken);

				if (isLocalTransactionContext)
					await transactionContext.CommitAsync(cancellationToken);

				return result;
			}
			catch (Exception exHost)
			{
				await GetHostLogger().LogErrorAsync(traceInfo ?? TraceInfo.Create(), IdHost, HostStatus.Unchanged, x => x.ExceptionInfo(exHost), "SendAsync<Messages.IRequestMessage<TResponse>> error", null, cancellationToken);

				if (isLocalTransactionContext)
				{
					try
					{
						await transactionContext.RollbackAsync(cancellationToken);
					}
					catch (Exception rollbackEx)
					{
						await GetHostLogger().LogErrorAsync(traceInfo ?? TraceInfo.Create(), IdHost, HostStatus.Unchanged, x => x.ExceptionInfo(rollbackEx), "SendAsync<Messages.IRequestMessage<TResponse>> rollback error", null);
					}
				}

				throw;
			}
		}


		protected abstract ITransactionContext CreateTransactionContext();

		protected abstract Task<ITransactionContext> CreateTransactionContextAsync(CancellationToken cancellationToken = default);

		protected abstract IHostLogger GetHostLogger();

		protected abstract IHandlerMessageLogger GetHandlerMessageLogger();

		protected abstract SavedMessage<TMessage> SaveRequestMessage<TMessage>(TMessage requestMessage, Messages.MessageOptions? options = null)
			where TMessage : IBaseRequestMessage;

		protected abstract Guid SaveResponseMessage<TResponse>(IResult<TResponse> responseMessage, IMessageHandlerContext handlerContext);

		protected abstract Task<SavedMessage<TMessage>> SaveRequestMessageAsync<TMessage>(TMessage requestMessage, Messages.MessageOptions? options = null, CancellationToken cancellation = default)
			where TMessage : IBaseRequestMessage;

		protected abstract Task<Guid> SaveResponseMessageAsync<TResponse>(IResult<TResponse> responseMessage, IMessageHandlerContext handlerContext, CancellationToken cancellation = default);
	}
}

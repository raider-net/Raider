using Raider.Commands.Aspects;
using Raider.DependencyInjection;
using Raider.Exceptions;
using Raider.Trace;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Raider.Commands.Internal
{
	internal abstract class AsyncCommandProcessor<TResult> : CommandProcessorBase
	{
		public AsyncCommandProcessor(ServiceFactory serviceFactory)
			: base(serviceFactory)
		{
		}

		public abstract Task<ICommandResult<bool>> CanExecuteAsync(
			ITraceInfo traceInfo,
			ICommandHandler handler,
			ICommand<TResult> command,
			ICommandInterceptorOptions? options,
			CancellationToken cancellationToken);

		public abstract Task<ICommandResult<TResult>> ExecuteAsync(
			ITraceInfo traceInfo,
			ICommandHandler handler,
			ICommand<TResult> command,
			ICommandInterceptorOptions? options,
			CancellationToken cancellationToken);
	}

	internal class AsyncCommandProcessor<TCommand, TResult> : AsyncCommandProcessor<TResult>
		where TCommand : ICommand<TResult>
	{
		private readonly ICommandHandlerRegistry _handlerRegistry;
		private readonly ICommandHandlerFactory _handlerFactory;

		public AsyncCommandProcessor(
			ICommandHandlerRegistry handlerRegistry,
			ICommandHandlerFactory handlerFactory,
			ServiceFactory serviceFactory)
			: base(serviceFactory)
		{
			_handlerRegistry = handlerRegistry ?? throw new ArgumentNullException(nameof(handlerRegistry));
			_handlerFactory = handlerFactory ?? throw new ArgumentNullException(nameof(handlerFactory));

			var _handlerType = _handlerRegistry.GetAsyncCommandHandler<TCommand, TResult>();
			if (_handlerType == null)
				throw new ConfigurationException($"No asynchronous handler registered for command: {typeof(TCommand).FullName}");
		}

		public override ICommandHandler CreateHandler()
		{
			var handler = _handlerFactory.CreateAsyncCommandHandler<TCommand, TResult>();
			if (handler == null)
				throw new InvalidOperationException($"Handler could not be created for type: {typeof(IAsyncCommandHandler<TCommand, TResult>).FullName}");

			return handler;
		}

		public override Task<ICommandResult<bool>> CanExecuteAsync(
			ITraceInfo traceInfo,
			ICommandHandler handler,
			ICommand<TResult> command,
			ICommandInterceptorOptions? options,
			CancellationToken cancellationToken)
		{
			var hnd = (IAsyncCommandHandler<TCommand, TResult>)handler;

			IAsyncCommandInterceptor<TCommand, TResult>? interceptor = null;
			if (hnd.InterceptorType != null)
			{
				if (!typeof(IAsyncCommandInterceptor<TCommand, TResult>).IsAssignableFrom(hnd.InterceptorType))
					throw new InvalidOperationException($"Handler {hnd.GetType().FullName} has invalid {nameof(hnd.InterceptorType)}. {hnd.InterceptorType.FullName} must implement {typeof(IAsyncCommandInterceptor<TCommand, TResult>).FullName}");

				interceptor = (IAsyncCommandInterceptor<TCommand, TResult>?)hnd.ServiceFactory.GetRequiredInstance(hnd.InterceptorType);
			}

			return interceptor == null
				? hnd.CanExecuteAsync((TCommand)command, CreateCommandHandlerContext(traceInfo), cancellationToken)
				: interceptor.InterceptCanExecuteAsync(traceInfo, hnd, (TCommand)command, options, cancellationToken);
		}

		public override Task<ICommandResult<TResult>> ExecuteAsync(
			ITraceInfo traceInfo,
			ICommandHandler handler,
			ICommand<TResult> command,
			ICommandInterceptorOptions? options,
			CancellationToken cancellationToken)
		{
			var hnd = (IAsyncCommandHandler<TCommand, TResult>)handler;

			IAsyncCommandInterceptor<TCommand, TResult>? interceptor = null;
			if (hnd.InterceptorType != null)
			{
				if (!typeof(IAsyncCommandInterceptor<TCommand, TResult>).IsAssignableFrom(hnd.InterceptorType))
					throw new InvalidOperationException($"Handler {hnd.GetType().FullName} has invalid {nameof(hnd.InterceptorType)}. {hnd.InterceptorType.FullName} must implement {typeof(IAsyncCommandInterceptor<TCommand, TResult>).FullName}");

				interceptor = (IAsyncCommandInterceptor<TCommand, TResult>?)hnd.ServiceFactory.GetRequiredInstance(hnd.InterceptorType);
			}

			return interceptor == null
				? hnd.ExecuteAsync((TCommand)command, CreateCommandHandlerContext(traceInfo), cancellationToken)
				: interceptor.InterceptExecuteAsync(traceInfo, hnd, (TCommand)command, options, cancellationToken);
		}

		public override void DisposeHandler(ICommandHandler? handler)
		{
			if (handler != null)
				_handlerFactory.Release(handler);
		}
	}
}

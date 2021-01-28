using Raider.Commands.Aspects;
using Raider.DependencyInjection;
using Raider.Exceptions;
using Raider.Trace;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Raider.Commands.Internal
{
	internal abstract class AsyncVoidCommandProcessor : CommandProcessorBase
	{
		public AsyncVoidCommandProcessor(ServiceFactory serviceFactory)
			: base(serviceFactory)
		{
		}

		public abstract Task<ICommandResult<bool>> CanExecuteAsync(
			ITraceInfo traceInfo,
			ICommandHandler handler,
			ICommand command,
			ICommandInterceptorOptions? options,
			CancellationToken cancellationToken);

		public abstract Task<ICommandResult> ExecuteAsync(
			ITraceInfo traceInfo,
			ICommandHandler handler,
			ICommand command,
			ICommandInterceptorOptions? options,
			CancellationToken cancellationToken);
	}

	internal class AsyncVoidCommandProcessor<TCommand> : AsyncVoidCommandProcessor
		where TCommand : ICommand
	{
		private readonly ICommandHandlerRegistry _handlerRegistry;
		private readonly ICommandHandlerFactory _handlerFactory;

		public AsyncVoidCommandProcessor(
			ICommandHandlerRegistry handlerRegistry,
			ICommandHandlerFactory handlerFactory,
			ServiceFactory serviceFactory)
			: base(serviceFactory)
		{
			_handlerRegistry = handlerRegistry ?? throw new ArgumentNullException(nameof(handlerRegistry));
			_handlerFactory = handlerFactory ?? throw new ArgumentNullException(nameof(handlerFactory));

			var _handlerType = _handlerRegistry.GetAsyncVoidCommandHandler<TCommand>();
			if (_handlerType == null)
				throw new ConfigurationException($"No asynchronous handler registered for command: {typeof(TCommand).FullName}");
		}

		public override ICommandHandler CreateHandler()
		{
			var handler = _handlerFactory.CreateAsyncVoidCommandHandler<TCommand>();
			if (handler == null)
				throw new InvalidOperationException($"Handler could not be created for type: {typeof(IAsyncCommandHandler<TCommand>).FullName}");

			return handler;
		}

		public override Task<ICommandResult<bool>> CanExecuteAsync(
			ITraceInfo traceInfo,
			ICommandHandler handler,
			ICommand command,
			ICommandInterceptorOptions? options,
			CancellationToken cancellationToken)
		{
			var hnd = (IAsyncCommandHandler<TCommand>)handler;

			IAsyncCommandInterceptor<TCommand>? interceptor = null;
			if (hnd.InterceptorType != null)
			{
				if (!typeof(IAsyncCommandInterceptor<TCommand>).IsAssignableFrom(hnd.InterceptorType))
					throw new InvalidOperationException($"Handler {hnd.GetType().FullName} has invalid {nameof(hnd.InterceptorType)}. {hnd.InterceptorType.FullName} must implement {typeof(IAsyncCommandInterceptor<TCommand>).FullName}");

				interceptor = (IAsyncCommandInterceptor<TCommand>?)hnd.ServiceFactory.GetRequiredInstance(hnd.InterceptorType);
			}

			return interceptor == null
				? hnd.CanExecuteAsync((TCommand)command, CreateCommandHandlerContext(traceInfo), cancellationToken)
				: interceptor.InterceptCanExecuteAsync(traceInfo, hnd, (TCommand)command, options, cancellationToken);
		}

		public override Task<ICommandResult> ExecuteAsync(
			ITraceInfo traceInfo,
			ICommandHandler handler,
			ICommand command,
			ICommandInterceptorOptions? options,
			CancellationToken cancellationToken)
		{
			var hnd = (IAsyncCommandHandler<TCommand>)handler;

			IAsyncCommandInterceptor<TCommand>? interceptor = null;
			if (hnd.InterceptorType != null)
			{
				if (!typeof(IAsyncCommandInterceptor<TCommand>).IsAssignableFrom(hnd.InterceptorType))
					throw new InvalidOperationException($"Handler {hnd.GetType().FullName} has invalid {nameof(hnd.InterceptorType)}. {hnd.InterceptorType.FullName} must implement {typeof(IAsyncCommandInterceptor<TCommand>).FullName}");

				interceptor = (IAsyncCommandInterceptor<TCommand>?)hnd.ServiceFactory.GetRequiredInstance(hnd.InterceptorType);
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

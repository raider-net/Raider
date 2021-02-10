using Raider.Commands.Aspects;
using Raider.DependencyInjection;
using Raider.Exceptions;
using Raider.Localization;
using Raider.Trace;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Raider.Commands.Internal
{
	internal abstract class AsyncVoidCommandProcessor : CommandProcessorBase
	{
		public AsyncVoidCommandProcessor()
			: base()
		{
		}

		public abstract Task<ICommandResult<bool>> CanExecuteAsync(
			ITraceInfo traceInfo,
			ICommandHandler handler,
			ICommand command,
			ICommandInterceptorOptions? options,
			IApplicationContext applicationContext,
			IApplicationResources applicationResources,
			CancellationToken cancellationToken);

		public abstract Task<ICommandResult> ExecuteAsync(
			ITraceInfo traceInfo,
			ICommandHandler handler,
			ICommand command,
			ICommandInterceptorOptions? options,
			IApplicationContext applicationContext,
			IApplicationResources applicationResources,
			CancellationToken cancellationToken);
	}

	internal class AsyncVoidCommandProcessor<TCommand> : AsyncVoidCommandProcessor
		where TCommand : ICommand
	{
		private readonly ICommandHandlerRegistry _handlerRegistry;

		public AsyncVoidCommandProcessor(
			ICommandHandlerRegistry handlerRegistry)
			: base()
		{
			_handlerRegistry = handlerRegistry ?? throw new ArgumentNullException(nameof(handlerRegistry));

			var _handlerType = _handlerRegistry.GetAsyncVoidCommandHandler<TCommand>();
			if (_handlerType == null)
				throw new ConfigurationException($"No asynchronous handler registered for command: {typeof(TCommand).FullName}");
		}

		public override ICommandHandler CreateHandler(ICommandHandlerFactory handlerFactory)
		{
			if (handlerFactory == null)
				throw new ArgumentNullException(nameof(handlerFactory));

			var handler = handlerFactory.CreateAsyncVoidCommandHandler<TCommand>();
			if (handler == null)
				throw new InvalidOperationException($"Handler could not be created for type: {typeof(IAsyncCommandHandler<TCommand>).FullName}");

			return handler;
		}

		public override Task<ICommandResult<bool>> CanExecuteAsync(
			ITraceInfo traceInfo,
			ICommandHandler handler,
			ICommand command,
			ICommandInterceptorOptions? options,
			IApplicationContext applicationContext,
			IApplicationResources applicationResources,
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
				? hnd.CanExecuteAsync((TCommand)command, CreateCommandHandlerContext(traceInfo, applicationContext, applicationResources), cancellationToken)
				: interceptor.InterceptCanExecuteAsync(traceInfo, hnd, (TCommand)command, options, cancellationToken);
		}

		public override Task<ICommandResult> ExecuteAsync(
			ITraceInfo traceInfo,
			ICommandHandler handler,
			ICommand command,
			ICommandInterceptorOptions? options,
			IApplicationContext applicationContext,
			IApplicationResources applicationResources,
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
				? hnd.ExecuteAsync((TCommand)command, CreateCommandHandlerContext(traceInfo, applicationContext, applicationResources), cancellationToken)
				: interceptor.InterceptExecuteAsync(traceInfo, hnd, (TCommand)command, options, cancellationToken);
		}

		public override void DisposeHandler(ICommandHandlerFactory handlerFactory, ICommandHandler? handler)
		{
			if (handler != null)
			{
				if (handlerFactory == null)
					throw new ArgumentNullException(nameof(handlerFactory));

				handlerFactory.Release(handler);
			}
		}
	}
}

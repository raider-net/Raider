using Microsoft.Extensions.DependencyInjection;
using Raider.Commands.Aspects;
using Raider.Exceptions;
using Raider.Trace;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Raider.Commands.Internal
{
	internal abstract class AsyncCommandProcessor<TResult> : CommandProcessorBase
	{
		public AsyncCommandProcessor()
			: base()
		{
		}

		public abstract Task<ICommandResult<bool>> CanExecuteAsync(
			ITraceInfo traceInfo,
			ICommandHandler handler,
			ICommand<TResult> command,
			ICommandInterceptorOptions? options,
			IApplicationContext applicationContext,
			CancellationToken cancellationToken);

		public abstract Task<ICommandResult<TResult>> ExecuteAsync(
			ITraceInfo traceInfo,
			ICommandHandler handler,
			ICommand<TResult> command,
			ICommandInterceptorOptions? options,
			IApplicationContext applicationContext,
			CancellationToken cancellationToken);
	}

	internal class AsyncCommandProcessor<TCommand, TResult> : AsyncCommandProcessor<TResult>
		where TCommand : ICommand<TResult>
	{
		private readonly ICommandHandlerRegistry _handlerRegistry;

		public AsyncCommandProcessor(ICommandHandlerRegistry handlerRegistry)
			: base()
		{
			_handlerRegistry = handlerRegistry ?? throw new ArgumentNullException(nameof(handlerRegistry));

			var _handlerType = _handlerRegistry.GetAsyncCommandHandler<TCommand, TResult>();
			if (_handlerType == null)
				throw new ConfigurationException($"No asynchronous handler registered for command: {typeof(TCommand).FullName}");
		}

		public override ICommandHandler CreateHandler(ICommandHandlerFactory handlerFactory)
		{
			if (handlerFactory == null)
				throw new ArgumentNullException(nameof(handlerFactory));

			var handler = handlerFactory.CreateAsyncCommandHandler<TCommand, TResult>();
			if (handler == null)
				throw new InvalidOperationException($"Handler could not be created for type: {typeof(IAsyncCommandHandler<TCommand, TResult>).FullName}");

			return handler;
		}

		public override Task<ICommandResult<bool>> CanExecuteAsync(
			ITraceInfo traceInfo,
			ICommandHandler handler,
			ICommand<TResult> command,
			ICommandInterceptorOptions? options,
			IApplicationContext applicationContext,
			CancellationToken cancellationToken)
		{
			var hnd = (IAsyncCommandHandler<TCommand, TResult>)handler;

			IAsyncCommandInterceptor<TCommand, TResult>? interceptor = null;
			if (hnd.InterceptorType != null)
			{
				if (!typeof(IAsyncCommandInterceptor<TCommand, TResult>).IsAssignableFrom(hnd.InterceptorType))
					throw new InvalidOperationException($"Handler {hnd.GetType().FullName} has invalid {nameof(hnd.InterceptorType)}. {hnd.InterceptorType.FullName} must implement {typeof(IAsyncCommandInterceptor<TCommand, TResult>).FullName}");

				interceptor = (IAsyncCommandInterceptor<TCommand, TResult>?)hnd.ServiceProvider.GetRequiredService(hnd.InterceptorType);
			}

			return interceptor == null
				? hnd.CanExecuteAsync((TCommand)command, CreateCommandHandlerContext(traceInfo, applicationContext), cancellationToken)
				: interceptor.InterceptCanExecuteAsync(traceInfo, hnd, (TCommand)command, options, cancellationToken);
		}

		public override Task<ICommandResult<TResult>> ExecuteAsync(
			ITraceInfo traceInfo,
			ICommandHandler handler,
			ICommand<TResult> command,
			ICommandInterceptorOptions? options,
			IApplicationContext applicationContext,
			CancellationToken cancellationToken)
		{
			var hnd = (IAsyncCommandHandler<TCommand, TResult>)handler;

			IAsyncCommandInterceptor<TCommand, TResult>? interceptor = null;
			if (hnd.InterceptorType != null)
			{
				if (!typeof(IAsyncCommandInterceptor<TCommand, TResult>).IsAssignableFrom(hnd.InterceptorType))
					throw new InvalidOperationException($"Handler {hnd.GetType().FullName} has invalid {nameof(hnd.InterceptorType)}. {hnd.InterceptorType.FullName} must implement {typeof(IAsyncCommandInterceptor<TCommand, TResult>).FullName}");

				interceptor = (IAsyncCommandInterceptor<TCommand, TResult>?)hnd.ServiceProvider.GetRequiredService(hnd.InterceptorType);
			}

			return interceptor == null
				? hnd.ExecuteAsync((TCommand)command, CreateCommandHandlerContext(traceInfo, applicationContext), cancellationToken)
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

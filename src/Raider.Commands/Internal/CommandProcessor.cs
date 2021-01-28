using Raider.Commands.Aspects;
using Raider.DependencyInjection;
using Raider.Exceptions;
using Raider.Trace;
using System;

namespace Raider.Commands.Internal
{
	internal abstract class CommandProcessor<TResult> : CommandProcessorBase
	{
		public CommandProcessor(ServiceFactory serviceFactory)
			: base(serviceFactory)
		{
		}

		public abstract ICommandResult<bool> CanExecute(
			ITraceInfo traceInfo,
			ICommandHandler handler,
			ICommand<TResult> command,
			ICommandInterceptorOptions? options);

		public abstract ICommandResult<TResult> Execute(
			ITraceInfo traceInfo,
			ICommandHandler handler,
			ICommand<TResult> command,
			ICommandInterceptorOptions? options);
	}

	internal class CommandProcessor<TCommand, TResult> : CommandProcessor<TResult>
		where TCommand : ICommand<TResult>
	{
		private readonly ICommandHandlerRegistry _handlerRegistry;
		private readonly ICommandHandlerFactory _handlerFactory;

		public CommandProcessor(
			ICommandHandlerRegistry handlerRegistry,
			ICommandHandlerFactory handlerFactory,
			ServiceFactory serviceFactory)
			: base(serviceFactory)
		{
			_handlerRegistry = handlerRegistry ?? throw new ArgumentNullException(nameof(handlerRegistry));
			_handlerFactory = handlerFactory ?? throw new ArgumentNullException(nameof(handlerFactory));

			var _handlerType = _handlerRegistry.GetCommandHandler<TCommand, TResult>();
			if (_handlerType == null)
				throw new ConfigurationException($"No synchronous handler registered for command: {typeof(TCommand).FullName}");
		}

		public override ICommandHandler CreateHandler()
		{
			var handler = _handlerFactory.CreateCommandHandler<TCommand, TResult>();
			if (handler == null)
				throw new InvalidOperationException($"Handler could not be created for type: {typeof(ICommandHandler<TCommand, TResult>).FullName}");

			return handler;
		}

		public override ICommandResult<bool> CanExecute(
			ITraceInfo traceInfo,
			ICommandHandler handler,
			ICommand<TResult> command,
			ICommandInterceptorOptions? options)
		{
			var hnd = (ICommandHandler<TCommand, TResult>)handler;

			ICommandInterceptor<TCommand, TResult>? interceptor = null;
			if (hnd.InterceptorType != null)
			{
				if (!typeof(ICommandInterceptor<TCommand, TResult>).IsAssignableFrom(hnd.InterceptorType))
					throw new InvalidOperationException($"Handler {hnd.GetType().FullName} has invalid {nameof(hnd.InterceptorType)}. {hnd.InterceptorType.FullName} must implement {typeof(ICommandInterceptor<TCommand, TResult>).FullName}");

				interceptor = (ICommandInterceptor<TCommand, TResult>?)hnd.ServiceFactory.GetRequiredInstance(hnd.InterceptorType);
			}

			return interceptor == null
				? hnd.CanExecute((TCommand)command, CreateCommandHandlerContext(traceInfo))
				: interceptor.InterceptCanExecute(traceInfo, hnd, (TCommand)command, options);
		}

		public override ICommandResult<TResult> Execute(
			ITraceInfo traceInfo,
			ICommandHandler handler,
			ICommand<TResult> command,
			ICommandInterceptorOptions? options)
		{
			var hnd = (ICommandHandler<TCommand, TResult>)handler;

			ICommandInterceptor<TCommand, TResult>? interceptor = null;
			if (hnd.InterceptorType != null)
			{
				if (!typeof(ICommandInterceptor<TCommand, TResult>).IsAssignableFrom(hnd.InterceptorType))
					throw new InvalidOperationException($"Handler {hnd.GetType().FullName} has invalid {nameof(hnd.InterceptorType)}. {hnd.InterceptorType.FullName} must implement {typeof(ICommandInterceptor<TCommand, TResult>).FullName}");

				interceptor = (ICommandInterceptor<TCommand, TResult>?)hnd.ServiceFactory.GetRequiredInstance(hnd.InterceptorType);
			}

			return interceptor == null
				? hnd.Execute((TCommand)command, CreateCommandHandlerContext(traceInfo))
				: interceptor.InterceptExecute(traceInfo, hnd, (TCommand)command, options);
		}

		public override void DisposeHandler(ICommandHandler? handler)
		{
			if (handler != null)
				_handlerFactory.Release(handler);
		}
	}
}

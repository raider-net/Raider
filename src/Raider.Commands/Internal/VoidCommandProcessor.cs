using Raider.Commands.Aspects;
using Raider.DependencyInjection;
using Raider.Exceptions;
using Raider.Localization;
using Raider.Trace;
using System;

namespace Raider.Commands.Internal
{
	internal abstract class VoidCommandProcessor : CommandProcessorBase
	{
		public VoidCommandProcessor()
			: base()
		{
		}

		public abstract ICommandResult<bool> CanExecute(
			ITraceInfo traceInfo,
			ICommandHandler handler,
			ICommand command,
			ICommandInterceptorOptions? options,
			IApplicationContext applicationContext,
			IApplicationResources applicationResources);

		public abstract ICommandResult Execute(
			ITraceInfo traceInfo,
			ICommandHandler handler,
			ICommand command,
			ICommandInterceptorOptions? options,
			IApplicationContext applicationContext,
			IApplicationResources applicationResources);
	}

	internal class VoidCommandProcessor<TCommand> : VoidCommandProcessor
		where TCommand : ICommand
	{
		private readonly ICommandHandlerRegistry _handlerRegistry;

		public VoidCommandProcessor(
			ICommandHandlerRegistry handlerRegistry)
			: base()
		{
			_handlerRegistry = handlerRegistry ?? throw new ArgumentNullException(nameof(handlerRegistry));

			var _handlerType = _handlerRegistry.GetVoidCommandHandler<TCommand>();
			if (_handlerType == null)
				throw new ConfigurationException($"No synchronous handler registered for command: {typeof(TCommand).FullName}");
		}

		public override ICommandHandler CreateHandler(ICommandHandlerFactory handlerFactory)
		{
			if (handlerFactory == null)
				throw new ArgumentNullException(nameof(handlerFactory));

			var handler = handlerFactory.CreateVoidCommandHandler<TCommand>();
			if (handler == null)
				throw new InvalidOperationException($"Handler could not be created for type: {typeof(ICommandHandler<TCommand>).FullName}");

			return handler;
		}

		public override ICommandResult<bool> CanExecute(
			ITraceInfo traceInfo,
			ICommandHandler handler,
			ICommand command,
			ICommandInterceptorOptions? options,
			IApplicationContext applicationContext,
			IApplicationResources applicationResources)
		{
			var hnd = (ICommandHandler<TCommand>)handler;

			ICommandInterceptor<TCommand>? interceptor = null;
			if (hnd.InterceptorType != null)
			{
				if (!typeof(ICommandInterceptor<TCommand>).IsAssignableFrom(hnd.InterceptorType))
					throw new InvalidOperationException($"Handler {hnd.GetType().FullName} has invalid {nameof(hnd.InterceptorType)}. {hnd.InterceptorType.FullName} must implement {typeof(ICommandInterceptor<TCommand>).FullName}");

				interceptor = (ICommandInterceptor<TCommand>?)hnd.ServiceFactory.GetRequiredInstance(hnd.InterceptorType);
			}

			return interceptor == null
				? hnd.CanExecute((TCommand)command, CreateCommandHandlerContext(traceInfo, applicationContext, applicationResources))
				: interceptor.InterceptCanExecute(traceInfo, hnd, (TCommand)command, options);
		}

		public override ICommandResult Execute(
			ITraceInfo traceInfo,
			ICommandHandler handler,
			ICommand command,
			ICommandInterceptorOptions? options,
			IApplicationContext applicationContext,
			IApplicationResources applicationResources)
		{
			var hnd = (ICommandHandler<TCommand>)handler;

			ICommandInterceptor<TCommand>? interceptor = null;
			if (hnd.InterceptorType != null)
			{
				if (!typeof(ICommandInterceptor<TCommand>).IsAssignableFrom(hnd.InterceptorType))
					throw new InvalidOperationException($"Handler {hnd.GetType().FullName} has invalid {nameof(hnd.InterceptorType)}. {hnd.InterceptorType.FullName} must implement {typeof(ICommandInterceptor<TCommand>).FullName}");

				interceptor = (ICommandInterceptor<TCommand>?)hnd.ServiceFactory.GetRequiredInstance(hnd.InterceptorType);
			}

			return interceptor == null
				? hnd.Execute((TCommand)command, CreateCommandHandlerContext(traceInfo, applicationContext, applicationResources))
				: interceptor.InterceptExecute(traceInfo, hnd, (TCommand)command, options);
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

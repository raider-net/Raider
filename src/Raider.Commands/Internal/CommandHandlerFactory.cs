using Raider.DependencyInjection;
using System;

namespace Raider.Commands.Internal
{
	public class CommandHandlerFactory : ICommandHandlerFactory
	{
		private readonly ServiceFactory _serviceFactory;

		public CommandHandlerFactory(ServiceFactory serviceFactory)
		{
			_serviceFactory = serviceFactory ?? throw new ArgumentNullException(nameof(serviceFactory));
		}

		public ICommandHandler<TCommand>? CreateVoidCommandHandler<TCommand>()
			where TCommand : ICommand
		{
			var handler = _serviceFactory.GetInstance<ICommandHandler<TCommand>>();
			if (handler != null)
				handler.ServiceFactory = _serviceFactory.GetRequiredInstance<ServiceFactory>();

			return handler;
		}

		public IAsyncCommandHandler<TCommand>? CreateAsyncVoidCommandHandler<TCommand>()
			where TCommand : ICommand
		{
			var handler = _serviceFactory.GetInstance<IAsyncCommandHandler<TCommand>>();
			if (handler != null)
				handler.ServiceFactory = _serviceFactory.GetRequiredInstance<ServiceFactory>();

			return handler;
		}

		public ICommandHandler<TCommand, TResult>? CreateCommandHandler<TCommand, TResult>()
			where TCommand : ICommand<TResult>
		{
			var handler = _serviceFactory.GetInstance<ICommandHandler<TCommand, TResult>>();
			if (handler != null)
				handler.ServiceFactory = _serviceFactory.GetRequiredInstance<ServiceFactory>();

			return handler;
		}

		public IAsyncCommandHandler<TCommand, TResult>? CreateAsyncCommandHandler<TCommand, TResult>()
			where TCommand : ICommand<TResult>
		{
			var handler = _serviceFactory.GetInstance<IAsyncCommandHandler<TCommand, TResult>>();
			if (handler != null)
				handler.ServiceFactory = _serviceFactory.GetRequiredInstance<ServiceFactory>();

			return handler;
		}

		public void Release(ICommandHandler? handler)
		{
			var disposal = handler as IDisposable;
			disposal?.Dispose();
		}
	}
}

using Microsoft.Extensions.DependencyInjection;
using System;

namespace Raider.Commands.Internal
{
	public class CommandHandlerFactory : ICommandHandlerFactory
	{
		private readonly IServiceProvider _serviceProvider;

		public CommandHandlerFactory(IServiceProvider serviceProvider)
		{
			_serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
		}

		public ICommandHandler<TCommand>? CreateVoidCommandHandler<TCommand>()
			where TCommand : ICommand
		{
			var handler = _serviceProvider.GetService<ICommandHandler<TCommand>>();
			if (handler != null)
				handler.ServiceProvider = _serviceProvider;

			return handler;
		}

		public IAsyncCommandHandler<TCommand>? CreateAsyncVoidCommandHandler<TCommand>()
			where TCommand : ICommand
		{
			var handler = _serviceProvider.GetService<IAsyncCommandHandler<TCommand>>();
			if (handler != null)
				handler.ServiceProvider = _serviceProvider;

			return handler;
		}

		public ICommandHandler<TCommand, TResult>? CreateCommandHandler<TCommand, TResult>()
			where TCommand : ICommand<TResult>
		{
			var handler = _serviceProvider.GetService<ICommandHandler<TCommand, TResult>>();
			if (handler != null)
				handler.ServiceProvider = _serviceProvider;

			return handler;
		}

		public IAsyncCommandHandler<TCommand, TResult>? CreateAsyncCommandHandler<TCommand, TResult>()
			where TCommand : ICommand<TResult>
		{
			var handler = _serviceProvider.GetService<IAsyncCommandHandler<TCommand, TResult>>();
			if (handler != null)
				handler.ServiceProvider = _serviceProvider;

			return handler;
		}

		public void Release(ICommandHandler? handler)
		{
			var disposal = handler as IDisposable;
			disposal?.Dispose();
		}
	}
}

using Microsoft.Extensions.DependencyInjection;
using Raider.Exceptions;
using System;
using System.Collections.Generic;

namespace Raider.Commands.Internal
{
	internal class CommandHandlerRegistry : ICommandHandlerRegistry
	{
		private static readonly Type _iVoidCommandHandlerTypeDefinition = typeof(ICommandHandler<>);
		private static readonly Type _iAsyncVoidCommandHandlerTypeDefinition = typeof(IAsyncCommandHandler<>);
		private static readonly Type _iCommandHandlerTypeDefinition = typeof(ICommandHandler<,>);
		private static readonly Type _iAsyncCommandHandlerTypeDefinition = typeof(IAsyncCommandHandler<,>);

		private readonly IServiceCollection _services;
		private readonly ServiceLifetime _lifetime;
		private readonly IDictionary<Type, Type> _voidCommandHandlersRegistry; //IDictionary<commandType, handlerType>
		private readonly IDictionary<Type, Type> _asyncVoidCommandHandlersRegistry; //IDictionary<commandType, handlerType>
		private readonly IDictionary<Type, Type> _commandHandlersRegistry; //IDictionary<commandType, handlerType>
		private readonly IDictionary<Type, Type> _asyncCommandHandlersRegistry; //IDictionary<commandType, handlerType>

		public CommandHandlerRegistry(IServiceCollection services, ServiceLifetime lifetime = ServiceLifetime.Transient)
		{
			_services = services ?? throw new ArgumentNullException(nameof(services));
			_lifetime = lifetime;
			_voidCommandHandlersRegistry = new Dictionary<Type, Type>();
			_asyncVoidCommandHandlersRegistry = new Dictionary<Type, Type>();
			_commandHandlersRegistry = new Dictionary<Type, Type>();
			_asyncCommandHandlersRegistry = new Dictionary<Type, Type>();
		}

		public bool TryRegisterHandler(Type handlerType)
		{
			if (handlerType == null)
				throw new ArgumentNullException(nameof(handlerType));

			var interfaces = handlerType.GetInterfaces();
			if (interfaces != null)
			{
				foreach (var ifc in interfaces)
				{
					if (ifc.IsGenericType)
					{
						if (ifc.GenericTypeArguments.Length == 1)
						{
							if (_iVoidCommandHandlerTypeDefinition.IsAssignableFrom(ifc.GetGenericTypeDefinition()))
							{
								RegisterVoidCommandHandler(ifc.GenericTypeArguments[0], handlerType);
								return true;
							}
							else if (_iAsyncVoidCommandHandlerTypeDefinition.IsAssignableFrom(ifc.GetGenericTypeDefinition()))
							{
								RegisterAsyncVoidCommandHandler(ifc.GenericTypeArguments[0], handlerType);
								return true;
							}
						}
						else if (ifc.GenericTypeArguments.Length == 2)
						{
							if (_iCommandHandlerTypeDefinition.IsAssignableFrom(ifc.GetGenericTypeDefinition()))
							{
								RegisterCommandHandler(ifc.GenericTypeArguments[0], ifc.GenericTypeArguments[1], handlerType);
								return true;
							}
							else if (_iAsyncCommandHandlerTypeDefinition.IsAssignableFrom(ifc.GetGenericTypeDefinition()))
							{
								RegisterAsyncCommandHandler(ifc.GenericTypeArguments[0], ifc.GenericTypeArguments[1], handlerType);
								return true;
							}
						}
					}
				}
			}

			return false;
		}

		public Type? GetVoidCommandHandler<TCommand>()
			where TCommand : ICommand
		{
			return GetVoidCommandHandler(typeof(TCommand));
		}

		public Type? GetVoidCommandHandler(Type commandType)
		{
			if (commandType == null)
				throw new ArgumentNullException(nameof(commandType));

			_voidCommandHandlersRegistry.TryGetValue(commandType, out Type? handlerType);
			return handlerType;
		}

		public void RegisterVoidCommandHandler<THandler>()
		{
			RegisterVoidCommandHandler(typeof(THandler));
		}

		public void RegisterVoidCommandHandler(Type handlerType)
		{
			if (handlerType == null)
				throw new ArgumentNullException(nameof(handlerType));

			var iVoidCommandHandlerTypeDefinition = typeof(ICommandHandler<>);

			var interfaces = handlerType.GetInterfaces();
			if (interfaces != null)
			{
				foreach (var ifc in interfaces)
				{
					if (ifc.IsGenericType
						&& ifc.GenericTypeArguments.Length == 1
						&& iVoidCommandHandlerTypeDefinition.IsAssignableFrom(ifc.GetGenericTypeDefinition()))
					{
						RegisterVoidCommandHandler(ifc.GenericTypeArguments[0], handlerType);
						return;
					}
				}
			}

			throw new ConfigurationException($"Invalid synchronous command handler type {handlerType.FullName}");
		}

		public void RegisterVoidCommandHandler<TCommand, THandler>()
			where TCommand : ICommand
			where THandler : ICommandHandler<TCommand>
		{
			RegisterVoidCommandHandler(typeof(TCommand), typeof(THandler));
		}

		public void RegisterVoidCommandHandler(Type commandType, Type handlerType)
		{
			if (commandType == null)
				throw new ArgumentNullException(nameof(commandType));
			if (handlerType == null)
				throw new ArgumentNullException(nameof(handlerType));

			if (_voidCommandHandlersRegistry.ContainsKey(commandType))
			{
				var registeredHandlerType = GetVoidCommandHandler(commandType);
				throw new ConfigurationException($"Command type {commandType.FullName} is already registered to {registeredHandlerType?.FullName ?? "--NULL--"} Cannot be registered to {handlerType.FullName}");
			}

			var iCommandType = typeof(ICommand);
			if (!iCommandType.IsAssignableFrom(commandType))
				throw new ConfigurationException($"Command type {commandType.FullName} must implement {iCommandType.FullName}");

			var iCommandHandlerType = typeof(ICommandHandler<>).MakeGenericType(commandType);
			if (!iCommandHandlerType.IsAssignableFrom(handlerType))
				throw new ConfigurationException($"Handler type {handlerType.FullName} must implement {iCommandHandlerType.FullName}");

			_voidCommandHandlersRegistry.Add(commandType, handlerType);

			_services.Add(new ServiceDescriptor(iCommandHandlerType, handlerType, _lifetime));
		}

		public Type? GetAsyncVoidCommandHandler<TCommand>()
			where TCommand : ICommand
		{
			return GetAsyncVoidCommandHandler(typeof(TCommand));
		}

		public Type? GetAsyncVoidCommandHandler(Type commandType)
		{
			if (commandType == null)
				throw new ArgumentNullException(nameof(commandType));

			_asyncVoidCommandHandlersRegistry.TryGetValue(commandType, out Type? handlerType);
			return handlerType;
		}

		public void RegisterAsyncVoidCommandHandler<THandler>()
		{
			RegisterAsyncVoidCommandHandler(typeof(THandler));
		}

		public void RegisterAsyncVoidCommandHandler(Type handlerType)
		{
			if (handlerType == null)
				throw new ArgumentNullException(nameof(handlerType));

			var interfaces = handlerType.GetInterfaces();
			if (interfaces != null)
			{
				foreach (var ifc in interfaces)
				{
					if (ifc.IsGenericType
						&& ifc.GenericTypeArguments.Length == 1
						&& _iAsyncVoidCommandHandlerTypeDefinition.IsAssignableFrom(ifc.GetGenericTypeDefinition()))
					{
						RegisterAsyncVoidCommandHandler(ifc.GenericTypeArguments[0], handlerType);
						return;
					}
				}
			}

			throw new ConfigurationException($"Invalid asynchronous command handler type {handlerType.FullName}");
		}

		public void RegisterAsyncVoidCommandHandler<TCommand, THandler>()
			where TCommand : ICommand
			where THandler : IAsyncCommandHandler<TCommand>
		{
			RegisterAsyncVoidCommandHandler(typeof(TCommand), typeof(THandler));
		}

		public void RegisterAsyncVoidCommandHandler(Type commandType, Type handlerType)
		{
			if (commandType == null)
				throw new ArgumentNullException(nameof(commandType));
			if (handlerType == null)
				throw new ArgumentNullException(nameof(handlerType));

			if (_asyncVoidCommandHandlersRegistry.ContainsKey(commandType))
			{
				var registeredHandlerType = GetAsyncVoidCommandHandler(commandType);
				throw new ConfigurationException($"Command type {commandType.FullName} is already registered to {registeredHandlerType?.FullName ?? "--NULL--"} Cannot be registered to {handlerType.FullName}");
			}

			var iCommandType = typeof(ICommand);
			if (!iCommandType.IsAssignableFrom(commandType))
				throw new ConfigurationException($"Command type {commandType.FullName} must implement {iCommandType.FullName}");

			var iCommandHandlerType = typeof(IAsyncCommandHandler<>).MakeGenericType(commandType);
			if (!iCommandHandlerType.IsAssignableFrom(handlerType))
				throw new ConfigurationException($"Handler type {handlerType.FullName} must implement {iCommandHandlerType.FullName}");

			_asyncVoidCommandHandlersRegistry.Add(commandType, handlerType);

			_services.Add(new ServiceDescriptor(iCommandHandlerType, handlerType, _lifetime));
		}

		public Type? GetCommandHandler<TCommand, TResult>()
			where TCommand : ICommand<TResult>
		{
			return GetCommandHandler(typeof(TCommand));
		}

		public Type? GetCommandHandler(Type commandType)
		{
			if (commandType == null)
				throw new ArgumentNullException(nameof(commandType));

			_commandHandlersRegistry.TryGetValue(commandType, out Type? handlerType);
			return handlerType;
		}

		public void RegisterCommandHandler<THandler>()
		{
			RegisterCommandHandler(typeof(THandler));
		}

		public void RegisterCommandHandler(Type handlerType)
		{
			if (handlerType == null)
				throw new ArgumentNullException(nameof(handlerType));

			var interfaces = handlerType.GetInterfaces();
			if (interfaces != null)
			{
				foreach (var ifc in interfaces)
				{
					if (ifc.IsGenericType
						&& ifc.GenericTypeArguments.Length == 2
						&& _iCommandHandlerTypeDefinition.IsAssignableFrom(ifc.GetGenericTypeDefinition()))
					{
						RegisterCommandHandler(ifc.GenericTypeArguments[0], ifc.GenericTypeArguments[1], handlerType);
						return;
					}
				}
			}

			throw new ConfigurationException($"Invalid synchronous command handler type {handlerType.FullName}");
		}

		public void RegisterCommandHandler<TCommand, TResult, THandler>()
			where TCommand : ICommand<TResult>
			where THandler : ICommandHandler<TCommand, TResult>
		{
			RegisterCommandHandler(typeof(TCommand), typeof(TResult), typeof(THandler));
		}

		public void RegisterCommandHandler(Type commandType, Type resultType, Type handlerType)
		{
			if (commandType == null)
				throw new ArgumentNullException(nameof(commandType));
			if (resultType == null)
				throw new ArgumentNullException(nameof(resultType));
			if (handlerType == null)
				throw new ArgumentNullException(nameof(handlerType));

			if (_commandHandlersRegistry.ContainsKey(commandType))
			{
				var registeredHandlerType = GetCommandHandler(commandType);
				throw new ConfigurationException($"Command type {commandType.FullName} is already registered to {registeredHandlerType?.FullName ?? "--NULL--"} Cannot be registered to {handlerType.FullName}");
			}

			var iCommandType = typeof(ICommand<>).MakeGenericType(resultType);
			if (!iCommandType.IsAssignableFrom(commandType))
				throw new ConfigurationException($"Command type {commandType.FullName} must implement {iCommandType.FullName}");

			var iCommandHandlerType = typeof(ICommandHandler<,>).MakeGenericType(commandType, resultType);
			if (!iCommandHandlerType.IsAssignableFrom(handlerType))
				throw new ConfigurationException($"Handler type {handlerType.FullName} must implement {iCommandHandlerType.FullName}");

			_commandHandlersRegistry.Add(commandType, handlerType);

			_services.Add(new ServiceDescriptor(iCommandHandlerType, handlerType, _lifetime));
		}

		public Type? GetAsyncCommandHandler<TCommand, TResult>()
			where TCommand : ICommand<TResult>
		{
			return GetAsyncCommandHandler(typeof(TCommand));
		}

		public Type? GetAsyncCommandHandler(Type commandType)
		{
			if (commandType == null)
				throw new ArgumentNullException(nameof(commandType));

			_asyncCommandHandlersRegistry.TryGetValue(commandType, out Type? handlerType);
			return handlerType;
		}

		public void RegisterAsyncCommandHandler<THandler>()
		{
			RegisterAsyncCommandHandler(typeof(THandler));
		}

		public void RegisterAsyncCommandHandler(Type handlerType)
		{
			if (handlerType == null)
				throw new ArgumentNullException(nameof(handlerType));

			var interfaces = handlerType.GetInterfaces();
			if (interfaces != null)
			{
				foreach (var ifc in interfaces)
				{
					if (ifc.IsGenericType
						&& ifc.GenericTypeArguments.Length == 2
						&& _iAsyncCommandHandlerTypeDefinition.IsAssignableFrom(ifc.GetGenericTypeDefinition()))
					{
						RegisterAsyncCommandHandler(ifc.GenericTypeArguments[0], ifc.GenericTypeArguments[1], handlerType);
						return;
					}
				}
			}

			throw new ConfigurationException($"Invalid asynchronous command handler type {handlerType.FullName}");
		}

		public void RegisterAsyncCommandHandler<TCommand, TResult, THandler>()
			where TCommand : ICommand<TResult>
			where THandler : IAsyncCommandHandler<TCommand, TResult>
		{
			RegisterAsyncCommandHandler(typeof(TCommand), typeof(TResult), typeof(THandler));
		}

		public void RegisterAsyncCommandHandler(Type commandType, Type resultType, Type handlerType)
		{
			if (commandType == null)
				throw new ArgumentNullException(nameof(commandType));
			if (resultType == null)
				throw new ArgumentNullException(nameof(resultType));
			if (handlerType == null)
				throw new ArgumentNullException(nameof(handlerType));

			if (_asyncCommandHandlersRegistry.ContainsKey(commandType))
			{
				var registeredHandlerType = GetAsyncCommandHandler(commandType);
				throw new ConfigurationException($"Command type {commandType.FullName} is already registered to {registeredHandlerType?.FullName ?? "--NULL--"} Cannot be registered to {handlerType.FullName}");
			}

			var iCommandType = typeof(ICommand<>).MakeGenericType(resultType);
			if (!iCommandType.IsAssignableFrom(commandType))
				throw new ConfigurationException($"Command type {commandType.FullName} must implement {iCommandType.FullName}");

			var iCommandHandlerType = typeof(IAsyncCommandHandler<,>).MakeGenericType(commandType, resultType);
			if (!iCommandHandlerType.IsAssignableFrom(handlerType))
				throw new ConfigurationException($"Handler type {handlerType.FullName} must implement {iCommandHandlerType.FullName}");

			_asyncCommandHandlersRegistry.Add(commandType, handlerType);

			_services.Add(new ServiceDescriptor(iCommandHandlerType, handlerType, _lifetime));
		}
	}
}

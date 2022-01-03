using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Raider.Converters;
using Raider.Exceptions;
using Raider.Logging.Extensions;
using Raider.ServiceBus.Internal.Model;
using Raider.ServiceBus.Messages.Interceptors;
using Raider.ServiceBus.Model;
using Raider.ServiceBus.Resolver;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Raider.ServiceBus.Messages.Internal
{
	internal class MessageHandlerRegistry : IMessageHandlerRegistry, IMessageTypeRegistry
	{
		private static readonly Type _iRequestMessage = typeof(Messages.IRequestMessage<>);
		private static readonly Type _iVoidRequestMessage = typeof(Messages.IRequestMessage);
		private static readonly Type _iMessageHandlerTypeDefinition = typeof(IMessageHandler<,,>);
		private static readonly Type _iAsyncMessageHandlerTypeDefinition = typeof(IAsyncMessageHandler<,,>);
		private static readonly Type _iVoidMessageHandlerTypeDefinition = typeof(IMessageHandler<,>);
		private static readonly Type _iAsyncVoidMessageHandlerTypeDefinition = typeof(IAsyncMessageHandler<,>);

		private static readonly Type _iMessageHandlerInterceptorTypeDefinition = typeof(IMessageHandlerInterceptor<,,>);
		private static readonly Type _iAsyncMessageHandlerInterceptorTypeDefinition = typeof(IAsyncMessageHandlerInterceptor<,,>);
		private static readonly Type _iVoidMessageHandlerInterceptorTypeDefinition = typeof(IMessageHandlerInterceptor<,>);
		private static readonly Type _iAsyncVoidMessageHandlerInterceptorTypeDefinition = typeof(IAsyncMessageHandlerInterceptor<,>);

		private readonly IServiceCollection _services;
		private readonly ITypeResolver _typeResolver;
		private readonly Type _messageHandlerContextType;
		private readonly ILogger _logger;
		private readonly ServiceLifetime _handlerLifetime;
		private readonly ServiceLifetime _interceptorLifetime;
		private readonly ConcurrentDictionary<Type, Type> _messageHandlersRegistry; //ConcurrentDictionary<messageType, handlerType>
		private readonly ConcurrentDictionary<Type, Type> _asyncMessageHandlersRegistry; //ConcurrentDictionary<messageType, handlerType>
		private readonly ConcurrentDictionary<Type, Type> _voidMessageHandlersRegistry; //ConcurrentDictionary<messageType, handlerType>
		private readonly ConcurrentDictionary<Type, Type> _asyncVoidMessageHandlersRegistry; //ConcurrentDictionary<messageType, handlerType>

		private readonly ConcurrentDictionary<Type, DbMessageType> _types; //ConcurrentDictionary<crl_message_type, MessageType>
		private readonly ConcurrentDictionary<IMessageType, Type> _messageTypes; //ConcurrentDictionary<MessageType, crl_message_type>

		public MessageHandlerRegistry(
			IServiceCollection services,
			Type messageHandlerContextType,
			ITypeResolver typeResolver,
			ILogger logger,
			ServiceLifetime handlerLifetime = ServiceLifetime.Transient,
			ServiceLifetime interceptorLifetime = ServiceLifetime.Transient)
		{
			_services = services ?? throw new ArgumentNullException(nameof(services));
			_messageHandlerContextType = messageHandlerContextType ?? throw new ArgumentNullException(nameof(messageHandlerContextType));
			_typeResolver = typeResolver ?? throw new ArgumentNullException(nameof(typeResolver));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_handlerLifetime = handlerLifetime;
			_interceptorLifetime = interceptorLifetime;
			_messageHandlersRegistry = new ConcurrentDictionary<Type, Type>();
			_asyncMessageHandlersRegistry = new ConcurrentDictionary<Type, Type>();
			_voidMessageHandlersRegistry = new ConcurrentDictionary<Type, Type>();
			_asyncVoidMessageHandlersRegistry = new ConcurrentDictionary<Type, Type>();

			_types = new ConcurrentDictionary<Type, DbMessageType>();
			_messageTypes = new ConcurrentDictionary<IMessageType, Type>();
		}

		//public void RegisterAssemblyTypes(IEnumerable<Assembly> assembliesToScan)
		//{
		//	RegisterTypes(_iMessageHandlerTypeDefinition, assembliesToScan);
		//	RegisterTypes(_iAsyncMessageHandlerTypeDefinition, assembliesToScan);
		//	RegisterTypes(_iVoidMessageHandlerTypeDefinition, assembliesToScan);
		//	RegisterTypes(_iAsyncVoidMessageHandlerTypeDefinition, assembliesToScan);
		//}

		//private void RegisterTypes(Type openInterface, IEnumerable<Assembly> assembliesToScan)
		//{
		//	var types = TypeHelper.GetImplementationsToTypesClosingOpenInterface(openInterface, assembliesToScan, false);
		//	foreach (var kvp in types)
		//		_services.TryAddTransient(kvp.Key, kvp.Value);
		//}

		public IEnumerable<IMessageType>? GetAllMessageTypes()
			=> _types.Values.ToList();

		public IMessageType? GetMessageType(Type type)
		{
			_types.TryGetValue(type, out var messageType);
			return messageType;
		}

		public Type? GetType(IMessageType messageType)
		{
			_messageTypes.TryGetValue(messageType, out var type);
			return type;
		}

		public bool TryRegisterHandlerAndInterceptor(Type type)
		{
			if (type == null)
				throw new ArgumentNullException(nameof(type));

			var interfaces = type.GetInterfaces();
			if (interfaces == null)
				return false;

			foreach (var ifc in interfaces)
			{
				if (ifc.IsGenericType)
				{
					if (ifc.GenericTypeArguments.Length == 2)
					{
						if (_iVoidMessageHandlerTypeDefinition.IsAssignableFrom(ifc.GetGenericTypeDefinition()))
						{
							RegisterVoidMessageHandler(ifc.GenericTypeArguments[0], ifc.GenericTypeArguments[1], type);
							return true;
						}
						else if (_iAsyncVoidMessageHandlerTypeDefinition.IsAssignableFrom(ifc.GetGenericTypeDefinition()))
						{
							RegisterAsyncVoidMessageHandler(ifc.GenericTypeArguments[0], ifc.GenericTypeArguments[1], type);
							return true;
						}
						else if (_iVoidMessageHandlerInterceptorTypeDefinition.IsAssignableFrom(ifc.GetGenericTypeDefinition()))
						{
							RegisterVoidMessageHandlerInterceptor(ifc.GenericTypeArguments[0], ifc.GenericTypeArguments[1], type);
						}
						else if (_iAsyncVoidMessageHandlerInterceptorTypeDefinition.IsAssignableFrom(ifc.GetGenericTypeDefinition()))
						{
							RegisterAsyncVoidMessageHandlerInterceptor(ifc.GenericTypeArguments[0], ifc.GenericTypeArguments[1], type);
						}
					}
					else if (ifc.GenericTypeArguments.Length == 3)
					{
						if (_iMessageHandlerTypeDefinition.IsAssignableFrom(ifc.GetGenericTypeDefinition()))
						{
							RegisterMessageHandler(ifc.GenericTypeArguments[0], ifc.GenericTypeArguments[1], ifc.GenericTypeArguments[2], type);
							return true;
						}
						else if (_iAsyncMessageHandlerTypeDefinition.IsAssignableFrom(ifc.GetGenericTypeDefinition()))
						{
							RegisterAsyncMessageHandler(ifc.GenericTypeArguments[0], ifc.GenericTypeArguments[1], ifc.GenericTypeArguments[2], type);
							return true;
						}
						else if (_iMessageHandlerInterceptorTypeDefinition.IsAssignableFrom(ifc.GetGenericTypeDefinition()))
						{
							RegisterMessageHandlerInterceptor(ifc.GenericTypeArguments[0], ifc.GenericTypeArguments[1], ifc.GenericTypeArguments[2], type);
						}
						else if (_iAsyncMessageHandlerInterceptorTypeDefinition.IsAssignableFrom(ifc.GetGenericTypeDefinition()))
						{
							RegisterAsyncMessageHandlerInterceptor(ifc.GenericTypeArguments[0], ifc.GenericTypeArguments[1], ifc.GenericTypeArguments[2], type);
						}
					}
				}
			}

			return false;
		}



		public Type? GetMessageHandlerType<TRequestMessage, TResponse>()
			where TRequestMessage : Messages.IRequestMessage<TResponse>
		{
			return GetMessageHandlerType(typeof(TRequestMessage));
		}

		public Type? GetMessageHandlerType(Type messageType)
		{
			if (messageType == null)
				throw new ArgumentNullException(nameof(messageType));

			_messageHandlersRegistry.TryGetValue(messageType, out Type? handlerType);
			return handlerType;
		}

		public void RegisterMessageHandler<THandler>()
		{
			RegisterMessageHandler(typeof(THandler));
		}

		public void RegisterMessageHandler(Type handlerType)
		{
			if (handlerType == null)
				throw new ArgumentNullException(nameof(handlerType));

			var interfaces = handlerType.GetInterfaces();
			if (interfaces != null)
			{
				foreach (var ifc in interfaces)
				{
					if (ifc.IsGenericType
						&& ifc.GenericTypeArguments.Length == 3
						&& _iMessageHandlerTypeDefinition.IsAssignableFrom(ifc.GetGenericTypeDefinition()))
					{
						RegisterMessageHandler(ifc.GenericTypeArguments[0], ifc.GenericTypeArguments[1], ifc.GenericTypeArguments[2], handlerType);
						return;
					}
				}
			}

			throw new ConfigurationException($"Invalid synchronous message handler type {handlerType.FullName}");
		}

		public void RegisterMessageHandler<TRequestMessage, TResponse, THandler, TContext>()
			where TRequestMessage : Messages.IRequestMessage<TResponse>
			where THandler : IMessageHandler<TRequestMessage, TResponse, TContext>
			where TContext : IMessageHandlerContext
		{
			RegisterMessageHandler(typeof(TRequestMessage), typeof(TResponse), typeof(TContext), typeof(THandler));
		}

		public void RegisterMessageHandler(Type messageType, Type resposeType, Type contextType, Type handlerType)
		{
			if (messageType == null)
				throw new ArgumentNullException(nameof(messageType));
			if (resposeType == null)
				throw new ArgumentNullException(nameof(resposeType));
			if (contextType == null)
				throw new ArgumentNullException(nameof(contextType));
			if (handlerType == null)
				throw new ArgumentNullException(nameof(handlerType));

			if (_messageHandlerContextType != contextType)
				throw new ConfigurationException($"For handler {handlerType.FullName} is required context type {_messageHandlerContextType.FullName} but found {contextType.FullName}");

			if (_messageHandlersRegistry.TryGetValue(messageType, out var registeredHandlerType))
				throw new ConfigurationException($"Message type {messageType.FullName} is already registered to {registeredHandlerType?.FullName ?? "--NULL--"} Cannot be registered to {handlerType.FullName}");

			var iMessageType = _iRequestMessage.MakeGenericType(resposeType);
			if (!iMessageType.IsAssignableFrom(messageType))
				throw new ConfigurationException($"For handler {handlerType.FullName} the message type {messageType.FullName} must implement {iMessageType.FullName}");

			var iMessageHandlerType = _iMessageHandlerTypeDefinition.MakeGenericType(messageType, resposeType, contextType);
			if (!iMessageHandlerType.IsAssignableFrom(handlerType))
				throw new ConfigurationException($"Handler type {handlerType.FullName} must implement {iMessageHandlerType.FullName}");

			var added = _messageHandlersRegistry.TryAdd(messageType, handlerType);
			if (!added)
				throw new ConfigurationException($"Message type {messageType.FullName} is already registered. Cannot be registered to {handlerType.FullName}");

			AddMessageType(messageType, resposeType, MessageMetaType.RequestMessage_TResponse);

			_services.Add(new ServiceDescriptor(iMessageHandlerType, handlerType, _handlerLifetime));
		}

		public void RegisterMessageHandlerInterceptor(Type messageType, Type resposeType, Type contextType, Type interceptorType)
		{
			if (messageType == null)
				throw new ArgumentNullException(nameof(messageType));
			if (resposeType == null)
				throw new ArgumentNullException(nameof(resposeType));
			if (contextType == null)
				throw new ArgumentNullException(nameof(contextType));
			if (interceptorType == null)
				throw new ArgumentNullException(nameof(interceptorType));

			if (_messageHandlerContextType != contextType)
				throw new ConfigurationException($"For interceptor {interceptorType.FullName} is required context type {_messageHandlerContextType.FullName} but found {contextType.FullName}");

			var iMessageType = _iRequestMessage.MakeGenericType(resposeType);
			if (!iMessageType.IsAssignableFrom(messageType))
				throw new ConfigurationException($"For interceptor {interceptorType.FullName} message type {messageType.FullName} must implement {iMessageType.FullName}");

			_services.Add(new ServiceDescriptor(interceptorType, interceptorType, _interceptorLifetime));
		}




		public Type? GetAsyncMessageHandlerType<TRequestMessage, TResponse>()
			where TRequestMessage : Messages.IRequestMessage<TResponse>
		{
			return GetAsyncMessageHandlerType(typeof(TRequestMessage));
		}

		public Type? GetAsyncMessageHandlerType(Type messageType)
		{
			if (messageType == null)
				throw new ArgumentNullException(nameof(messageType));

			_asyncMessageHandlersRegistry.TryGetValue(messageType, out Type? handlerType);
			return handlerType;
		}

		public void RegisterAsyncMessageHandler<THandler>()
		{
			RegisterAsyncMessageHandler(typeof(THandler));
		}

		public void RegisterAsyncMessageHandler(Type handlerType)
		{
			if (handlerType == null)
				throw new ArgumentNullException(nameof(handlerType));

			var interfaces = handlerType.GetInterfaces();
			if (interfaces != null)
			{
				foreach (var ifc in interfaces)
				{
					if (ifc.IsGenericType
						&& ifc.GenericTypeArguments.Length == 3
						&& _iAsyncMessageHandlerTypeDefinition.IsAssignableFrom(ifc.GetGenericTypeDefinition()))
					{
						RegisterAsyncMessageHandler(ifc.GenericTypeArguments[0], ifc.GenericTypeArguments[1], ifc.GenericTypeArguments[2], handlerType);
						return;
					}
				}
			}

			throw new ConfigurationException($"Invalid asynchronous message handler type {handlerType.FullName}");
		}

		public void RegisterAsyncMessageHandler<TRequestMessage, TResponse, THandler, TContext>()
			where TRequestMessage : Messages.IRequestMessage<TResponse>
			where THandler : IAsyncMessageHandler<TRequestMessage, TResponse, TContext>
			where TContext : IMessageHandlerContext
		{
			RegisterAsyncMessageHandler(typeof(TRequestMessage), typeof(TResponse), typeof(TContext), typeof(THandler));
		}

		public void RegisterAsyncMessageHandler(Type messageType, Type resposeType, Type contextType, Type handlerType)
		{
			if (messageType == null)
				throw new ArgumentNullException(nameof(messageType));
			if (resposeType == null)
				throw new ArgumentNullException(nameof(resposeType));
			if (contextType == null)
				throw new ArgumentNullException(nameof(contextType));
			if (handlerType == null)
				throw new ArgumentNullException(nameof(handlerType));

			if (_messageHandlerContextType != contextType)
				throw new ConfigurationException($"For handler {handlerType.FullName} is required context type {_messageHandlerContextType.FullName} but found {contextType.FullName}");

			if (_asyncMessageHandlersRegistry.TryGetValue(messageType, out var registeredHandlerType))
				throw new ConfigurationException($"Message type {messageType.FullName} is already registered to {registeredHandlerType?.FullName ?? "--NULL--"} Cannot be registered to {handlerType.FullName}");

			var iMessageType = _iRequestMessage.MakeGenericType(resposeType);
			if (!iMessageType.IsAssignableFrom(messageType))
				throw new ConfigurationException($"For handler {handlerType.FullName} the message type {messageType.FullName} must implement {iMessageType.FullName}");

			var iMessageHandlerType = _iAsyncMessageHandlerTypeDefinition.MakeGenericType(messageType, resposeType, contextType);
			if (!iMessageHandlerType.IsAssignableFrom(handlerType))
				throw new ConfigurationException($"Handler type {handlerType.FullName} must implement {iMessageHandlerType.FullName}");

			var added = _asyncMessageHandlersRegistry.TryAdd(messageType, handlerType);
			if (!added)
				throw new ConfigurationException($"Message type {messageType.FullName} is already registered. Cannot be registered to {handlerType.FullName}");

			AddMessageType(messageType, resposeType, MessageMetaType.RequestMessage_TResponse);

			_services.Add(new ServiceDescriptor(iMessageHandlerType, handlerType, _handlerLifetime));
		}

		public void RegisterAsyncMessageHandlerInterceptor(Type messageType, Type resposeType, Type contextType, Type interceptorType)
		{
			if (messageType == null)
				throw new ArgumentNullException(nameof(messageType));
			if (resposeType == null)
				throw new ArgumentNullException(nameof(resposeType));
			if (contextType == null)
				throw new ArgumentNullException(nameof(contextType));
			if (interceptorType == null)
				throw new ArgumentNullException(nameof(interceptorType));

			if (_messageHandlerContextType != contextType)
				throw new ConfigurationException($"For interceptor {interceptorType.FullName} is required context type {_messageHandlerContextType.FullName} but found {contextType.FullName}");

			var iMessageType = _iRequestMessage.MakeGenericType(resposeType);
			if (!iMessageType.IsAssignableFrom(messageType))
				throw new ConfigurationException($"For interceptor {interceptorType.FullName} message type {messageType.FullName} must implement {iMessageType.FullName}");

			_services.Add(new ServiceDescriptor(interceptorType, interceptorType, _interceptorLifetime));
		}




		public Type? GetVoidMessageHandlerType<TRequestMessage>()
			where TRequestMessage : Messages.IRequestMessage
		{
			return GetVoidMessageHandlerType(typeof(TRequestMessage));
		}

		public Type? GetVoidMessageHandlerType(Type messageType)
		{
			if (messageType == null)
				throw new ArgumentNullException(nameof(messageType));

			_voidMessageHandlersRegistry.TryGetValue(messageType, out Type? handlerType);
			return handlerType;
		}

		public void RegisterVoidMessageHandler<THandler>()
		{
			RegisterVoidMessageHandler(typeof(THandler));
		}

		public void RegisterVoidMessageHandler(Type handlerType)
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
						&& _iVoidMessageHandlerTypeDefinition.IsAssignableFrom(ifc.GetGenericTypeDefinition()))
					{
						RegisterVoidMessageHandler(ifc.GenericTypeArguments[0], ifc.GenericTypeArguments[1], handlerType);
						return;
					}
				}
			}

			throw new ConfigurationException($"Invalid synchronous message handler type {handlerType.FullName}");
		}

		public void RegisterVoidMessageHandler<TRequestMessage, THandler, TContext>()
			where TRequestMessage : Messages.IRequestMessage
			where THandler : IMessageHandler<TRequestMessage, TContext>
			where TContext : IMessageHandlerContext
		{
			RegisterVoidMessageHandler(typeof(TRequestMessage), typeof(TContext), typeof(THandler));
		}

		public void RegisterVoidMessageHandler(Type messageType, Type contextType, Type handlerType)
		{
			if (messageType == null)
				throw new ArgumentNullException(nameof(messageType));
			if (contextType == null)
				throw new ArgumentNullException(nameof(contextType));
			if (handlerType == null)
				throw new ArgumentNullException(nameof(handlerType));

			if (_messageHandlerContextType != contextType)
				throw new ConfigurationException($"For handler {handlerType.FullName} is required context type {_messageHandlerContextType.FullName} but found {contextType.FullName}");

			if (_voidMessageHandlersRegistry.TryGetValue(messageType, out var registeredHandlerType))
				throw new ConfigurationException($"Message type {messageType.FullName} is already registered to {registeredHandlerType?.FullName ?? "--NULL--"} Cannot be registered to {handlerType.FullName}");

			if (!_iVoidRequestMessage.IsAssignableFrom(messageType))
				throw new ConfigurationException($"For handler {handlerType.FullName} the message type {messageType.FullName} must implement {_iVoidRequestMessage.FullName}");

			var iMessageHandlerType = _iVoidMessageHandlerTypeDefinition.MakeGenericType(messageType, contextType);
			if (!iMessageHandlerType.IsAssignableFrom(handlerType))
				throw new ConfigurationException($"Handler type {handlerType.FullName} must implement {iMessageHandlerType.FullName}");

			var added = _voidMessageHandlersRegistry.TryAdd(messageType, handlerType);
			if (!added)
				throw new ConfigurationException($"Message type {messageType.FullName} is already registered. Cannot be registered to {handlerType.FullName}");

			AddMessageType(messageType, null, MessageMetaType.RequestMessage_Void);

			_services.Add(new ServiceDescriptor(iMessageHandlerType, handlerType, _handlerLifetime));
		}

		public void RegisterVoidMessageHandlerInterceptor(Type messageType, Type contextType, Type interceptorType)
		{
			if (messageType == null)
				throw new ArgumentNullException(nameof(messageType));
			if (contextType == null)
				throw new ArgumentNullException(nameof(contextType));
			if (interceptorType == null)
				throw new ArgumentNullException(nameof(interceptorType));

			if (_messageHandlerContextType != contextType)
				throw new ConfigurationException($"For interceptor {interceptorType.FullName} is required context type {_messageHandlerContextType.FullName} but found {contextType.FullName}");

			if (!_iVoidRequestMessage.IsAssignableFrom(messageType))
				throw new ConfigurationException($"For interceptor {interceptorType.FullName} message type {messageType.FullName} must implement {_iVoidRequestMessage.FullName}");

			_services.Add(new ServiceDescriptor(interceptorType, interceptorType, _interceptorLifetime));
		}





		public Type? GetAsyncVoidMessageHandlerType<TRequestMessage>()
			where TRequestMessage : Messages.IRequestMessage
		{
			return GetAsyncVoidMessageHandlerType(typeof(TRequestMessage));
		}

		public Type? GetAsyncVoidMessageHandlerType(Type messageType)
		{
			if (messageType == null)
				throw new ArgumentNullException(nameof(messageType));

			_asyncVoidMessageHandlersRegistry.TryGetValue(messageType, out Type? handlerType);
			return handlerType;
		}

		public void RegisterAsyncVoidMessageHandler<THandler>()
		{
			RegisterAsyncVoidMessageHandler(typeof(THandler));
		}

		public void RegisterAsyncVoidMessageHandler(Type handlerType)
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
						&& _iAsyncVoidMessageHandlerTypeDefinition.IsAssignableFrom(ifc.GetGenericTypeDefinition()))
					{
						RegisterAsyncVoidMessageHandler(ifc.GenericTypeArguments[0], ifc.GenericTypeArguments[1], handlerType);
						return;
					}
				}
			}

			throw new ConfigurationException($"Invalid asynchronous message handler type {handlerType.FullName}");
		}

		public void RegisterAsyncVoidMessageHandler<TRequestMessage, THandler, TContext>()
			where TRequestMessage : Messages.IRequestMessage
			where THandler : IAsyncMessageHandler<TRequestMessage, TContext>
			where TContext : IMessageHandlerContext
		{
			RegisterAsyncVoidMessageHandler(typeof(TRequestMessage), typeof(TContext), typeof(THandler));
		}

		public void RegisterAsyncVoidMessageHandler(Type messageType, Type contextType, Type handlerType)
		{
			if (messageType == null)
				throw new ArgumentNullException(nameof(messageType));
			if (contextType == null)
				throw new ArgumentNullException(nameof(contextType));
			if (handlerType == null)
				throw new ArgumentNullException(nameof(handlerType));

			if (_messageHandlerContextType != contextType)
				throw new ConfigurationException($"For handler {handlerType.FullName} is required context type {_messageHandlerContextType.FullName} but found {contextType.FullName}");

			if (_asyncVoidMessageHandlersRegistry.TryGetValue(messageType, out var registeredHandlerType))
				throw new ConfigurationException($"Message type {messageType.FullName} is already registered to {registeredHandlerType?.FullName ?? "--NULL--"} Cannot be registered to {handlerType.FullName}");

			if (!_iVoidRequestMessage.IsAssignableFrom(messageType))
				throw new ConfigurationException($"For handler {handlerType.FullName} the message type {messageType.FullName} must implement {_iVoidRequestMessage.FullName}");

			var iMessageHandlerType = _iAsyncVoidMessageHandlerTypeDefinition.MakeGenericType(messageType, contextType);
			if (!iMessageHandlerType.IsAssignableFrom(handlerType))
				throw new ConfigurationException($"Handler type {handlerType.FullName} must implement {iMessageHandlerType.FullName}");

			var added = _asyncVoidMessageHandlersRegistry.TryAdd(messageType, handlerType);
			if (!added)
				throw new ConfigurationException($"Message type {messageType.FullName} is already registered. Cannot be registered to {handlerType.FullName}");

			AddMessageType(messageType, null, MessageMetaType.RequestMessage_Void);

			_services.Add(new ServiceDescriptor(iMessageHandlerType, handlerType, _handlerLifetime));
		}

		public void RegisterAsyncVoidMessageHandlerInterceptor(Type messageType, Type contextType, Type interceptorType)
		{
			if (messageType == null)
				throw new ArgumentNullException(nameof(messageType));
			if (contextType == null)
				throw new ArgumentNullException(nameof(contextType));
			if (interceptorType == null)
				throw new ArgumentNullException(nameof(interceptorType));

			if (_messageHandlerContextType != contextType)
				throw new ConfigurationException($"For interceptor {interceptorType.FullName} is required context type {_messageHandlerContextType.FullName} but found {contextType.FullName}");

			if (!_iVoidRequestMessage.IsAssignableFrom(messageType))
				throw new ConfigurationException($"For interceptor {interceptorType.FullName} message type {messageType.FullName} must implement {_iVoidRequestMessage.FullName}");

			_services.Add(new ServiceDescriptor(interceptorType, interceptorType, _interceptorLifetime));
		}




		private void AddMessageType(Type type, Type? responseType, MessageMetaType messageMetaType)
		{
			var resolvedTypeString = _typeResolver.ToName(type);
			if (string.IsNullOrWhiteSpace(resolvedTypeString))
				throw new InvalidOperationException($"Message type {type} {nameof(resolvedTypeString)} == NULL");

			var messageType =
				new DbMessageType
				(
					GuidConverter.ToGuid(resolvedTypeString),
					type.FullName ?? type.Name,
					(int)messageMetaType,
					resolvedTypeString
				);

			var added = _types.TryAdd(type, messageType);
			if (!added)
				_logger.LogWarningMessage(x => x.InternalMessage($"{nameof(_types)} already contains request {nameof(type)} = {type.FullName} | {nameof(DbMessageType.IdMessageType)} = {messageType.IdMessageType}"));

			added = _messageTypes.TryAdd(messageType, type);
			if (!added)
				_logger.LogWarningMessage(x => x.InternalMessage($"{nameof(_messageTypes)} already contains request {nameof(DbMessageType.IdMessageType)} = {messageType.IdMessageType} | {nameof(type)} = {type.FullName}"));

			if (responseType != null)
			{
				resolvedTypeString = _typeResolver.ToName(responseType);
				if (string.IsNullOrWhiteSpace(resolvedTypeString))
					throw new InvalidOperationException($"Response type's {responseType} FullName == NULL");

				messageType =
					new DbMessageType
					(
						GuidConverter.ToGuid(resolvedTypeString),
						responseType.FullName ?? responseType.Name,
						(int)MessageMetaType.TResponse,
						resolvedTypeString
					);

				added = _types.TryAdd(responseType, messageType);
				if (!added)
					_logger.LogWarningMessage(x => x.InternalMessage($"{nameof(_types)} already contains response {nameof(responseType)} = {responseType.FullName} | {nameof(DbMessageType.IdMessageType)} = {messageType.IdMessageType}"));

				added = _messageTypes.TryAdd(messageType, responseType);
				if (!added)
					_logger.LogWarningMessage(x => x.InternalMessage($"{nameof(_messageTypes)} already contains response {nameof(DbMessageType.IdMessageType)} = {messageType.IdMessageType} | {nameof(responseType)} = {responseType.FullName}"));
			}
		}
	}
}

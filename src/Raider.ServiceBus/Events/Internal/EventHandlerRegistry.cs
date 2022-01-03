using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Raider.Converters;
using Raider.Exceptions;
using Raider.ServiceBus.Events.Interceptors;
using Raider.ServiceBus.Internal.Model;
using Raider.ServiceBus.Model;
using Raider.ServiceBus.Resolver;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Raider.ServiceBus.Events.Internal
{
	internal class EventHandlerRegistry : IEventHandlerRegistry, IEventTypeRegistry
	{
		private static readonly Type _iEvent = typeof(IEvent);
		private static readonly Type _iEventHandlerTypeDefinition = typeof(IEventHandler<,>);
		private static readonly Type _iAsyncEventHandlerTypeDefinition = typeof(IAsyncEventHandler<,>);

		private static readonly Type _iEventHandlerInterceptorTypeDefinition = typeof(IEventHandlerInterceptor<,>);
		private static readonly Type _iAsyncEventHandlerInterceptorTypeDefinition = typeof(IAsyncEventHandlerInterceptor<,>);

		private readonly IServiceCollection _services;
		private readonly ITypeResolver _typeResolver;
		private readonly Type _eventHandlerContextType;
		private readonly ILogger _logger;
		private readonly ServiceLifetime _handlerLifetime;
		private readonly ServiceLifetime _interceptorLifetime;
		private readonly ConcurrentDictionary<Type, List<Type>> _eventHandlersRegistry; //ConcurrentDictionary<eventType, List<handlerType>>
		private readonly ConcurrentDictionary<Type, List<Type>> _asyncEventHandlersRegistry; //ConcurrentDictionary<eventType, List<handlerType>>

		private readonly ConcurrentDictionary<Type, DbMessageType> _types; //ConcurrentDictionary<crl_event_type, EventType>
		private readonly ConcurrentDictionary<IMessageType, Type> _eventTypes; //ConcurrentDictionary<EventType, crl_event_type>

		public EventHandlerRegistry(
			IServiceCollection services,
			Type eventHandlerContextType,
			ITypeResolver typeResolver,
			ILogger logger,
			ServiceLifetime handlerLifetime = ServiceLifetime.Transient,
			ServiceLifetime interceptorLifetime = ServiceLifetime.Transient)
		{
			_services = services ?? throw new ArgumentNullException(nameof(services));
			_eventHandlerContextType = eventHandlerContextType ?? throw new ArgumentNullException(nameof(eventHandlerContextType));
			_typeResolver = typeResolver ?? throw new ArgumentNullException(nameof(typeResolver));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_handlerLifetime = handlerLifetime;
			_interceptorLifetime = interceptorLifetime;
			_eventHandlersRegistry = new ConcurrentDictionary<Type, List<Type>>();
			_asyncEventHandlersRegistry = new ConcurrentDictionary<Type, List<Type>>();

			_types = new ConcurrentDictionary<Type, DbMessageType>();
			_eventTypes = new ConcurrentDictionary<IMessageType, Type>();
		}

		public IEnumerable<IMessageType>? GetAllEventTypes()
			=> _types.Values.ToList();

		public IMessageType? GetEventType(Type type)
		{
			_types.TryGetValue(type, out var eventType);
			return eventType;
		}

		public Type? GetType(IMessageType eventType)
		{
			_eventTypes.TryGetValue(eventType, out var type);
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
						if (_iEventHandlerTypeDefinition.IsAssignableFrom(ifc.GetGenericTypeDefinition()))
						{
							RegisterEventHandler(ifc.GenericTypeArguments[0], ifc.GenericTypeArguments[1], type);
							return true;
						}
						else if (_iAsyncEventHandlerTypeDefinition.IsAssignableFrom(ifc.GetGenericTypeDefinition()))
						{
							RegisterAsyncEventHandler(ifc.GenericTypeArguments[0], ifc.GenericTypeArguments[1], type);
							return true;
						}
						else if (_iEventHandlerInterceptorTypeDefinition.IsAssignableFrom(ifc.GetGenericTypeDefinition()))
						{
							RegisterEventHandlerInterceptor(ifc.GenericTypeArguments[0], ifc.GenericTypeArguments[1], type);
						}
						else if (_iAsyncEventHandlerInterceptorTypeDefinition.IsAssignableFrom(ifc.GetGenericTypeDefinition()))
						{
							RegisterAsyncEventHandlerInterceptor(ifc.GenericTypeArguments[0], ifc.GenericTypeArguments[1], type);
						}
					}
				}
			}

			return false;
		}




		public List<Type>? GetEventHandlerType<TEvent>()
			where TEvent : IEvent
		{
			return GetEventHandlerType(typeof(TEvent));
		}

		public List<Type>? GetEventHandlerType(Type eventType)
		{
			if (eventType == null)
				throw new ArgumentNullException(nameof(eventType));

			_eventHandlersRegistry.TryGetValue(eventType, out List<Type>? handlerTypes);
			return handlerTypes;
		}

		public void RegisterEventHandler<THandler>()
		{
			RegisterEventHandler(typeof(THandler));
		}

		public void RegisterEventHandler(Type handlerType)
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
						&& _iEventHandlerTypeDefinition.IsAssignableFrom(ifc.GetGenericTypeDefinition()))
					{
						RegisterEventHandler(ifc.GenericTypeArguments[0], ifc.GenericTypeArguments[1], handlerType);
						return;
					}
				}
			}

			throw new ConfigurationException($"Invalid synchronous event handler type {handlerType.FullName}");
		}

		public void RegisterEventHandler<TEvent, THandler, TContext>()
			where TEvent : IEvent
			where THandler : IEventHandler<TEvent, TContext>
			where TContext : IEventHandlerContext
		{
			RegisterEventHandler(typeof(TEvent), typeof(TContext), typeof(THandler));
		}

		public void RegisterEventHandler(Type eventType, Type contextType, Type handlerType)
		{
			if (eventType == null)
				throw new ArgumentNullException(nameof(eventType));
			if (contextType == null)
				throw new ArgumentNullException(nameof(contextType));
			if (handlerType == null)
				throw new ArgumentNullException(nameof(handlerType));

			if (_eventHandlerContextType != contextType)
				throw new ConfigurationException($"For handler {handlerType.FullName} is required context type {_eventHandlerContextType.FullName} but found {contextType.FullName}");

			if (!_iEvent.IsAssignableFrom(eventType))
				throw new ConfigurationException($"For handler {handlerType.FullName} the event type {eventType.FullName} must implement {_iEvent.FullName}");

			var iEventHandlerType = _iEventHandlerTypeDefinition.MakeGenericType(eventType, contextType);
			if (!iEventHandlerType.IsAssignableFrom(handlerType))
				throw new ConfigurationException($"Handler type {handlerType.FullName} must implement {iEventHandlerType.FullName}");

			var added = _eventHandlersRegistry.AddOrUpdate(eventType, new List<Type> { handlerType }, (key, existingTypes) =>
			{
				existingTypes.Add(handlerType);
				return existingTypes;
			});

			AddEventType(eventType, MessageMetaType.Event);

			_services.Add(new ServiceDescriptor(iEventHandlerType, handlerType, _handlerLifetime));
		}

		public void RegisterEventHandlerInterceptor(Type eventType, Type contextType, Type interceptorType)
		{
			if (eventType == null)
				throw new ArgumentNullException(nameof(eventType));
			if (contextType == null)
				throw new ArgumentNullException(nameof(contextType));
			if (interceptorType == null)
				throw new ArgumentNullException(nameof(interceptorType));

			if (_eventHandlerContextType != contextType)
				throw new ConfigurationException($"For interceptor {interceptorType.FullName} is required context type {_eventHandlerContextType.FullName} but found {contextType.FullName}");

			if (!_iEvent.IsAssignableFrom(eventType))
				throw new ConfigurationException($"For interceptor {interceptorType.FullName} event type {eventType.FullName} must implement {_iEvent.FullName}");

			_services.Add(new ServiceDescriptor(interceptorType, interceptorType, _interceptorLifetime));
		}





		public List<Type>? GetAsyncEventHandlerType<TEvent>()
			where TEvent : IEvent
		{
			return GetAsyncEventHandlerType(typeof(TEvent));
		}

		public List<Type>? GetAsyncEventHandlerType(Type eventType)
		{
			if (eventType == null)
				throw new ArgumentNullException(nameof(eventType));

			_asyncEventHandlersRegistry.TryGetValue(eventType, out List<Type>? handlerTypes);
			return handlerTypes;
		}

		public void RegisterAsyncEventHandler<THandler>()
		{
			RegisterAsyncEventHandler(typeof(THandler));
		}

		public void RegisterAsyncEventHandler(Type handlerType)
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
						&& _iAsyncEventHandlerTypeDefinition.IsAssignableFrom(ifc.GetGenericTypeDefinition()))
					{
						RegisterAsyncEventHandler(ifc.GenericTypeArguments[0], ifc.GenericTypeArguments[1], handlerType);
						return;
					}
				}
			}

			throw new ConfigurationException($"Invalid asynchronous event handler type {handlerType.FullName}");
		}

		public void RegisterAsyncEventHandler<TEvent, THandler, TContext>()
			where TEvent : IEvent
			where THandler : IAsyncEventHandler<TEvent, TContext>
			where TContext : IEventHandlerContext
		{
			RegisterAsyncEventHandler(typeof(TEvent), typeof(TContext), typeof(THandler));
		}

		public void RegisterAsyncEventHandler(Type eventType, Type contextType, Type handlerType)
		{
			if (eventType == null)
				throw new ArgumentNullException(nameof(eventType));
			if (contextType == null)
				throw new ArgumentNullException(nameof(contextType));
			if (handlerType == null)
				throw new ArgumentNullException(nameof(handlerType));

			if (_eventHandlerContextType != contextType)
				throw new ConfigurationException($"For handler {handlerType.FullName} is required context type {_eventHandlerContextType.FullName} but found {contextType.FullName}");

			if (!_iEvent.IsAssignableFrom(eventType))
				throw new ConfigurationException($"For handler {handlerType.FullName} the event type {eventType.FullName} must implement {_iEvent.FullName}");

			var iEventHandlerType = _iAsyncEventHandlerTypeDefinition.MakeGenericType(eventType, contextType);
			if (!iEventHandlerType.IsAssignableFrom(handlerType))
				throw new ConfigurationException($"Handler type {handlerType.FullName} must implement {iEventHandlerType.FullName}");

			var added = _asyncEventHandlersRegistry.AddOrUpdate(eventType, new List<Type> { handlerType }, (key, existingTypes) =>
			{
				existingTypes.Add(handlerType);
				return existingTypes;
			});

			AddEventType(eventType, MessageMetaType.Event);

			_services.Add(new ServiceDescriptor(iEventHandlerType, handlerType, _handlerLifetime));
		}

		public void RegisterAsyncEventHandlerInterceptor(Type eventType, Type contextType, Type interceptorType)
		{
			if (eventType == null)
				throw new ArgumentNullException(nameof(eventType));
			if (contextType == null)
				throw new ArgumentNullException(nameof(contextType));
			if (interceptorType == null)
				throw new ArgumentNullException(nameof(interceptorType));

			if (_eventHandlerContextType != contextType)
				throw new ConfigurationException($"For interceptor {interceptorType.FullName} is required context type {_eventHandlerContextType.FullName} but found {contextType.FullName}");

			if (!_iEvent.IsAssignableFrom(eventType))
				throw new ConfigurationException($"For interceptor {interceptorType.FullName} event type {eventType.FullName} must implement {_iEvent.FullName}");

			_services.Add(new ServiceDescriptor(interceptorType, interceptorType, _interceptorLifetime));
		}




		private void AddEventType(Type type, MessageMetaType eventMetaType)
		{
			var resolvedTypeString = _typeResolver.ToName(type);
			if (string.IsNullOrWhiteSpace(resolvedTypeString))
				throw new InvalidOperationException($"Event type {type} {nameof(resolvedTypeString)} == NULL");

			var eventType =
				new DbMessageType
				(
					GuidConverter.ToGuid(resolvedTypeString),
					type.FullName ?? type.Name,
					(int)eventMetaType,
					resolvedTypeString
				);

			_types.TryAdd(type, eventType);
			_eventTypes.TryAdd(eventType, type);
		}
	}
}

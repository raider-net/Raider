using Microsoft.Extensions.DependencyInjection;
using Raider.Exceptions;
using System;
using System.Collections.Generic;

namespace Raider.Queries.Internal
{
	internal class QueryHandlerRegistry : IQueryHandlerRegistry
	{
		private static readonly Type _iQueryHandlerTypeDefinition = typeof(IQueryHandler<,>);
		private static readonly Type _iAsyncQueryHandlerTypeDefinition = typeof(IAsyncQueryHandler<,>);

		private readonly IServiceCollection _services;
		private readonly ServiceLifetime _lifetime;
		private readonly IDictionary<Type, Type> _queryHandlersRegistry; //IDictionary<queryType, handlerType>
		private readonly IDictionary<Type, Type> _asyncQueryHandlersRegistry; //IDictionary<queryType, handlerType>

		public QueryHandlerRegistry(IServiceCollection services, ServiceLifetime lifetime = ServiceLifetime.Transient)
		{
			_services = services ?? throw new ArgumentNullException(nameof(services));
			_lifetime = lifetime;
			_queryHandlersRegistry = new Dictionary<Type, Type>();
			_asyncQueryHandlersRegistry = new Dictionary<Type, Type>();
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
						if (ifc.GenericTypeArguments.Length == 2)
						{
							if (_iQueryHandlerTypeDefinition.IsAssignableFrom(ifc.GetGenericTypeDefinition()))
							{
								RegisterQueryHandler(ifc.GenericTypeArguments[0], ifc.GenericTypeArguments[1], handlerType);
								return true;
							}
							else if (_iAsyncQueryHandlerTypeDefinition.IsAssignableFrom(ifc.GetGenericTypeDefinition()))
							{
								RegisterAsyncQueryHandler(ifc.GenericTypeArguments[0], ifc.GenericTypeArguments[1], handlerType);
								return true;
							}
						}
					}
				}
			}

			return false;
		}

		public Type? GetQueryHandler<TQuery, TResult>()
			where TQuery : IQuery<TResult>
		{
			return GetQueryHandler(typeof(TQuery));
		}

		public Type? GetQueryHandler(Type queryType)
		{
			if (queryType == null)
				throw new ArgumentNullException(nameof(queryType));

			_queryHandlersRegistry.TryGetValue(queryType, out Type? handlerType);
			return handlerType;
		}

		public void RegisterQueryHandler<THandler>()
		{
			RegisterQueryHandler(typeof(THandler));
		}

		public void RegisterQueryHandler(Type handlerType)
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
						&& _iQueryHandlerTypeDefinition.IsAssignableFrom(ifc.GetGenericTypeDefinition()))
					{
						RegisterQueryHandler(ifc.GenericTypeArguments[0], ifc.GenericTypeArguments[1], handlerType);
						return;
					}
				}
			}

			throw new ConfigurationException($"Invalid synchronous query handler type {handlerType.FullName}");
		}

		public void RegisterQueryHandler<TQuery, TResult, THandler>()
			where TQuery : IQuery<TResult>
			where THandler : IQueryHandler<TQuery, TResult>
		{
			RegisterQueryHandler(typeof(TQuery), typeof(TResult), typeof(THandler));
		}

		public void RegisterQueryHandler(Type queryType, Type resultType, Type handlerType)
		{
			if (queryType == null)
				throw new ArgumentNullException(nameof(queryType));
			if (resultType == null)
				throw new ArgumentNullException(nameof(resultType));
			if (handlerType == null)
				throw new ArgumentNullException(nameof(handlerType));

			if (_queryHandlersRegistry.ContainsKey(queryType))
			{
				var registeredHandlerType = GetQueryHandler(queryType);
				throw new ConfigurationException($"Query type {queryType.FullName} is already registered to {registeredHandlerType?.FullName ?? "--NULL--"} Cannot be registered to {handlerType.FullName}");
			}

			var iQueryType = typeof(IQuery<>).MakeGenericType(resultType);
			if (!iQueryType.IsAssignableFrom(queryType))
				throw new ConfigurationException($"Query type {queryType.FullName} must implement {iQueryType.FullName}");

			var iQueryHandlerType = typeof(IQueryHandler<,>).MakeGenericType(queryType, resultType);
			if (!iQueryHandlerType.IsAssignableFrom(handlerType))
				throw new ConfigurationException($"Handler type {handlerType.FullName} must implement {iQueryHandlerType.FullName}");

			_queryHandlersRegistry.Add(queryType, handlerType);

			_services.Add(new ServiceDescriptor(iQueryHandlerType, handlerType, _lifetime));
		}

		public Type? GetAsyncQueryHandler<TQuery, TResult>()
			where TQuery : IQuery<TResult>
		{
			return GetAsyncQueryHandler(typeof(TQuery));
		}

		public Type? GetAsyncQueryHandler(Type queryType)
		{
			if (queryType == null)
				throw new ArgumentNullException(nameof(queryType));

			_asyncQueryHandlersRegistry.TryGetValue(queryType, out Type? handlerType);
			return handlerType;
		}

		public void RegisterAsyncQueryHandler<THandler>()
		{
			RegisterAsyncQueryHandler(typeof(THandler));
		}

		public void RegisterAsyncQueryHandler(Type handlerType)
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
						&& _iAsyncQueryHandlerTypeDefinition.IsAssignableFrom(ifc.GetGenericTypeDefinition()))
					{
						RegisterAsyncQueryHandler(ifc.GenericTypeArguments[0], ifc.GenericTypeArguments[1], handlerType);
						return;
					}
				}
			}

			throw new ConfigurationException($"Invalid asynchronous query handler type {handlerType.FullName}");
		}

		public void RegisterAsyncQueryHandler<TQuery, TResult, THandler>()
			where TQuery : IQuery<TResult>
			where THandler : IAsyncQueryHandler<TQuery, TResult>
		{
			RegisterAsyncQueryHandler(typeof(TQuery), typeof(TResult), typeof(THandler));
		}

		public void RegisterAsyncQueryHandler(Type queryType, Type resultType, Type handlerType)
		{
			if (queryType == null)
				throw new ArgumentNullException(nameof(queryType));
			if (resultType == null)
				throw new ArgumentNullException(nameof(resultType));
			if (handlerType == null)
				throw new ArgumentNullException(nameof(handlerType));

			if (_asyncQueryHandlersRegistry.ContainsKey(queryType))
			{
				var registeredHandlerType = GetAsyncQueryHandler(queryType);
				throw new ConfigurationException($"Query type {queryType.FullName} is already registered to {registeredHandlerType?.FullName ?? "--NULL--"} Cannot be registered to {handlerType.FullName}");
			}

			var iQueryType = typeof(IQuery<>).MakeGenericType(resultType);
			if (!iQueryType.IsAssignableFrom(queryType))
				throw new ConfigurationException($"Query type {queryType.FullName} must implement {iQueryType.FullName}");

			var iQueryHandlerType = typeof(IAsyncQueryHandler<,>).MakeGenericType(queryType, resultType);
			if (!iQueryHandlerType.IsAssignableFrom(handlerType))
				throw new ConfigurationException($"Handler type {handlerType.FullName} must implement {iQueryHandlerType.FullName}");

			_asyncQueryHandlersRegistry.Add(queryType, handlerType);

			_services.Add(new ServiceDescriptor(iQueryHandlerType, handlerType, _lifetime));
		}
	}
}

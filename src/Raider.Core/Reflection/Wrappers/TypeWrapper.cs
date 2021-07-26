using Raider.Extensions;
using Raider.Reflection.Internal;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Raider.Reflection
{
	internal interface ITypeWrapper { }

	internal class TypeWrapper<T> : ITypeWrapper
	{
		private static readonly ConcurrentDictionary<Type, ITypeWrapper> _typeManagerCache = new ();

		public Dictionary<string, Func<T?, object?>> Getters;
		public Dictionary<string, Func<T?, object?>> StaticGetters;
		public Dictionary<string, Action<T?, object?>> Setters;
		public Dictionary<string, Action<T?, object?>> StaticSetters;
		//public Dictionary<string, MethodCall<T?, object?>> Methods;
		//public Dictionary<string, MethodCall<T?, object?>> StaticMethods;

		private TypeWrapper()
		{
			Getters = new Dictionary<string, Func<T?, object?>>();
			StaticGetters = new Dictionary<string, Func<T?, object?>>();
			Setters = new Dictionary<string, Action<T?, object?>>();
			StaticSetters = new Dictionary<string, Action<T?, object?>>();
			//Methods = new Dictionary<string, MethodCall<T?, object?>>();
			//StaticMethods = new Dictionary<string, MethodCall<T?, object?>>();

			var type = typeof(T);

			var properties = type.GetProperties();
			foreach (var property in properties)
			{
				if (property.IsStatic())
				{
					if (property.CanRead)
					{
						var getter = DelegateFactory.Instance.CreateGet<T>(property);
						StaticGetters.TryAdd(property.Name, getter);
					}
					if (property.CanWrite)
					{
						var setter = DelegateFactory.Instance.CreateSet<T>(property);
						StaticSetters.TryAdd(property.Name, setter);
					}
				}
				else
				{
					if (property.CanRead)
					{
						var getter = DelegateFactory.Instance.CreateGet<T>(property);
						Getters.TryAdd(property.Name, getter);
					}
					if (property.CanWrite)
					{
						var setter = DelegateFactory.Instance.CreateSet<T>(property);
						Setters.TryAdd(property.Name, setter);
					}
				}
			}

			var fields = type.GetFields();
			foreach (var field in fields)
			{
				var getter = DelegateFactory.Instance.CreateGet<T>(field);

				if (field.IsStatic())
				{
					StaticGetters.TryAdd(field.Name, getter);
					
					if (!field.IsConst())
					{
						var setter = DelegateFactory.Instance.CreateSet<T>(field);
						StaticSetters.TryAdd(field.Name, setter);
					}
				}
				else
				{
					Getters.TryAdd(field.Name, getter);

					if (!field.IsConst())
					{
						var setter = DelegateFactory.Instance.CreateSet<T>(field);
						Setters.TryAdd(field.Name, setter);
					}
				}
			}

			//var methods = type.Methods();
			//foreach (var method in methods)
			//{
			//	var methodCall = DelegateFactory.Instance.CreateMethodCall<T>(method);

			//	if (method.IsStatic())
			//	{
			//		StaticMethods.TryAdd(method.Name, methodCall);
			//	}
			//	else
			//	{
			//		Methods.TryAdd(method.Name, methodCall);
			//	}
			//}
		}

		public static TypeWrapper<T> Create()
			=> (TypeWrapper<T>)_typeManagerCache.GetOrAdd(typeof(T), type => new TypeWrapper<T>());
	}
}

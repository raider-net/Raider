using System;
using System.Collections.Generic;

namespace Raider.DependencyInjection
{
	public delegate object? ServiceFactory(Type serviceType);

	public static class ServiceFactoryExtensions
	{
		public static object GetRequiredInstance(this ServiceFactory factory, Type serviceType)
		{
			if (serviceType == null)
				throw new ArgumentNullException(nameof(serviceType));

			var instance = factory(serviceType);
			if (instance == null)
				throw new InvalidOperationException($"Service for {serviceType.FullName} was not registered.");

			return instance;
		}

		public static T? GetInstance<T>(this ServiceFactory factory)
			=> (T?)factory(typeof(T));

		public static IEnumerable<T>? GetInstances<T>(this ServiceFactory factory)
			=> (IEnumerable<T>?)factory(typeof(IEnumerable<T>));
	
		public static T GetRequiredInstance<T>(this ServiceFactory factory)
		{
			var instance = (T?)factory(typeof(T));
			if (instance == null)
				throw new InvalidOperationException($"Service for {typeof(T).FullName} was not registered.");

			return instance;
		}

		public static IEnumerable<T> GetRequiredInstances<T>(this ServiceFactory factory)
		{
			var instance = (IEnumerable<T>?)factory(typeof(IEnumerable<T>));
			if (instance == null)
				throw new InvalidOperationException($"Service for {typeof(IEnumerable<T>).FullName} was not registered.");

			return instance;
		}
	}
}

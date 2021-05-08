//using System;
//using System.Collections.Generic;

//namespace Raider.DependencyInjection
//{
//	public delegate object? ServiceFactory(Type serviceType);

//	public static class ServiceFactoryExtensions
//	{
//		public static object GetRequiredService(this ServiceFactory factory, Type serviceType)
//		{
//			if (serviceType == null)
//				throw new ArgumentNullException(nameof(serviceType));

//			var instance = factory(serviceType);
//			if (instance == null)
//				throw new InvalidOperationException($"Service for {serviceType.FullName} was not registered.");

//			return instance;
//		}

//		public static T? GetService<T>(this ServiceFactory factory)
//			=> (T?)factory(typeof(T));

//		public static IEnumerable<T>? GetServices<T>(this ServiceFactory factory)
//			=> (IEnumerable<T>?)factory(typeof(IEnumerable<T>));

//		public static T GetRequiredService<T>(this ServiceFactory factory)
//		{
//			var instance = (T?)factory(typeof(T));
//			if (instance == null)
//				throw new InvalidOperationException($"Service for {typeof(T).FullName} was not registered.");

//			return instance;
//		}

//		public static IEnumerable<T> GetRequiredServices<T>(this ServiceFactory factory)
//		{
//			var instance = (IEnumerable<T>?)factory(typeof(IEnumerable<T>));
//			if (instance == null)
//				throw new InvalidOperationException($"Service for {typeof(IEnumerable<T>).FullName} was not registered.");

//			return instance;
//		}
//	}
//}

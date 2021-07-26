using Raider.Reflection.Delegates.Extensions;
using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Raider.Reflection.Delegates
{
	/// <summary>
	///     Creates delegates for types members
	/// </summary>
	public static partial class DelegateFactory
	{
		/// <summary>
		///     Creates delegate to static property getter with return type of property type
		/// </summary>
		/// <typeparam name="TSource">Type with defined property</typeparam>
		/// <typeparam name="TProperty">Type of property</typeparam>
		/// <param name="propertyName">Name of property</param>
		/// <returns>Delegate for static property getter</returns>
		public static Func<TProperty>? StaticPropertyGet<TSource, TProperty>(string propertyName)
		{
			return typeof(TSource).StaticPropertyGet<TProperty>(propertyName);
		}

		/// <summary>
		///     Creates delegate to static property getter with return type of property type
		/// </summary>
		/// <typeparam name="TProperty">Type of property</typeparam>
		/// <param name="source">Type with defined property</param>
		/// <param name="propertyName">Name of property</param>
		/// <returns>Delegate for static property getter</returns>
		public static Func<TProperty>? StaticPropertyGet<TProperty>(this Type source, string propertyName)
		{
			var propertyInfo = source.GetPropertyInfo(propertyName, true);
			return propertyInfo?.GetMethod?.CreateDelegate<Func<TProperty>>();
		}

		/// <summary>
		///     Creates delegate to static property getter with return type of object
		/// </summary>
		/// <param name="source">Type with defined property</param>
		/// <param name="propertyName">Name of property</param>
		/// <returns>Delegate for static property getter</returns>
		public static Func<object>? StaticPropertyGet(this Type source, string propertyName)
		{
			var propertyInfo = source.GetPropertyInfo(propertyName, true);
			if (propertyInfo?.GetMethod == null) return null;
			Expression returnExpression = Expression.Call(propertyInfo.GetMethod);
			if (!propertyInfo.PropertyType.GetTypeInfo().IsClass)
				returnExpression = Expression.Convert(returnExpression, typeof(object));
			return Expression.Lambda<Func<object>>(returnExpression).Compile();
		}
	}
}

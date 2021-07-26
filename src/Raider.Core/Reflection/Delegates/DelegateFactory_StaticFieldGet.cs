﻿using Raider.Reflection.Delegates.Extensions;
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
		///     Creates delegate for retrieving static field value
		/// </summary>
		/// <typeparam name="TSource">Source type with defined field</typeparam>
		/// <typeparam name="TField">Type of field</typeparam>
		/// <param name="fieldName">Field name</param>
		/// <returns>Delegate for retrieving static field value</returns>
		public static Func<TField>? StaticFieldGet<TSource, TField>(string fieldName)
		{
			var source = typeof(TSource);
			return source.StaticFieldGet<TField>(fieldName);
		}

		/// <summary>
		///     Creates delegate for retrieving static field value
		/// </summary>
		/// <typeparam name="TField">Type of field</typeparam>
		/// <param name="source">Type with defined field</param>
		/// <param name="fieldName">Field name</param>
		/// <returns>Delegate for retrieving static field value</returns>
		public static Func<TField>? StaticFieldGet<TField>(this Type source, string fieldName)
		{
			var fieldInfo = source.GetFieldInfo(fieldName, true);
			if (fieldInfo != null)
			{
				var lambda = Expression.Lambda(Expression.Field(null, fieldInfo));
				return (Func<TField>)lambda.Compile();
			}

			return null;
		}

		/// <summary>
		///     Creates delegate for retrieving static field value as object
		/// </summary>
		/// <param name="source">Type with defined field</param>
		/// <param name="fieldName">Field name</param>
		/// <returns>Delegate for retrieving static field value</returns>
		public static Func<object>? StaticFieldGet(this Type source, string fieldName)
		{
			var fieldInfo = source.GetFieldInfo(fieldName, true);
			if (fieldInfo != null)
			{
				Expression returnExpression = Expression.Field(null, fieldInfo);
				if (!fieldInfo.FieldType.GetTypeInfo().IsClass)
					returnExpression = Expression.Convert(returnExpression, typeof(object));
				var lambda = Expression.Lambda<Func<object>>(returnExpression);
				return lambda.Compile();
			}

			return null;
		}
	}
}

﻿using Raider.Reflection.Delegates.Extensions;
using System;
using System.Linq.Expressions;
using System.Reflection;
using static Raider.Reflection.Delegates.Helper.DelegateHelper;

namespace Raider.Reflection.Delegates
{
	/// <summary>
	///     Creates delegates for types members
	/// </summary>
	public static partial class DelegateFactory
	{
		/// <summary>
		///     Creates delegate for type constructor. Constructed type has to be return type of delegate.
		/// </summary>
		/// <typeparam name="TDelegate">
		///     Constructor delegate type. It should have parameters of searched constructor
		///     and return constructed type.
		/// </typeparam>
		/// <returns>Requested constructor delegate</returns>
		public static TDelegate? Constructor<TDelegate>() where TDelegate : class
		{
			var source = GetDelegateReturnType<TDelegate>();
			var ctrArgs = GetDelegateArguments<TDelegate>();
			var constructorInfo = source.GetConstructorInfo(ctrArgs);
			if (constructorInfo == null)
			{
				return null;
			}

			var parameters = ctrArgs.GetParamsExprFromTypes();
			return Expression.Lambda<TDelegate>(Expression.New(constructorInfo, parameters), parameters)
				.Compile();
		}

		/// <summary>
		///     Creates delegate for type constructor. Delegate takes array of objects as parameters of a constructor and
		///     returns constructed type as object.
		/// </summary>
		/// <param name="source">Type to be constructed</param>
		/// <param name="ctrArgs">Array of types of constructor parameters</param>
		/// <returns>Constructor delegate</returns>
		public static Func<object[], object>? Constructor(this Type source, params Type[] ctrArgs)
		{
			var constructorInfo = source.GetConstructorInfo(ctrArgs);
			if (constructorInfo == null)
			{
				return null;
			}

			var argsArray = Expression.Parameter(typeof(object[]), "args");
			var paramsExpression = new Expression[ctrArgs.Length];
			for (var i = 0; i < ctrArgs.Length; i++)
			{
				var argType = ctrArgs[i];
				paramsExpression[i] =
					Expression.Convert(Expression.ArrayIndex(argsArray, Expression.Constant(i)), argType);
			}

			Expression returnExpression = Expression.New(constructorInfo, paramsExpression);
			if (!source.GetTypeInfo().IsClass)
			{
				returnExpression = Expression.Convert(returnExpression, typeof(object));
			}

			return Expression.Lambda<Func<object[], object>>(returnExpression, argsArray).Compile();
		}

		/// <summary>
		///     Creates delegate for type constructor returns constructed type as object.
		/// </summary>
		/// <typeparam name="TDelegate">
		///     Type of delegate to return. It should have parameters of searched
		///     constructor and return constructed type.
		/// </typeparam>
		/// <param name="source">Type to be constructed</param>
		/// <returns>Constructor delegate</returns>
		public static TDelegate? Constructor<TDelegate>(this Type source)
			where TDelegate : class
		{
			var ctrArgs = GetDelegateArguments<TDelegate>();
			var constructorInfo = source.GetConstructorInfo(ctrArgs);
			if (constructorInfo == null)
			{
				return null;
			}

			var parameters = ctrArgs.GetParamsExprFromTypes();
			Expression returnExpression = Expression.New(constructorInfo, parameters);
			if (!source.GetTypeInfo().IsClass)
			{
				returnExpression = Expression.Convert(returnExpression, typeof(object));
			}

			return Expression.Lambda<TDelegate>(returnExpression, parameters).Compile();
		}

		/// <summary>
		///     Creates delegate for type default constructor.
		/// </summary>
		/// <typeparam name="TSource">Type of instance to be created by delegate.</typeparam>
		/// <returns>Default constructor delegate</returns>
		public static Func<TSource>? DefaultConstructor<TSource>()
		{
			return Constructor<Func<TSource>>();
		}

		/// <summary>
		///     Creates delegate for type default constructor.
		/// </summary>
		/// <param name="type">Type to be constructed</param>
		/// <returns>Default constructor delegate</returns>
		public static Func<object>? DefaultConstructor(this Type type)
		{
			return type.Constructor<Func<object>>();
		}
	}
}

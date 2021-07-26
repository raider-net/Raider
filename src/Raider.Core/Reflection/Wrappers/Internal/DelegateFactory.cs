using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Raider.Reflection.Internal
{
	public delegate object ObjectConstructor<T>(params object?[] args);
	public delegate TResult? MethodCall<T, TResult>(T? target, params object?[] args);

	internal class DelegateFactory : IDelegateFactory
	{
		private static readonly DelegateFactory _instance = new();
		public static DelegateFactory Instance => _instance;

		public Func<T?, object?> CreateGet<T>(MemberInfo memberInfo)
		{
			if (memberInfo == null)
				throw new ArgumentNullException(nameof(memberInfo));

			if (memberInfo is PropertyInfo propertyInfo)
			{
				if (propertyInfo.PropertyType.IsByRef)
					throw new InvalidOperationException($"Could not create getter for {propertyInfo.Name}. ByRef return values are not supported.");

				return CreateGet<T>(propertyInfo);
			}

			if (memberInfo is FieldInfo fieldInfo)
				return CreateGet<T>(fieldInfo);

			throw new Exception($"Could not create getter for {memberInfo.Name}");
		}

		public Func<T?, object?> CreateGet<T>(PropertyInfo propertyInfo)
		{
			if (propertyInfo == null)
				throw new ArgumentNullException(nameof(propertyInfo));

			Type instanceType = typeof(T);
			Type resultType = typeof(object);

			ParameterExpression parameterExpression = Expression.Parameter(instanceType, "instance");
			Expression resultExpression;

			MethodInfo? getMethod = propertyInfo.GetGetMethod(true);
			if (getMethod == null)
				throw new ArgumentException("Property does not have a getter.");

			if (getMethod.IsStatic)
			{
				resultExpression = Expression.MakeMemberAccess(null, propertyInfo);
			}
			else
			{
				Expression readParameter = EnsureCastExpression(parameterExpression, propertyInfo.DeclaringType!);
				resultExpression = Expression.MakeMemberAccess(readParameter, propertyInfo);
			}

			resultExpression = EnsureCastExpression(resultExpression, resultType);

			LambdaExpression lambdaExpression = Expression.Lambda(typeof(Func<T, object>), resultExpression, parameterExpression);

			var compiled = (Func<T?, object?>)lambdaExpression.Compile();
			return compiled;
		}

		public Func<T?, object?> CreateGet<T>(FieldInfo fieldInfo)
		{
			if (fieldInfo == null)
				throw new ArgumentNullException(nameof(fieldInfo));

			ParameterExpression sourceParameter = Expression.Parameter(typeof(T), "source");

			Expression fieldExpression;
			if (fieldInfo.IsStatic)
			{
				fieldExpression = Expression.Field(null, fieldInfo);
			}
			else
			{
				Expression sourceExpression = EnsureCastExpression(sourceParameter, fieldInfo.DeclaringType!);
				fieldExpression = Expression.Field(sourceExpression, fieldInfo);
			}

			fieldExpression = EnsureCastExpression(fieldExpression, typeof(object));

			var compiled = Expression.Lambda<Func<T?, object?>>(fieldExpression, sourceParameter).Compile();
			return compiled;
		}

		public Action<T?, object?> CreateSet<T>(MemberInfo memberInfo)
		{
			if (memberInfo == null)
				throw new ArgumentNullException(nameof(memberInfo));

			if (memberInfo is PropertyInfo propertyInfo)
				return CreateSet<T>(propertyInfo);

			if (memberInfo is FieldInfo fieldInfo)
				return CreateSet<T>(fieldInfo);

			throw new Exception($"Could not create setter for {memberInfo.Name}");
		}

		public Action<T?, object?> CreateSet<T>(PropertyInfo propertyInfo)
		{
			if (propertyInfo == null)
				throw new ArgumentNullException(nameof(propertyInfo));

			if (propertyInfo.DeclaringType?.IsValueType ?? false)
				return (o, v) => propertyInfo.SetValue(o, v, null);

			Type instanceType = typeof(T);
			Type valueType = typeof(object);

			ParameterExpression instanceParameter = Expression.Parameter(instanceType, "instance");

			ParameterExpression valueParameter = Expression.Parameter(valueType, "value");
			Expression readValueParameter = EnsureCastExpression(valueParameter, propertyInfo.PropertyType);

			MethodInfo? setMethod = propertyInfo.GetSetMethod(true);
			if (setMethod == null)
				throw new ArgumentException("Property does not have a setter.");

			Expression setExpression;
			if (setMethod.IsStatic)
			{
				setExpression = Expression.Call(setMethod, readValueParameter);
			}
			else
			{
				Expression readInstanceParameter = EnsureCastExpression(instanceParameter, propertyInfo.DeclaringType!);
				setExpression = Expression.Call(readInstanceParameter, setMethod, readValueParameter);
			}

			LambdaExpression lambdaExpression = Expression.Lambda(typeof(Action<T, object?>), setExpression, instanceParameter, valueParameter);

			var compiled = (Action<T?, object?>)lambdaExpression.Compile();
			return compiled;
		}

		public Action<T?, object?> CreateSet<T>(FieldInfo fieldInfo)
		{
			if (fieldInfo == null)
				throw new ArgumentNullException(nameof(fieldInfo));

			if ((fieldInfo.DeclaringType?.IsValueType ?? false) || fieldInfo.IsInitOnly)
				return (o, v) => fieldInfo.SetValue(o, v);

			ParameterExpression sourceParameterExpression = Expression.Parameter(typeof(T), "source");
			ParameterExpression valueParameterExpression = Expression.Parameter(typeof(object), "value");

			Expression fieldExpression;
			if (fieldInfo.IsStatic)
			{
				fieldExpression = Expression.Field(null, fieldInfo);
			}
			else
			{
				Expression sourceExpression = EnsureCastExpression(sourceParameterExpression, fieldInfo.DeclaringType!);
				fieldExpression = Expression.Field(sourceExpression, fieldInfo);
			}

			Expression valueExpression = EnsureCastExpression(valueParameterExpression, fieldExpression.Type);
			BinaryExpression assignExpression = Expression.Assign(fieldExpression, valueExpression);
			LambdaExpression lambdaExpression = Expression.Lambda(typeof(Action<T, object>), assignExpression, sourceParameterExpression, valueParameterExpression);

			var compiled = (Action<T?, object?>)lambdaExpression.Compile();
			return compiled;
		}

		public Func<T> CreateDefaultConstructor<T>(Type type)
		{
			if (type == null)
				throw new ArgumentNullException(nameof(type));

			if (type.IsAbstract)
				return () => (T)Activator.CreateInstance(type)!;

			try
			{
				Type resultType = typeof(T);
				Expression expression = Expression.New(type);
				expression = EnsureCastExpression(expression, resultType);
				LambdaExpression lambdaExpression = Expression.Lambda(typeof(Func<T>), expression);
				Func<T> compiled = (Func<T>)lambdaExpression.Compile();
				return compiled;
			}
			catch
			{
				return () => (T)Activator.CreateInstance(type)!;
			}
		}

		public ObjectConstructor<object> CreateParameterizedConstructor(MethodBase method)
		{
			if (method == null)
				throw new ArgumentNullException(nameof(method));

			Type type = typeof(object);
			ParameterExpression argsParameterExpression = Expression.Parameter(typeof(object[]), "args");
			Expression callExpression = BuildMethodCall(method, type, null, argsParameterExpression);
			LambdaExpression lambdaExpression = Expression.Lambda(typeof(ObjectConstructor<object>), callExpression, argsParameterExpression);
			ObjectConstructor<object> compiled = (ObjectConstructor<object>)lambdaExpression.Compile();
			return compiled;
		}

		public MethodCall<T?, object?> CreateMethodCall<T>(MethodBase method)
		{
			if (method == null)
				throw new ArgumentNullException(nameof(method));

			Type type = typeof(object);

			ParameterExpression targetParameterExpression = Expression.Parameter(type, "target");
			ParameterExpression argsParameterExpression = Expression.Parameter(typeof(object[]), "args");

			Expression callExpression = BuildMethodCall(method, type, targetParameterExpression, argsParameterExpression);

			LambdaExpression lambdaExpression = Expression.Lambda(typeof(MethodCall<T, object>), callExpression, targetParameterExpression, argsParameterExpression);

			var compiled = (MethodCall<T?, object?>)lambdaExpression.Compile();
			return compiled;
		}

		private class ByRefParameter
		{
			public Expression Value;
			public ParameterExpression Variable;
			public bool IsOut;

			public ByRefParameter(Expression value, ParameterExpression variable, bool isOut)
			{
				Value = value;
				Variable = variable;
				IsOut = isOut;
			}
		}

		private static Expression BuildMethodCall(MethodBase method, Type type, ParameterExpression? targetParameterExpression, ParameterExpression argsParameterExpression)
		{
			ParameterInfo[] parametersInfo = method.GetParameters();

			Expression[] argsExpression;
			IList<ByRefParameter> refParameterMap;
			if (parametersInfo.Length == 0)
			{
				argsExpression = ArrayEmpty<Expression>();
				refParameterMap = ArrayEmpty<ByRefParameter>();
			}
			else
			{
				argsExpression = new Expression[parametersInfo.Length];
				refParameterMap = new List<ByRefParameter>();

				for (int i = 0; i < parametersInfo.Length; i++)
				{
					ParameterInfo parameter = parametersInfo[i];
					Type parameterType = parameter.ParameterType;
					bool isByRef = false;
					if (parameterType.IsByRef)
					{
						parameterType = parameterType.GetElementType()!;
						isByRef = true;
					}

					Expression indexExpression = Expression.Constant(i);
					Expression paramAccessorExpression = Expression.ArrayIndex(argsParameterExpression, indexExpression);
					Expression argExpression = EnsureCastExpression(paramAccessorExpression, parameterType, !isByRef);

					if (isByRef)
					{
						ParameterExpression variable = Expression.Variable(parameterType);
						refParameterMap.Add(new ByRefParameter(argExpression, variable, parameter.IsOut));
						argExpression = variable;
					}

					argsExpression[i] = argExpression;
				}
			}

			Expression callExpression;
			if (method.IsConstructor)
			{
				callExpression = Expression.New((ConstructorInfo)method, argsExpression);
			}
			else if (method.IsStatic)
			{
				callExpression = Expression.Call((MethodInfo)method, argsExpression);
			}
			else
			{
				Expression readParameter = EnsureCastExpression(targetParameterExpression!, method.DeclaringType!);
				callExpression = Expression.Call(readParameter, (MethodInfo)method, argsExpression);
			}

			if (method is MethodInfo m)
			{
				if (m.ReturnType != typeof(void))
					callExpression = EnsureCastExpression(callExpression, type);
				else
					callExpression = Expression.Block(callExpression, Expression.Constant(null));
			}
			else
			{
				callExpression = EnsureCastExpression(callExpression, type);
			}

			if (refParameterMap.Count > 0)
			{
				IList<ParameterExpression> variableExpressions = new List<ParameterExpression>();
				IList<Expression> bodyExpressions = new List<Expression>();
				foreach (ByRefParameter p in refParameterMap)
				{
					if (!p.IsOut)
						bodyExpressions.Add(Expression.Assign(p.Variable, p.Value));

					variableExpressions.Add(p.Variable);
				}

				bodyExpressions.Add(callExpression);

				callExpression = Expression.Block(variableExpressions, bodyExpressions);
			}

			return callExpression;
		}

		private static Expression EnsureCastExpression(Expression expression, Type targetType, bool allowWidening = false)
		{
			Type expressionType = expression.Type;

			if (expressionType == targetType || (!expressionType.IsValueType && targetType.IsAssignableFrom(expressionType)))
				return expression;

			if (targetType.IsValueType)
			{
				Expression convert = Expression.Unbox(expression, targetType);

				if (allowWidening && targetType.IsPrimitive)
				{
					MethodInfo toTargetTypeMethod = typeof(Convert).GetMethod("To" + targetType.Name, new[] { typeof(object) })!;

					if (toTargetTypeMethod != null)
					{
						convert = Expression.Condition(
							Expression.TypeIs(expression, targetType),
							convert,
							Expression.Call(toTargetTypeMethod, expression));
					}
				}

				return Expression.Condition(
					Expression.Equal(expression, Expression.Constant(null, typeof(object))),
					Expression.Default(targetType),
					convert);
			}

			return Expression.Convert(expression, targetType);
		}

		private static T[] ArrayEmpty<T>()
			=> EmptyArrayContainer<T>.Empty;

		private static class EmptyArrayContainer<T>
		{
			public static readonly T[] Empty = Array.Empty<T>();
		}
	}
}

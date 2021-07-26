using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using static Raider.Reflection.Delegates.Helper.ParametersIndexes;

namespace Raider.Reflection.Delegates.Extensions
{
	internal static class TypeExtensions
	{
		private static Type? _typeUtils;
		private static Func<Type, Type, bool>? _hasIdentityPrimitiveOrNullableConversionDeleg;
		private static Func<Type, Type, bool>? _hasReferenceConversionDeleg;
		private const string Item = "Item";

		public const string AddAccessor = "add";

		public const string RemoveAccessor = "remove";

		private const BindingFlags PrivateOrProtectedBindingFlags = BindingFlags.NonPublic;

		private const BindingFlags InternalBindingFlags = BindingFlags.Public | BindingFlags.NonPublic;

		public static bool CanBeAssignedFrom(this Type destination, Type source)
		{
			if (source == null || destination == null)
			{
				return false;
			}

			if (destination == source || source.GetTypeInfo().IsSubclassOf(destination))
			{
				return true;
			}

			if (destination.GetTypeInfo().IsInterface)
			{
				return source.ImplementsInterface(destination);
			}

			if (!destination.IsGenericParameter)
			{
				return false;
			}

			var validNewConstraint = IsNewConstraintValid(destination, source);
			var validReferenceConstraint = IsReferenceConstraintValid(destination, source);
			var validValueTypeConstraint = IsValueConstraintValid(destination, source);
			var constraints = destination.GetTypeInfo().GetGenericParameterConstraints();
			return validNewConstraint && validReferenceConstraint && validValueTypeConstraint &&
				constraints.All(t1 => t1.CanBeAssignedFrom(source));
		}

		public static bool IsCrossConstraintInvalid(this Type source, Type[] allGenericArgs, Type[] typeParameters)
		{
			var constraints = source.GetTypeInfo().GetGenericParameterConstraints();
			var invalid = false;
			foreach (var constraint in constraints)
			{
				//if constraint is in collection of other generic parameters types definitions -> cross constraint; check inheritance
				var indexOf = Array.IndexOf(allGenericArgs, constraint);
				if (indexOf > -1)
				{
					var sourceTypeParameter = typeParameters[Array.IndexOf(allGenericArgs, source)];
					var constraintTypeParameter = typeParameters[indexOf];
					if (!constraintTypeParameter.CanBeAssignedFrom(sourceTypeParameter))
					{
						invalid = true;
						break;
					}
				}
			}

			return invalid;
		}

		private static bool IsNewConstraintValid(Type destination, Type source)
		{
			//check new() constraint which is not included in GetGenericParameterConstraints
			var valid = destination.GetTypeInfo().GenericParameterAttributes ==
				GenericParameterAttributes.DefaultConstructorConstraint;
			if (valid)
			{
				valid &= source.GetConstructor(Array.Empty<Type>()) != null;
			}
			else
			{
				valid = true;
			}

			return valid;
		}

		private static bool IsReferenceConstraintValid(Type destination, Type source)
		{
			//check class constraint which is not included in GetGenericParameterConstraints
			var valid = destination.GetTypeInfo().GenericParameterAttributes ==
				GenericParameterAttributes.ReferenceTypeConstraint;
			if (valid)
			{
				valid &= source.GetTypeInfo().IsClass;
			}
			else
			{
				valid = true;
			}

			return valid;
		}

		private static bool IsValueConstraintValid(Type destination, Type source)
		{
			//check new() constraint which is not included in GetGenericParameterConstraints
			var valid = destination.GetTypeInfo().GenericParameterAttributes ==
				GenericParameterAttributes.NotNullableValueTypeConstraint;
			if (valid)
			{
				valid &= source.GetTypeInfo().IsValueType;
			}
			else
			{
				valid = true;
			}

			return valid;
		}

		private static bool ImplementsInterface(this Type? source, Type interfaceType)
		{
			while (source != null)
			{
				var interfaces = source.GetInterfaces();
				if (interfaces.Any(i => i == interfaceType
					|| i.ImplementsInterface(interfaceType)))
				{
					return true;
				}

				source = source.GetTypeInfo().BaseType;
			}

			return false;
		}

		public static bool IsValidReturnType(this Type destination, Type source)
		{
			if (!source.CanBeAssignedFrom(destination)
				&& !source.CanBeConvertedFrom(destination))
			{
				throw new ArgumentException(
					$"Provided return type \'{source.Name}\' is not compatible with expected type " +
					$"\'{destination.Name}\'");
			}

			return true;
		}

		public static bool CanBeConvertedFrom(this Type destination, Type source)
		{
			return HasIdentityPrimitiveOrNullableConversionDeleg?.Invoke(destination, source) == true
				|| HasReferenceConversionDeleg?.Invoke(destination, source) == true;
		}

		internal static Func<Type, Type, bool>? HasReferenceConversionDeleg =>
			_hasReferenceConversionDeleg ??= TypeUtils?.StaticMethod<Func<Type, Type, bool>>("HasReferenceConversionTo");

		internal static Func<Type, Type, bool>? HasIdentityPrimitiveOrNullableConversionDeleg =>
			_hasIdentityPrimitiveOrNullableConversionDeleg ??= TypeUtils?.StaticMethod<Func<Type, Type, bool>>("HasIdentityPrimitiveOrNullableConversionTo");

		internal static Type? TypeUtils
		{
			get
			{
				var t = Type.GetType("System.Dynamic.Utils.TypeUtils, System.Linq.Expressions");
				return _typeUtils ??=
							typeof(Expression)
								.GetTypeInfo()
								.Assembly
								.GetType("System.Dynamic.Utils.TypeUtils", true)
;
			}
		}

		public static List<ParameterExpression> GetParamsExprFromTypes(this Type[] types)
		{
			var parameters = types
				.Select(Expression.Parameter)
				.ToList();
			return parameters;
		}

		private static IEnumerable<MethodInfo> GetAllMethods(this Type source, bool isStatic)
		{
			var staticOrInstance = isStatic ? BindingFlags.Static : BindingFlags.Instance;
			var ms = source.GetTypeInfo().GetMethods(staticOrInstance | BindingFlags.Public)
				.Concat(source.GetTypeInfo().GetMethods(staticOrInstance | PrivateOrProtectedBindingFlags))
				.Concat(source.GetTypeInfo().GetMethods(staticOrInstance | InternalBindingFlags));
			return ms;
		}

		public static MethodInfo? GetGenericMethod(this Type source, string name, Type[]? parametersTypes,
			Type[] typeParameters,
			bool isStatic)
		{
			MethodInfo? methodInfo = null;
			var ms = source.GetAllMethods(isStatic);
			foreach (var m in ms)
			{
				if (m.Name == name && m.IsGenericMethod)
				{
					var parameters = m.GetParameters();
					var genericArguments = m.GetGenericArguments();
					var parametersTypesValid = parameters.Length == parametersTypes?.Length;
					parametersTypesValid &= genericArguments.Length == typeParameters.Length;
					if (!parametersTypesValid)
					{
						continue;
					}

					for (var index = 0; index < parameters.Length; index++)
					{
						var parameterInfo = parameters[index];
						var parameterType = parametersTypes[index];
						if (parameterInfo.ParameterType != parameterType
							&& parameterInfo.ParameterType.IsGenericParameter
							&& !parameterInfo.ParameterType.CanBeAssignedFrom(parameterType))
						{
							parametersTypesValid = false;
							break;
						}
					}

					for (var index = 0; index < genericArguments.Length; index++)
					{
						var genericArgument = genericArguments[index];
						var typeParameter = typeParameters[index];
						if (!genericArgument.CanBeAssignedFrom(typeParameter)
							//check cross parameters constraints
							|| genericArgument.IsCrossConstraintInvalid(genericArguments, typeParameters))
						{
							parametersTypesValid = false;
							break;
						}
					}

					if (parametersTypesValid)
					{
						methodInfo = m.MakeGenericMethod(typeParameters);
						break;
					}
				}
			}

			return methodInfo;
		}

		public static MethodInfo? GetMethodInfoByEnumerate(this Type source, string name, Type[]? parametersTypes,
			bool isStatic)
		{
			var methods = source.GetAllMethods(isStatic).Where(m => m.Name == name && !m.IsGenericMethod);
			foreach (var method in methods)
			{
				var methodInfo = CheckMethodInfoParameters(parametersTypes, method);
				if (methodInfo != null)
				{
					return methodInfo;
				}
			}

			return null;
		}

		private static MethodInfo? CheckMethodInfoParameters(Type[]? parametersTypes, MethodInfo method)
		{
			var parameters = method.GetParameters();
			//TODO: test if instance method with the same number parameters but different types will not break this code
			var parametersTypesValid = parameters.Length == parametersTypes?.Length;
			if (!parametersTypesValid)
			{
				return null;
			}

			for (var index = 0; index < parameters.Length; index++)
			{
				var parameterInfo = parameters[index];
				var parameterType = parametersTypes[index];
				if (parameterInfo.ParameterType != parameterType)
				{
					parametersTypesValid = false;
					break;
				}
			}

			if (parametersTypesValid)
			{
				return method;
			}

			return null;
		}

		public static MethodInfo? GetMethodInfo(this Type source, string name, Type[]? parametersTypes,
			Type[]? typeParameters = null, bool isStatic = false)
		{
			MethodInfo? methodInfo = null;
			if (typeParameters == null || typeParameters.Length == 0)
			{
                methodInfo = GetMethodInfoByEnumerate(source, name, parametersTypes, isStatic);
			}
			//check for generic methods
			else
			{
				methodInfo = GetGenericMethod(source, name, parametersTypes, typeParameters, isStatic);
			}

			return methodInfo;
		}

		public static PropertyInfo? GetPropertyInfo(this Type source, string name, bool isStatic)
		{
			var staticOrInstance = isStatic ? BindingFlags.Static : BindingFlags.Instance;
			var typeInfo = source.GetTypeInfo();
			var propertyInfo = typeInfo.GetProperty(name, staticOrInstance) ??
				typeInfo.GetProperty(name, staticOrInstance | PrivateOrProtectedBindingFlags) ??
				typeInfo.GetProperty(name, staticOrInstance | InternalBindingFlags);
			return propertyInfo;
		}

		public static FieldInfo? GetFieldInfo(this Type source, string fieldName, bool isStatic)
		{
			var staticOrInstance = isStatic ? BindingFlags.Static : BindingFlags.Instance;
			var typeInfo = source.GetTypeInfo();
			var fieldInfo = (typeInfo.GetField(fieldName, staticOrInstance) ??
					typeInfo.GetField(fieldName, staticOrInstance | PrivateOrProtectedBindingFlags)) ??
				typeInfo.GetField(fieldName, staticOrInstance | InternalBindingFlags);
			return fieldInfo;
		}

		private static IEnumerable<PropertyInfo> GetAllProperties(this Type source)
		{
			var properties = source.GetProperties().Concat(
					source.GetProperties(BindingFlags.NonPublic)).Concat(
					source.GetProperties(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance))
				.ToArray();
			return properties;
		}

		public static PropertyInfo? GetIndexerPropertyInfo(this Type source, Type[] indexesTypes, string? indexerName = null)
		{
			indexerName ??= Item;
			var properties = source.GetAllProperties().ToArray();
			if (indexerName == Item)
			{
				var firstIndexerInfo = properties.FirstOrDefault(p => p.GetIndexParameters().Length > 0);
				if (firstIndexerInfo != null && firstIndexerInfo.Name != indexerName)
				{
					indexerName = firstIndexerInfo.Name;
				}
			}

			var indexerInfo = properties.FirstOrDefault(p => p.Name == indexerName
				&& IndexParametersEquals(p.GetIndexParameters(), indexesTypes));
			if (indexerInfo != null)
			{
				return indexerInfo;
			}

			return null;
		}

		private static IEnumerable<ConstructorInfo> GetAllConstructors(this Type source)
		{
			var constructors = source.GetTypeInfo().GetConstructors(BindingFlags.Public).Concat(
				source.GetTypeInfo().GetConstructors(PrivateOrProtectedBindingFlags)).Concat(
				source.GetTypeInfo().GetConstructors(InternalBindingFlags | BindingFlags.Instance));
			return constructors;
		}

		public static ConstructorInfo? GetConstructorInfo(this Type source, Type[] types)
		{
			ConstructorInfo? constructor = null;
			var constructors = source.GetAllConstructors();
			foreach (var c in constructors)
			{
				var parameters = c.GetParameters();
				var parametersTypesValid = parameters.Length == types.Length;
				if (!parametersTypesValid)
				{
					continue;
				}

				for (var index = 0; index < parameters.Length; index++)
				{
					var parameterInfo = parameters[index];
					var parameterType = types[index];
					if (parameterInfo.ParameterType != parameterType)
					{
						parametersTypesValid = false;
						break;
					}
				}

				if (parametersTypesValid)
				{
					constructor = c;
					break;
				}
			}
			return constructor;
		}

		private static EventInfo? GetEventByName(this Type source, string eventName)
		{
			return (source.GetTypeInfo().GetEvent(eventName)
					?? source.GetTypeInfo().GetEvent(eventName, PrivateOrProtectedBindingFlags))
				?? source.GetTypeInfo().GetEvent(eventName,
					InternalBindingFlags | BindingFlags.Instance);
		}

		public static EventInfo? GetEventInfo(this Type sourceType, string eventName)
		{
			var eventInfo = sourceType.GetEventByName(eventName);
			return eventInfo;
		}

		public static MethodInfo? GetEventAccessor(this Type sourceType, string eventName, string accessor)
		{
			var eventInfo = sourceType.GetEventInfo(eventName);
			return accessor == AddAccessor ? eventInfo?.AddMethod : eventInfo?.RemoveMethod;
		}
	}
}

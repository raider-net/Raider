﻿using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Linq;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Raider.Extensions
{
	public static class TypeExtensions
	{
		/*
         * PRIMITIVE TYPES
         http://msdn.microsoft.com/en-us/library/system.type.isprimitive%28v=vs.110%29.aspx
         The primitive types are Boolean, Byte, SByte, Int16, UInt16, Int32, UInt32, Int64, UInt64, IntPtr, UIntPtr, Char, Double, and Single.
          
         VALUE TYPES
         http://msdn.microsoft.com/en-us/library/bfft1t3c.aspx
         */

		private static readonly Dictionary<string, string> baseTypeStrings = new Dictionary<string, string>()
		{
			{"bool", "bool"},
			{"Boolean", "bool"},
			{"System.Boolean", "bool"},
			{"date", "DateTime"},
			{"DateTime", "DateTime"},
			{"System.DateTime", "DateTime"},
			{"byte", "byte"},
			{"Byte", "byte"},
			{"System.Byte", "byte"},
			{"decimal", "decimal"},
			{"Decimal", "decimal"},
			{"System.Decimal", "decimal"},
			{"double", "double"},
			{"Double", "double"},
			{"System.Double", "double"},
			{"float", "float"},
			{"Single", "float"},
			{"System.Single", "float"},
			{"short", "short"},
			{"Int16", "short"},
			{"System.Int16", "short"},
			{"int", "int"},
			{"Int32", "int"},
			{"System.Int32", "int"},
			{"long", "long"},
			{"Int64", "long"},
			{"System.Int64", "long"},
			{"char", "char"},
			{"Char", "char"},
			{"System.Char", "char"},
			{"string", "string"},
			{"String", "string"},
			{"System.String", "string"},
			{"sbyte", "sbyte"},
			{"SByte", "sbyte"},
			{"System.SByte", "sbyte"},
			{"ushort", "ushort"},
			{"UInt16", "ushort"},
			{"System.UInt16", "ushort"},
			{"uint", "uint"},
			{"UInt32", "uint"},
			{"System.UInt32", "uint"},
			{"ulong", "ulong"},
			{"UInt64", "ulong"},
			{"System.UInt64", "ulong"},
			{"void", "void"},
			{"Void", "void"},
			{"System.Void", "void"}
		};

		private static readonly Dictionary<Type, string> typesMap = new Dictionary<Type, string>() {
			{ typeof(void), "void" },
			{ typeof(bool), "bool" },
			{ typeof(char), "char" },
			{ typeof(byte), "byte" },
			{ typeof(sbyte), "sbyte" },
			{ typeof(short), "short" },
			{ typeof(ushort), "ushort" },
			{ typeof(int), "int" },
			{ typeof(uint), "uint" },
			{ typeof(long), "long" },
			{ typeof(ulong), "ulong" },
			{ typeof(float), "float" },
			{ typeof(double), "double" },
			{ typeof(decimal), "decimal" },
			{ typeof(string), "string" },
			{ typeof(object), "object" }
		};

		public static string? GetShortValueTypeName(this Type type)
		{
			if (type == null)
				return null;

			typesMap.TryGetValue(type, out var result);
			return result;
		}

		public static bool IsBaseType(this Type type)
		{
			if (type == null)
				return false;
			
			return baseTypeStrings.ContainsKey(type.Name);
		}

		public static bool IsNullable(this Type type)
		{
			if (type == null)
				return false;

			if (!type.IsValueType)
				return true; // ref-type

			if (Nullable.GetUnderlyingType(type) != null)
				return true; // Nullable<T>

			return false; // value-type
		}

		public static bool IsNullableType(this Type type)
		{
			var typeInfo = type.GetTypeInfo();

			return !typeInfo.IsValueType
					|| (typeInfo.IsGenericType
						&& typeInfo.GetGenericTypeDefinition() == typeof(Nullable<>));
		}

		public static Type ToNullable(this Type type)
			=> IsNullable(type)
				? type
				: typeof(Nullable<>).MakeGenericType(type);

		public static bool IsDelegate(this Type type)
		{
			if (type == null)
				return false;

			return type == typeof(MulticastDelegate)
					|| type.IsSubclassOf(typeof(Delegate))
					|| type == typeof(Delegate);
		}

		public static bool IsStruct(this Type type)
		{
			if (type == null)
				return false;

			return type.IsValueType
					&& !type.IsPrimitive
					&& !type.IsEnum
					&& type != typeof(decimal);
		}

		public static bool IsEquivalentNullableType(this Type mainType, Type type)
		{
			if (mainType == null || type == null)
				return mainType == null && type == null;

			Type mainT = mainType.IsNullable()
				? (Nullable.GetUnderlyingType(mainType) ?? mainType)
				: mainType;

			Type t = type.IsNullable()
				? (Nullable.GetUnderlyingType(type) ?? type)
				: type;

			return mainT == t;
		}

		[return: NotNullIfNotNull("mainType")]
		public static Type? GetUnderlyingNullableType(this Type mainType)
		{
			if (mainType == null)
				return null;

			Type type = mainType.IsNullable()
				? (Nullable.GetUnderlyingType(mainType) ?? mainType)
				: mainType;

			return type;
		}

		public static Type GetGenericTypeDefinitionIfExists(this Type type)
		{
			if (type == null)
				throw new ArgumentNullException(nameof(type));

			return type.IsGenericTypeDefinition
				? type
				: (type.IsGenericType
					? type.GetGenericTypeDefinition()
					: type);
		}

		public static bool HasSameTypeDefinition(this Type type, Type otherType)
		{
			if (type == null)
				throw new ArgumentNullException(nameof(type));

			if (otherType == null)
				return false;

			var typeDefinition = type.GetGenericTypeDefinitionIfExists();
			var otherTypeDefinition = otherType.GetGenericTypeDefinitionIfExists();

			return typeDefinition == otherTypeDefinition;
		}

		public static string ToFriendlyFullName(this Type type)
		{
			return ToFriendlyName(type, false, true, true, false, true);
		}

		public static string ToFriendlyName(this Type type, bool useShortValueTypes = true, bool showGenericArguments = true, bool showReflectedType = true, bool compactNullable = false, bool showFullNames = false)
		{
			if (type == null)
				return string.Empty;

			StringBuilder sb = new();
			BuildfriendlyName(sb, type, useShortValueTypes, showGenericArguments, showReflectedType, compactNullable, showFullNames);
			return sb.ToString();
		}

		private static void BuildfriendlyName(StringBuilder builder, Type type, bool useShortValueTypes, bool showGenericArguments, bool showReflectedType, bool compactNullable, bool showFullNames)
		{
			bool isBasic = true;
			if (showReflectedType && type.IsNested && !type.IsGenericParameter)
			{
				BuildfriendlyName(builder, type.ReflectedType, useShortValueTypes, showGenericArguments, showReflectedType, compactNullable, showFullNames);
				builder.Append('.');
			}
			if (type.IsArray)
			{
				isBasic = false;
				BuildfriendlyName(builder, type.GetElementType(), useShortValueTypes, showGenericArguments, showReflectedType, compactNullable, showFullNames);
				builder.Append('[');
				for (int rank = type.GetArrayRank(); rank > 1; --rank) builder.Append(',');
				builder.Append(']');
			}
			if (type.IsPointer)
			{
				isBasic = false;
				BuildfriendlyName(builder, type.GetElementType(), useShortValueTypes, showGenericArguments, showReflectedType, compactNullable, showFullNames);
				builder.Append('*');
			}
			if (type.IsGenericParameter)
			{
				isBasic = false;
				GenericParameterAttributes gpAttributes = type.GenericParameterAttributes;
				if ((gpAttributes & GenericParameterAttributes.Covariant) == GenericParameterAttributes.Covariant)
				{
					builder.Append("out ");
				}
				else if ((gpAttributes & GenericParameterAttributes.Contravariant) == GenericParameterAttributes.Contravariant)
				{
					builder.Append("in ");
				}
				if (showFullNames && !string.IsNullOrWhiteSpace(type.FullName)) //type.FullName == null pre T napr. pri type Nullable<T> / Nullable<>
				{
					builder.Append(type.FullName);
				}
				else
				{
					builder.Append(type.Name);
				}
			}
			if (type.IsGenericType)
			{
				isBasic = false;
				string name;
				if (showFullNames && !string.IsNullOrWhiteSpace(type.FullName)) //type.FullName == null pre T napr. pri type Nullable<T> / Nullable<>
				{
					name = type.FullName;
				}
				else
				{
					name = type.Name;
				}
				int index = name.IndexOf('`');
				if (index == -1) index = name.Length;
				name = name.Substring(0, index);
				if (type.IsSealed && type.Namespace == null && name.Contains("AnonymousType"))
				{
					builder.Append(name);
				}
				else if (type.IsGenericTypeDefinition)
				{
					builder.Append(name);
					if (showGenericArguments)
					{
						builder.Append('<');
						Type[] args = type.GetGenericArguments();
						for (int i = 0; i < args.Length; ++i)
						{
							if (i > 0) builder.Append(", ");
							BuildfriendlyName(builder, args[i], useShortValueTypes, showGenericArguments, showReflectedType, compactNullable, showFullNames);
						}
						builder.Append('>');
					}
				}
				else
				{
					bool isNullable = compactNullable && type.GetGenericTypeDefinition() == typeof(Nullable<>);
					if (isNullable)
					{
						BuildfriendlyName(builder, Nullable.GetUnderlyingType(type), useShortValueTypes, showGenericArguments, showReflectedType, compactNullable, showFullNames);
						builder.Append('?');
					}
					else
					{
						builder.Append(name);
						if (showGenericArguments)
						{
							builder.Append('<');
							Type[] args = type.GetGenericArguments();
							for (int i = 0; i < args.Length; ++i)
							{
								if (i > 0) builder.Append(", ");
								BuildfriendlyName(builder, args[i], useShortValueTypes, showGenericArguments, showReflectedType, compactNullable, showFullNames);
							}
							builder.Append('>');
						}
					}
				}
			}
			if (isBasic)
			{
				string? valueType =
					useShortValueTypes
						? GetShortValueTypeName(type)
						: null;

				if (showFullNames && !string.IsNullOrWhiteSpace(type.FullName)) //type.FullName == null pre T napr. pri type Nullable<T> / Nullable<>
				{
					builder.Append(valueType ?? type.FullName);
				}
				else
				{
					builder.Append(valueType ?? type.Name);
				}
			}
		}

		public static T? GetFirstAttribute<T>(this Type type, bool inherit = true) where T : System.Attribute
		{
			if (type == null)
				return default;

			var result = type.GetCustomAttributes(typeof(T), inherit);
			return
				result != null
					? result.FirstOrDefault() as T
					: null;
		}

		public static T[]? GetAttributeList<T>(this Type type, bool inherit = true) where T : System.Attribute
		{
			if (type == null)
				return default;

			var result = type.GetCustomAttributes(typeof(T), inherit);
			return
				result != null
					? result as T[]
					: null;
		}

		/// <summary>
		/// Returns true if the supplied <paramref name="type"/> inherits from the given class <typeparamref name="T"/>.
		/// </summary>
		/// <typeparam name="T">The type (class) to check for.</typeparam>
		/// <param name="type">The type to check.</param>
		/// <returns>True if the given type inherits from the specified class.</returns>
		/// <remarks>This method is for classes only. Use <seealso cref="Implements"/> for interface types and <seealso cref="InheritsOrImplements"/> 
		/// to check both interfaces and classes.</remarks>
		public static bool Inherits<T>(this Type type)
		{
			return type.Inherits(typeof(T));
		}

		/// <summary>
		/// Returns true if the supplied <paramref name="type"/> inherits from the given class <paramref name="baseType"/>.
		/// </summary>
		/// <param name="baseType">The type (class) to check for.</param>
		/// <param name="type">The type to check.</param>
		/// <returns>True if the given type inherits from the specified class.</returns>
		/// <remarks>This method is for classes only. Use <seealso cref="Implements"/> for interface types and <seealso cref="InheritsOrImplements"/> 
		/// to check both interfaces and classes.</remarks>
		public static bool Inherits(this Type type, Type baseType)
		{
			if (baseType == null || type == null || type == baseType)
				return false;
			var rootType = typeof(object);
			if (baseType == rootType)
				return true;
			while (type != null && type != rootType)
			{
				var current = type.IsGenericType && baseType.IsGenericTypeDefinition ? type.GetGenericTypeDefinition() : type;
				if (baseType == current)
					return true;
				type = type.BaseType;
			}
			return false;
		}

		/// <summary>
		/// Returns true if the supplied <paramref name="type"/> implements the given interface <typeparamref name="T"/>.
		/// </summary>
		/// <typeparam name="T">The type (interface) to check for.</typeparam>
		/// <param name="type">The type to check.</param>
		/// <returns>True if the given type implements the specified interface.</returns>
		/// <remarks>This method is for interfaces only. Use <seealso cref="Inherits"/> for class types and <seealso cref="InheritsOrImplements"/> 
		/// to check both interfaces and classes.</remarks>
		public static bool Implements<T>(this Type type)
		{
			return type.Implements(typeof(T));
		}

		/// <summary>
		/// Returns true of the supplied <paramref name="type"/> implements the given interface <paramref name="interfaceType"/>. If the given
		/// interface type is a generic type definition this method will use the generic type definition of any implemented interfaces
		/// to determine the result.
		/// </summary>
		/// <param name="interfaceType">The interface type to check for.</param>
		/// <param name="type">The type to check.</param>
		/// <returns>True if the given type implements the specified interface.</returns>
		/// <remarks>This method is for interfaces only. Use <seealso cref="Inherits"/> for classes and <seealso cref="InheritsOrImplements"/> 
		/// to check both interfaces and classes.</remarks>
		public static bool Implements(this Type type, Type interfaceType)
		{
			if (type == null || interfaceType == null || type == interfaceType || !interfaceType.IsInterface)
				return false;

			if (interfaceType.IsGenericTypeDefinition && type.GetInterfaces().Where(t => t.IsGenericType).Select(t => t.GetGenericTypeDefinition()).Any(gt => gt == interfaceType))
				return true;

			return interfaceType.IsAssignableFrom(type);
		}

		/// <summary>
		/// Returns true if the supplied <paramref name="type"/> inherits from or implements the type <typeparamref name="T"/>.
		/// </summary>
		/// <typeparam name="T">The base type to check for.</typeparam>
		/// <param name="type">The type to check.</param>
		/// <returns>True if the given type inherits from or implements the specified base type.</returns>
		public static bool InheritsOrImplements<T>(this Type type)
		{
			return type.InheritsOrImplements(typeof(T));
		}

		/// <summary>
		/// Returns true of the supplied <paramref name="type"/> inherits from or implements the type <paramref name="baseType"/>.
		/// </summary>
		/// <param name="baseType">The base type to check for.</param>
		/// <param name="type">The type to check.</param>
		/// <returns>True if the given type inherits from or implements the specified base type.</returns>
		public static bool InheritsOrImplements(this Type type, Type baseType)
		{
			if (type == null || baseType == null)
				return false;
			return baseType.IsInterface ? type.Implements(baseType) : type.Inherits(baseType);
		}

		public static IEnumerable<Type> GetBaseTypes(this Type type)
		{
			if (type == null)
				yield break;

			var currentBaseType = type.BaseType;
			while (currentBaseType != null)
			{
				yield return currentBaseType;
				currentBaseType = currentBaseType.BaseType;
			}
		}

		public static IEnumerable<Type> GetBaseTypesAndInterfaces(this Type type)
		{
			if (type == null)
				yield break;

			var currentBaseType = type.BaseType;
			while (currentBaseType != null)
			{
				yield return currentBaseType;
				currentBaseType = currentBaseType.BaseType;
			}

			foreach (var ifc in type.GetInterfaces())
			{
				yield return ifc;
			}
		}

		public static object? GetDefaultValue(this Type type)
		{
			if (type.IsValueType && Nullable.GetUnderlyingType(type) == null)
			{
				return Activator.CreateInstance(type);
			}
			else
			{
				return null;
			}
		}

		public static object? GetDefaultNullableValue(this Type type)
		{
			if (type.IsValueType)
			{
				return Activator.CreateInstance(GetUnderlyingNullableType(type));
			}
			else
			{
				return null;
			}
		}

		public static ConstructorInfo? GetDefaultConstructor(this Type type)
			=> type?.GetConstructors().FirstOrDefault(t => t.GetParameters().Count() == 0);

		public static bool CanBeCastTo(this Type type, Type toType)
		{
			if (type == null)
				return false;

			if (type == toType)
				return true;

			return toType.GetTypeInfo().IsAssignableFrom(type.GetTypeInfo());
		}

		/// <summary>
		/// examples: Class1<> Class2<,> Class3<,,> Interface1<> Interface2<,> Interface3<,,>...
		/// </summary>
		public static bool IsOpenGeneric(this Type type)
			=> type != null
				&& (type.GetTypeInfo().IsGenericTypeDefinition
					|| type.GetTypeInfo().ContainsGenericParameters);

		public static bool IsInstanceable(this Type type)
			=> type != null
				&& !type.GetTypeInfo().IsAbstract
				&& !type.GetTypeInfo().IsInterface;
	}
}

using Raider.Extensions;
using System;
using System.Collections.Generic;

namespace Raider.Reflection
{
	public static class TypeHelper
	{
		public static readonly Dictionary<Type, int> TypeSizeInBytes = new Dictionary<Type, int>()
		{
			{ typeof(bool), 1 },
			{ typeof(byte), 1 },
			{ typeof(char), 2 },
			{ typeof(DateTime), 8 },
			{ typeof(decimal), 16 },
			{ typeof(double), 8 },
			{ typeof(float), 4 },
			{ typeof(int), 4 },
			{ typeof(long), 8 },
			{ typeof(sbyte), 1 },
			{ typeof(short), 2 },
			{ typeof(uint), 4 },
			{ typeof(ulong), 8 },
			{ typeof(ushort), 2 }
		};

		public static bool IsDerivedFrom(Type type, Type baseType)
		{
			if (type == null)
				throw new System.ArgumentNullException(nameof(type));
			if (baseType == null)
				throw new System.ArgumentNullException(nameof(baseType));

			return IsDerivedFromInternal(type, baseType);
		}

		private static bool IsDerivedFromInternal(Type type, Type baseType)
		{
			if (type == baseType || type.HasTheSameTypeDefinition(baseType) || baseType.IsAssignableFrom(type))
			{
				return true;
			}
			else
			{
				if (type.BaseType == null || type.BaseType == typeof(object))
				{
					return baseType == typeof(object);
				}
				else
				{
					return IsDerivedFromInternal(type.BaseType, baseType);
				}
			}
		}

		public static bool IsDeclaredIn(Type type, Type otherType)
		{
			if (type == null)
			{
				throw new System.ArgumentNullException(nameof(type));
			}
			if (otherType == null)
			{
				throw new System.ArgumentNullException(nameof(otherType));
			}
			return IsDeclaredInInternal(type, otherType);
		}

		private static bool IsDeclaredInInternal(Type type, Type otherType)
		{
			if (type == otherType)
			{
				return true;
			}
			else
			{
				if (type.DeclaringType == null || type.DeclaringType == typeof(object))
				{
					return otherType == typeof(object);
				}
				else
				{
					return IsDeclaredInInternal(type.DeclaringType, otherType);
				}
			}
		}

		public static T? GetDefaultValue<T>()
			=> default;

		public static T GetDefaultValueForNullable<T>()
			where T : struct
			=> typeof(T).IsNullable()
				? new T?().GetValueOrDefault()
				: default;
	}
}

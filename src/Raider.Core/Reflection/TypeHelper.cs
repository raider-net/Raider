using Raider.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Raider.Reflection
{
	public static class TypeHelper
	{
		public static readonly IReadOnlyDictionary<Type, int> TypeSizeInBytes;

		static TypeHelper()
		{
			TypeSizeInBytes = new Dictionary<Type, int>()
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
		}

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
			if (type == baseType || type.HasSameTypeDefinition(baseType) || baseType.IsAssignableFrom(type))
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

		public static List<KeyValuePair<Type, Type>> GetImplementationsToTypesClosingOpenInterface(
			Type openInterface,
			IEnumerable<Assembly> assembliesToScan,
			bool addIfAlreadyExists)
		{
			var result = new List<KeyValuePair<Type, Type>>();
			var instanceables = new List<Type>();
			var interfaces = new List<Type>();
			foreach (var type in assembliesToScan.SelectMany(a => a.DefinedTypes).Where(t => !t.IsOpenGeneric()))
			{
				var interfaceTypes = type.FindInterfacesThatCloses(openInterface).ToArray();
				if (!interfaceTypes.Any()) continue;

				if (type.IsInstanceable())
					instanceables.Add(type);

				foreach (var interfaceType in interfaceTypes)
					interfaces.AddUniqueItem(interfaceType);
			}

			foreach (var @interface in interfaces)
			{
				var exactMatches = instanceables.Where(x => x.CanBeCastTo(@interface)).ToList();
				if (addIfAlreadyExists)
				{
					foreach (var type in exactMatches)
						result.Add(new KeyValuePair<Type, Type>(@interface, type));
				}
				else
				{
					if (exactMatches.Count > 1)
						exactMatches.RemoveAll(m => !IsMatchingWithInterface(m, @interface));

					foreach (var type in exactMatches)
						result.Add(new KeyValuePair<Type, Type>(@interface, type));
				}

				if (!@interface.IsOpenGeneric())
				{
					foreach (var instanceable in instanceables.Where(x => x.IsOpenGeneric() && x.CouldCloseTo(@interface)))
					{
						try
						{
							result.Add(new KeyValuePair<Type, Type>(@interface, instanceable.MakeGenericType(@interface.GenericTypeArguments)));
						}
						catch { }
					}
				}
			}

			return result;
		}

		private static bool IsMatchingWithInterface(Type handlerType, Type handlerInterface)
		{
			if (handlerType == null || handlerInterface == null)
			{
				return false;
			}

			if (handlerType.IsInterface)
			{
				if (handlerType.GenericTypeArguments.SequenceEqual(handlerInterface.GenericTypeArguments))
				{
					return true;
				}
			}
			else
			{
				return IsMatchingWithInterface(handlerType.GetInterface(handlerInterface.Name), handlerInterface);
			}

			return false;
		}

		private static bool CouldCloseTo(this Type openInstanceables, Type closedInterface)
		{
			var openInterface = closedInterface.GetGenericTypeDefinition();
			var arguments = closedInterface.GenericTypeArguments;

			var instanceableArguments = openInstanceables.GenericTypeArguments;
			return arguments.Length == instanceableArguments.Length && openInstanceables.CanBeCastTo(openInterface);
		}

		private static IEnumerable<Type> FindInterfacesThatCloses(this Type type, Type genericTypeDefinition)
			=> FindInterfacesThatClosesInternal(type, genericTypeDefinition).Distinct();

		private static IEnumerable<Type> FindInterfacesThatClosesInternal(Type type, Type genericTypeDefinition)
		{
			if (type == null)
				yield break;

			if (!type.IsInstanceable())
				yield break;

			var baseType = type.GetTypeInfo().BaseType;

			if (genericTypeDefinition.GetTypeInfo().IsInterface)
			{
				foreach (var interfaceType in type.GetInterfaces().Where(type => type.GetTypeInfo().IsGenericType && (type.GetGenericTypeDefinition() == genericTypeDefinition)))
					yield return interfaceType;
			}
			else if (baseType.GetTypeInfo().IsGenericType
					&& (baseType.GetGenericTypeDefinition() == genericTypeDefinition))
			{
				yield return baseType;
			}

			if (baseType == typeof(object))
				yield break;

			foreach (var interfaceType in FindInterfacesThatClosesInternal(baseType, genericTypeDefinition))
				yield return interfaceType;
		}
	}
}

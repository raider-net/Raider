using Raider.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Raider.Extensions
{
	public static class MemberInfoExtensions
	{
		public static T? GetFirstAttribute<T>(this MemberInfo mi, bool inherit = true)
			where T : Attribute
		{
			if (mi == null) return default;
			var result = mi.GetCustomAttributes(typeof(T), inherit);
			return result != null ? result.FirstOrDefault() as T : null;
		}

		public static T[]? GetAttributeList<T>(this MemberInfo mi, bool inherit = true)
			where T : Attribute
		{
			if (mi == null) return default;
			var result = mi.GetCustomAttributes(typeof(T), inherit);
			return result != null ? result as T[] : null;
		}

		public static MemberInfo GetFirstMemberInfoWithAttribute<T>(this IEnumerable<MemberInfo> infoList, Func<T, bool> attributeMatch, bool inherit = true)
			where T : Attribute
		{
			return ReflectionHelper.GetFirstMemberInfoWithAttribute<T>(infoList, attributeMatch, inherit);
		}

		public static List<MemberInfo> GetAllMemberInfosWithAttribute<T>(this IEnumerable<MemberInfo> infoList, Func<T, bool> attributeMatch, bool inherit = true)
			where T : Attribute
		{
			return ReflectionHelper.GetAllMemberInfosWithAttribute<T>(infoList, attributeMatch, inherit);
		}

		/// <summary>
		/// Determines whether the given <paramref name="member"/> is a static member.
		/// </summary>
		/// <returns>True for static fields, properties and methods and false for instance fields,
		/// properties and methods. Throws an exception for all other <see href="MemberTypes" />.</returns>
		public static bool IsStatic(this MemberInfo member)
		{
			var field = member as FieldInfo;
			if (field != null)
				return field.IsStatic;
			var property = member as PropertyInfo;
			if (property != null)
				return property.CanRead
					? (property.GetGetMethod(true)?.IsStatic ?? false)
					: (property.GetSetMethod(true)?.IsStatic ?? false);
			var method = member as MethodInfo;
			if (method != null)
				return method.IsStatic;
			string message = string.Format("Unable to determine IsStatic for member {0}.{1}" +
				"MemberType was {2} but only fields, properties and methods are supported.",
				member.Name, member.MemberType, Environment.NewLine);
			throw new NotSupportedException(message);
		}

		/// <summary>
		/// Gets the system type of the field or property identified by the <paramref name="member"/>.
		/// </summary>
		/// <returns>The system type of the member.</returns>
		public static Type GetFieldOrPropertyType(this MemberInfo member)
		{
			if (member == null)
				throw new ArgumentNullException(nameof(member));

			if (member.MemberType == MemberTypes.Property)
			{
				var property = member as PropertyInfo;
				if (property != null)
				{
					return property.PropertyType;
				}
			}

			if (member.MemberType == MemberTypes.Field)
			{
				var field = member as FieldInfo;
				if (field != null)
				{
					return field.FieldType;
				}
			}
			throw new NotSupportedException("Can only determine the type for fields and properties.");
		}

		/// <summary>
		/// Gets the return type of an member.
		/// </summary>
		/// <param name="member">The member.</param>
		/// <returns></returns>
		/// <exception cref="System.NotSupportedException">Unable to get return type of member of type  + member.MemberType</exception>
		public static Type GetReturnType(this MemberInfo member)
		{
			switch (member)
			{
				case PropertyInfo propertyInfo:
					return propertyInfo.PropertyType;
				case MethodInfo methodInfo:
					return methodInfo.ReturnType;
				case FieldInfo fieldInfo:
					return fieldInfo.FieldType;
			}

			throw new NotSupportedException("Unable to get return type of member of type " + member.GetType().Name);
		}
	}
}

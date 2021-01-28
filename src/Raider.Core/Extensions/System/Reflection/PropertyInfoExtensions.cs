using Raider.Reflection;
using System;
using System.Linq;
using System.Reflection;

namespace Raider.Extensions
{
	public static class PropertyInfoExtensions
	{
		public static T? GetFirstAttribute<T>(this PropertyInfo pi, bool inherit = true) where T : Attribute
		{
			if (pi == null) return default;
			var result = pi.GetCustomAttributes(typeof(T), inherit);
			return result != null ? result.FirstOrDefault() as T : null;
		}

		public static T[]? GetAttributeList<T>(this PropertyInfo pi, bool inherit = true) where T : Attribute
		{
			if (pi == null) return default;
			var result = pi.GetCustomAttributes(typeof(T), inherit);
			return result != null ? result as T[] : null;
		}

		public static bool IsStaticProperty(this PropertyInfo propertyInfo)
		{
			return PropertyInfoHelper.IsStatic(propertyInfo);
		}
	}
}

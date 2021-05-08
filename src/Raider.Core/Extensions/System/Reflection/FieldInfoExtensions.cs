using System;
using System.Linq;
using System.Reflection;

namespace Raider.Extensions
{
	public static class FieldInfoExtensions
	{
		public static T? GetFirstAttribute<T>(this FieldInfo fi, bool inherit = true) where T : Attribute
		{
			if (fi == null) return default;
			var result = fi.GetCustomAttributes(typeof(T), inherit);
			return result != null ? result.FirstOrDefault() as T : null;
		}

		public static T[]? GetAttributeList<T>(this FieldInfo fi, bool inherit = true) where T : Attribute
		{
			if (fi == null) return default;
			var result = fi.GetCustomAttributes(typeof(T), inherit);
			return result != null ? result as T[] : null;
		}

		public static bool IsConst(this FieldInfo fi)
		{
			if (fi == null) return false;
			return fi.IsLiteral && !fi.IsInitOnly;
		}
	}
}

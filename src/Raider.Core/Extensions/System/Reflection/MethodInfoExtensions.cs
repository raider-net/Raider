using System;
using System.Linq;
using System.Reflection;

namespace Raider.Extensions
{
	public static class MethodInfoExtensions
	{
		public static T? GetFirstAttribute<T>(this MethodInfo methodInfo, bool inherit = true) where T : System.Attribute
		{
			if (methodInfo == null) return default;
			var result = methodInfo.GetCustomAttributes(typeof(T), inherit);
			return result != null ? result.FirstOrDefault() as T : null;
		}

		public static T[]? GetAttributeList<T>(this MethodInfo methodInfo, bool inherit = true) where T : System.Attribute
		{
			if (methodInfo == null) return default;
			var result = methodInfo.GetCustomAttributes(typeof(T), inherit);
			return result != null ? result as T[] : null;
		}

		public static bool IsAwaitable(this MethodInfo methodInfo)
		{
			if (methodInfo == null)
				throw new ArgumentNullException(nameof(methodInfo));

			//var asyncAttribute = methodInfo.GetFirstAttribute<Runtime.CompilerServices.AsyncStateMachineAttribute>(true);
			//return asyncAttribute != null;

			return methodInfo.ReturnType?.GetMethod("GetAwaiter") != null;
		}
	}
}

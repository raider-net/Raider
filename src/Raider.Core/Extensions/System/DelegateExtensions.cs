using System;

namespace Raider.Extensions
{
	public static class DelegateExtensions
	{
		public static Func<object, object?> ToNonGeneric<T, TProperty>(this Func<T, TProperty> func)
		{
			return x => func((T)x);
		}

		public static Func<object, bool> ToNonGeneric<T>(this Func<T, bool> func)
		{
			return x => func((T)x);
		}
	}
}

using System;
using System.Reflection;

namespace Raider.Reflection
{
	public static class PropertyInfoHelper
	{
		public static bool IsStatic(PropertyInfo propertyInfo)
		{
			if (propertyInfo == null)
				throw new ArgumentNullException(nameof(propertyInfo));

			return propertyInfo.CanRead
				? (propertyInfo.GetGetMethod(true)?.IsStatic ?? false)
				: (propertyInfo.GetSetMethod(true)?.IsStatic ?? false);
		}
	}
}

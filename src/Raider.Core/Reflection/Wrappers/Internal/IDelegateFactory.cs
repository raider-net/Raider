using System;
using System.Reflection;

namespace Raider.Reflection.Internal
{
	internal interface IDelegateFactory
	{
		Func<T?, object?> CreateGet<T>(MemberInfo memberInfo);

		Func<T?, object?> CreateGet<T>(PropertyInfo propertyInfo);

		Func<T?, object?> CreateGet<T>(FieldInfo fieldInfo);

		Action<T?, object?> CreateSet<T>(MemberInfo memberInfo);

		Action<T?, object?> CreateSet<T>(PropertyInfo propertyInfo);

		Action<T?, object?> CreateSet<T>(FieldInfo fieldInfo);

		Func<T> CreateDefaultConstructor<T>(Type type);

		ObjectConstructor<object> CreateParameterizedConstructor(MethodBase method);

		MethodCall<T?, object?> CreateMethodCall<T>(MethodBase method);
	}
}

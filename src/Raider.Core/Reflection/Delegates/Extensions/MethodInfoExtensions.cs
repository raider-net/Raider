using System;
using System.Reflection;
using Raider.Reflection.Delegates.Helper;

namespace Raider.Reflection.Delegates.Extensions
{
	internal static class MethodInfoExtensions
	{
		public static TDelegate? CreateDelegate<TDelegate>(this MethodInfo method)
			where TDelegate : class
		{
			DelegateHelper.CheckDelegateReturnType<TDelegate>(method);
			return method.CreateDelegate(typeof(TDelegate)) as TDelegate;
		}

		public static void IsEventArgsTypeCorrect(this MethodInfo method, Type eventArgsType)
		{
			var argsType = method.GetParameters()[0].ParameterType.GetMethod("Invoke")?
				.GetParameters()[1].ParameterType;
			DelegateHelper.IsEventArgsTypeCorrect(argsType!, eventArgsType, false);
		}
	}
}

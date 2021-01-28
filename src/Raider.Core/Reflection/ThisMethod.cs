using Raider.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Raider.Reflection
{
	public static class ThisMethod
	{
		public static string? GetMethodFullName(bool ignoreAsync = true, bool includeAssemblyFullName = true, bool includeReflectedType = true, bool ReflectedTypeFullName = true, bool includeParameters = true, bool parameterTypeFullName = false)
		{
			return GetMethodBase(ignoreAsync)?.GetMethodFullName(includeAssemblyFullName, includeReflectedType, ReflectedTypeFullName, includeParameters, parameterTypeFullName);
		}

		public static string? GetPreviousMethodFullName(bool ignoreAsync = true, bool includeAssemblyFullName = true, bool includeReflectedType = true, bool ReflectedTypeFullName = true, bool includeParameters = true, bool parameterTypeFullName = false)
		{
			return GetPreviousMethodBase(ignoreAsync)?.GetMethodFullName(includeAssemblyFullName, includeReflectedType, ReflectedTypeFullName, includeParameters, parameterTypeFullName);
		}

		public static MethodBase GetMethodBase(bool ignoreAsync = true)
		{
			return GetMethodBase(0, ignoreAsync, null);
		}

		public static MethodBase GetPreviousMethodBase(bool ignoreAsync = true)
		{
			return GetMethodBase(-1, ignoreAsync, null);
		}

		internal static string? GetMethodFullName(List<Type> notAssignableFrom, bool ignoreAsync = true, bool includeAssemblyFullName = true, bool includeReflectedType = true, bool ReflectedTypeFullName = true, bool includeParameters = true, bool parameterTypeFullName = false)
		{
			return GetMethodBase(notAssignableFrom, ignoreAsync)?.GetMethodFullName(includeAssemblyFullName, includeReflectedType, ReflectedTypeFullName, includeParameters, parameterTypeFullName);
		}

		internal static MethodBase GetMethodBase(List<Type> notAssignableFrom, bool ignoreAsync = true)
		{
			return GetMethodBase(0, ignoreAsync, notAssignableFrom);
		}

		public static List<string> GetMethodsCallStack(bool ignoreAsync = true, bool includeAssemblyFullName = true, bool includeReflectedType = true, bool ReflectedTypeFullName = true, bool includeParameters = true, bool parameterTypeFullName = false)
		{
			return
				GetMethodBaseCallStack()
					.Select(m => m.GetMethodFullName(includeAssemblyFullName, includeReflectedType, ReflectedTypeFullName, includeParameters, parameterTypeFullName))
					.ToList();
		}

		internal static List<MethodBase> GetMethodBaseCallStack()
		{
			List<MethodBase> result = new List<MethodBase>();
			System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(true);
			for (int i = 0; i < stackTrace.FrameCount; ++i)
			{
				System.Diagnostics.StackFrame frame = stackTrace.GetFrame(i);
				MethodBase callerMethod = frame.GetMethod();
				if (callerMethod.DeclaringType == null || !TypeHelper.IsDerivedFrom(callerMethod.DeclaringType, typeof(ThisMethod)))
				{
					result.Add(callerMethod);
				}
			}
			return result;
		}

        private static MethodBase GetMethodBase(int previousFrameIndex, bool ignoreAsync, List<Type>? notAssignableFrom)
        {
            System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(true);
            for (int i = 0; i < stackTrace.FrameCount; ++i)
            {
                System.Diagnostics.StackFrame frame = stackTrace.GetFrame(i);
                MethodBase callerMethod = frame.GetMethod();
                if (callerMethod.DeclaringType == null ||
                    (!TypeHelper.IsDerivedFrom(callerMethod.DeclaringType, typeof(ThisMethod))
                    && ((notAssignableFrom == null || notAssignableFrom.Count == 0)
                            || (notAssignableFrom.All(x => !x.IsAssignableFrom(callerMethod.DeclaringType))
                                    && notAssignableFrom.All(x => !TypeHelper.IsDeclaredIn(callerMethod.DeclaringType, x))))))
                {
                    if (stackTrace.FrameCount - 1 < i - previousFrameIndex)
                    {
                        return null;
                    }
                    if (ignoreAsync && callerMethod.IsAsync())
                        continue;

                    return GetMethodBase(stackTrace, i, -previousFrameIndex, ignoreAsync);
                }
            }
            return null;
        }

        private static MethodBase GetMethodBase(System.Diagnostics.StackTrace stackTrace, int fromIndex, int framesToSkip, bool ignoreAsync)
        {
            int skippedFrames = 0;
            for (int i = fromIndex; i < stackTrace.FrameCount; i++)
            {
                System.Diagnostics.StackFrame frame = stackTrace.GetFrame(i);
                MethodBase method = frame.GetMethod();

                if (ignoreAsync && method.IsAsync())
                    continue;

                if (skippedFrames < framesToSkip)
                {
                    skippedFrames++;
                    continue;
                }

                return method;
            }

            return null;
        }

		//private static MethodBase GetMethodBase(int previousFrameIndex, bool ignoreAsync, List<Type> notAssignableFrom)
		//{
		//	var framesToSkip = -previousFrameIndex;
		//	bool inAsyncStateMachine = false;
		//	System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(true);

		//	var degub = string.Join(Environment.NewLine, stackTrace.GetFrames().Select(f => f.GetMethod()).Select(m => $"{((m.DeclaringType != null && (m.DeclaringType.Namespace == "System.Runtime.CompilerServices" || m.DeclaringType.Namespace.StartsWith("System.Threading"))) ? "SKIP" : "OK  ")} {m.GetMethodFullName()}"));

		//	if (stackTrace.FrameCount - 1 < framesToSkip)
		//		return null;

		//	int skippedFrames = 0;
		//	bool isInLocalClassStack = true;
		//	for (int i = 0; i < stackTrace.FrameCount; ++i)
		//	{
		//		System.Diagnostics.StackFrame frame = stackTrace.GetFrame(i);
		//		MethodBase method = frame.GetMethod();

		//		if (isInLocalClassStack)
		//		{
		//			if (method.DeclaringType == null ||
		//				(!TypeHelper.IsDerivedFrom(method.DeclaringType, typeof(ThisMethod))
		//				&& (notAssignableFrom == null
		//					|| notAssignableFrom.Count == 0
		//					|| (notAssignableFrom.All(x => !x.IsAssignableFrom(method.DeclaringType))
		//						&& notAssignableFrom.All(x => !TypeHelper.IsDeclaredIn(method.DeclaringType, x))))))
		//			{
		//				isInLocalClassStack = false;
		//			}
		//			else
		//			{
		//				continue;
		//			}
		//		}

		//		if (ignoreAsync)
		//		{
		//			var isAsyncStateMachine = method.IsAsyncStateMachine();
		//			inAsyncStateMachine = inAsyncStateMachine || isAsyncStateMachine;

		//			if (inAsyncStateMachine)
		//			{
		//				if (method.DeclaringType != null
		//					&& (method.DeclaringType.Namespace == "System.Runtime.CompilerServices"
		//						|| method.DeclaringType.Namespace.StartsWith("System.Threading")))
		//					continue;

		//				if (isAsyncStateMachine)
		//				{
		//					if (skippedFrames < framesToSkip)
		//					{
		//						skippedFrames++;
		//						continue;
		//					}

		//					return method;
		//				}
		//				else
		//				{
		//					continue;
		//				}
		//			}
		//			else
		//			{
		//				if (skippedFrames < framesToSkip)
		//				{
		//					skippedFrames++;
		//					continue;
		//				}

		//				return method;
		//			}
		//		}
		//		else
		//		{
		//			if (skippedFrames < framesToSkip)
		//			{
		//				skippedFrames++;
		//				continue;
		//			}

		//			return method;
		//		}
		//	}
		//	return null;
		//}
	}
}

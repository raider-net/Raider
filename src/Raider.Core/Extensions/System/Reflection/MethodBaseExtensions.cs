using System;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Raider.Extensions
{
	public static class MethodBaseExtensions
	{
		public static string? GetMethodFullName(this MethodBase method, bool includeAssemblyFullName = true, bool includeReflectedType = true, bool ReflectedTypeFullName = true, bool includeParameters = true, bool parameterTypeFullName = false)
		{
			if (method != null)
			{
				Type? dt = method.ReflectedType ?? method.DeclaringType;
				StringBuilder sb = new StringBuilder();
				if (!includeReflectedType || dt == null)
				{
					sb.AppendFormat("{0}(", method.Name);
				}
				else if (ReflectedTypeFullName)
				{
					sb.AppendFormat("{0}.{1}(", dt.ToFriendlyFullName(), method.Name);
				}
				else
				{
					sb.AppendFormat("{0}.{1}(", dt.ToFriendlyName(), method.Name);
				}

				if (includeParameters)
				{
					ParameterInfo[] parameters = method.GetParameters();
					if (parameters != null)
					{
						int parametersCount = 0;
						foreach (ParameterInfo pi in parameters)
						{
							parametersCount++;
							if (1 < parametersCount) sb.Append(", ");
							if (parameterTypeFullName)
							{
								sb.Append(pi.ParameterType.ToFriendlyFullName());
							}
							else
							{
								sb.Append(pi.ParameterType.ToFriendlyName());
							}
							sb.AppendFormat(" {0}", pi.Name);
						}
					}
				}

				sb.Append(')');
				if (includeAssemblyFullName && dt != null && dt.Assembly != null)
				{
					sb.AppendFormat("; {0}", dt.Assembly.FullName);
				}
				return sb.ToString();
			}
			else return null;
		}

		public static T? GetFirstAttribute<T>(this MethodBase method, bool inherit = true) where T : System.Attribute
		{
			if (method == null) return default;
			var result = method.GetCustomAttributes(typeof(T), inherit);
			return result != null ? result.FirstOrDefault() as T : null;
		}

		public static T[]? GetAttributeList<T>(this MethodBase method, bool inherit = true) where T : System.Attribute
		{
			if (method == null) return default;
			var result = method.GetCustomAttributes(typeof(T), inherit);
			return result != null ? result as T[] : null;
		}

		public static bool IsAsync(this MethodBase method)
		{
			if (method == null)
				throw new ArgumentNullException(nameof(method));

			return method.DeclaringType != null
							&& (typeof(System.Runtime.CompilerServices.IAsyncStateMachine).IsAssignableFrom(method.DeclaringType)
								|| method.DeclaringType.Namespace == "System.Runtime.CompilerServices");
		}

		public static bool IsAsyncStateMachine(this MethodBase method)
		{
			if (method == null)
				throw new ArgumentNullException(nameof(method));

			return method.DeclaringType != null
					&& typeof(System.Runtime.CompilerServices.IAsyncStateMachine).IsAssignableFrom(method.DeclaringType);
		}
	}
}

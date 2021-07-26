using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Raider.Services.Aspects
{
	public abstract class SingleAspect
	{
		public void Intercept(
			Action<AspectContext> action,
			Action<InterceptorOptions>? configure = null,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
		{

		}
	}
}

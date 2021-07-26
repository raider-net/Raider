using Raider.Commands;
using System;
using System.Threading.Tasks;

namespace Raider.Services
{
	public class AsyncInterceptorOptions
	{
		public Func<AspectContext, Task<ICommandResult<OnBeforeAspectContinuation>>>? OnBeforeInvokeAsync { get; set; }
		public Func<AspectContext, object?, Task<ICommandResult<OnAfterAspectContinuation>>>? OnAfterInvokeAsync { get; set; }

		public ExceptionHandlingEnum DefaultExceptionHandling { get; set; } = ExceptionHandlingEnum.Handle;
		public Func<AspectContext, Exception, ICommandResult<ExceptionHandlingEnum>>? OnError { get; set; }

		internal AsyncInterceptorOptions() { }
	}
}

using Raider.Commands;
using System;

namespace Raider.Services
{
	public class InterceptorOptions
	{
		public Func<AspectContext, ICommandResult<OnBeforeAspectContinuation>>? OnBeforeInvoke { get; set; }
		public Func<AspectContext, object?, ICommandResult<OnAfterAspectContinuation>>? OnAfterInvoke { get; set; }

		public ExceptionHandlingEnum DefaultExceptionHandling { get; set; } = ExceptionHandlingEnum.Handle;
		public Func<AspectContext, Exception, ICommandResult<ExceptionHandlingEnum>>? OnError { get; set; }

		internal InterceptorOptions() { }
	}
}

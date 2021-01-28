using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Raider.Trace
{
	public class TraceFrame
	{
		public static ITraceFrame Create(
			IEnumerable<MethodParameter>? methodParameters = null,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
			=> new TraceFrameBuilder()
				.CallerMemberName(memberName)
				.CallerFilePath(sourceFilePath)
				.CallerLineNumber(sourceLineNumber == 0 ? (int?)null : sourceLineNumber)
				.MethodParameters(methodParameters)
				.Build();

		public static ITraceFrame Create(
			ITraceFrame? previousTraceFrame,
			IEnumerable<MethodParameter>? methodParameters = null,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
			=> new TraceFrameBuilder(previousTraceFrame)
				.CallerMemberName(memberName)
				.CallerFilePath(sourceFilePath)
				.CallerLineNumber(sourceLineNumber == 0 ? (int?)null : sourceLineNumber)
				.MethodParameters(methodParameters)
				.Build();

		public static string GetThisCallerMethodFullName(
			IEnumerable<MethodParameter>? methodParameters = null,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
			=> new TraceFrameBuilder()
				.CallerMemberName(memberName)
				.CallerFilePath(sourceFilePath)
				.CallerLineNumber(sourceLineNumber == 0 ? (int?)null : sourceLineNumber)
				.MethodParameters(methodParameters)
				.Build()
				.ToCallerMethodFullName();

		public static string GetThisCallerMethodFullName(
			ITraceFrame? previousTraceFrame,
			IEnumerable<MethodParameter>? methodParameters = null,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
			=> new TraceFrameBuilder(previousTraceFrame)
				.CallerMemberName(memberName)
				.CallerFilePath(sourceFilePath)
				.CallerLineNumber(sourceLineNumber == 0 ? (int?)null : sourceLineNumber)
				.MethodParameters(methodParameters)
				.Build()
				.ToCallerMethodFullName();

		public static string GetCallerPath(
			IEnumerable<MethodParameter>? methodParameters = null,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
			=> new TraceFrameBuilder()
				.CallerMemberName(memberName)
				.CallerFilePath(sourceFilePath)
				.CallerLineNumber(sourceLineNumber == 0 ? (int?)null : sourceLineNumber)
				.MethodParameters(methodParameters)
				.Build()
				.ToString() ?? "";

		public static string GetCallerPath(
			ITraceFrame? previousTraceFrame,
			IEnumerable<MethodParameter>? methodParameters = null,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
			=> new TraceFrameBuilder(previousTraceFrame)
				.CallerMemberName(memberName)
				.CallerFilePath(sourceFilePath)
				.CallerLineNumber(sourceLineNumber == 0 ? (int?)null : sourceLineNumber)
				.MethodParameters(methodParameters)
				.Build()
				.ToString() ?? "";
	}
}

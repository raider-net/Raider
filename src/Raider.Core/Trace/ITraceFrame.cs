using System;
using System.Collections.Generic;

namespace Raider.Trace
{
	public interface ITraceFrame
	{
		Guid MethodCallId { get; }
		string? CallerMemberName { get; }
		string? CallerFilePath { get; }
		int? CallerLineNumber { get; }
		IEnumerable<MethodParameter>? MethodParameters { get; }
		ITraceFrame? Previous { get; }

		string ToCallerMethodFullName();
		IReadOnlyList<ITraceFrame> GetTrace();
	}
}

using Raider.Infrastructure;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Raider.Trace
{
	public class TraceInfo : ITraceInfo
	{
		public Guid RuntimeUniqueKey { get; internal set; }

		public ITraceFrame TraceFrame { get; }

		public int? IdUser { get; internal set; }

		public string? ExternalCorrelationId { get; internal set; }

		public Guid? CorrelationId { get; internal set; }

		internal TraceInfo(ITraceFrame traceFrame)
		{
			RuntimeUniqueKey = EnvironmentInfo.RUNTIME_UNIQUE_KEY;
			TraceFrame = traceFrame ?? throw new ArgumentNullException(nameof(traceFrame));
		}

		public override string ToString()
			=> $"{TraceFrame}: {nameof(RuntimeUniqueKey)} = {RuntimeUniqueKey} | {nameof(CorrelationId)} = {CorrelationId} | {nameof(IdUser)} = {IdUser}";

		public static ITraceInfo Create(
			int? iduser = null,
			Guid? correlationId = null,
			IEnumerable<MethodParameter>? methodParameters = null,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
			=> new TraceInfoBuilder(
					new TraceFrameBuilder()
						.CallerMemberName(memberName)
						.CallerFilePath(sourceFilePath)
						.CallerLineNumber(sourceLineNumber == 0 ? (int?)null : sourceLineNumber)
						.MethodParameters(methodParameters)
						.Build(),
					null)
					.IdUser(iduser)
					.CorrelationId(correlationId)
				.Build();

		public static ITraceInfo Create(
			ITraceFrame? previousTraceFrame,
			int? iduser = null,
			Guid? correlationId = null,
			IEnumerable<MethodParameter>? methodParameters = null,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
			=> new TraceInfoBuilder(
					new TraceFrameBuilder(previousTraceFrame)
						.CallerMemberName(memberName)
						.CallerFilePath(sourceFilePath)
						.CallerLineNumber(sourceLineNumber == 0 ? (int?)null : sourceLineNumber)
						.MethodParameters(methodParameters)
						.Build(),
					null)
					.IdUser(iduser)
					.CorrelationId(correlationId)
				.Build();

		public static ITraceInfo Create(
			ITraceInfo? previousTraceInfo,
			IEnumerable<MethodParameter>? methodParameters = null,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
			=> new TraceInfoBuilder(
					new TraceFrameBuilder()
						.CallerMemberName(memberName)
						.CallerFilePath(sourceFilePath)
						.CallerLineNumber(sourceLineNumber == 0 ? (int?)null : sourceLineNumber)
						.MethodParameters(methodParameters)
						.Build(),
					previousTraceInfo)
				.Build();
	}
}

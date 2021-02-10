using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace Raider.Trace
{
	public class TraceFrame : ITraceFrame
	{
		public Guid MethodCallId { get; set; } = Guid.NewGuid();
		public string? CallerMemberName { get; set; }
		public string? CallerFilePath { get; set; }
		public int? CallerLineNumber { get; set; }
		public IEnumerable<MethodParameter>? MethodParameters { get; set; }
		public ITraceFrame? Previous { get; set; }

		internal TraceFrame()
		{
		}

		public string ToCallerMethodFullName()
		{
			var empty = true;
			var sb = new StringBuilder();

			if (!string.IsNullOrWhiteSpace(CallerMemberName))
			{
				sb.Append(CallerMemberName);
				empty = false;
			}

			if (!string.IsNullOrWhiteSpace(CallerFilePath))
			{
				var callerFileName = CallerFilePath.Trim().EndsWith(".cs", StringComparison.InvariantCultureIgnoreCase)
					? Path.GetFileName(CallerFilePath)
					: CallerFilePath;

				sb.Append(empty
					? callerFileName
					: $" in {callerFileName}");

				empty = false;
			}

			if (CallerLineNumber.HasValue)
			{
				sb.Append(empty
					? $"line {CallerLineNumber}"
					: $":line {CallerLineNumber}");
			}

			if (MethodParameters != null)
			{
				foreach (var param in MethodParameters)
				{
					if (string.IsNullOrWhiteSpace(param.ParameterName))
					{
						sb.AppendLine();
						sb.Append($"\t-param[{param.ParameterName}]: {param.SerializedValue}");
					}
				}
			}

			return sb.ToString();
		}

		public IReadOnlyList<ITraceFrame> GetTrace()
		{
			var result = new List<ITraceFrame> { this };

			if (Previous == null)
				return result;

			var previous = Previous;
			while (previous != null)
			{
				result.Add(previous);
				previous = previous.Previous;
			}

			return result;
		}

		public override string ToString()
			=> Previous == null
				? ToCallerMethodFullName()
				: $"{ToCallerMethodFullName()}{Environment.NewLine}{Previous}";

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

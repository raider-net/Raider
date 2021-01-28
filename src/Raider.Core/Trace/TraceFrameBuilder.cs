using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Raider.Trace
{
	public interface ITraceFrameBuilder<TBuilder>
		where TBuilder : ITraceFrameBuilder<TBuilder>
	{
		ITraceFrame Build();

		internal TBuilder MethodCallId(Guid methodCallId);

		TBuilder CallerMemberName(string? callerMemberName, bool force = false);

		TBuilder CallerFilePath(string? callerFilePath, bool force = false);

		TBuilder CallerLineNumber(int? callerLineNumber, bool force = false);

		TBuilder MethodParameters(IEnumerable<MethodParameter>? methodParameters, bool force = false);
	}

	public abstract class TraceFrameBuilderBase<TBuilder> : ITraceFrameBuilder<TBuilder>
		where TBuilder : TraceFrameBuilderBase<TBuilder>
	{
		public class TraceFrame : ITraceFrame
		{
			public Guid MethodCallId { get; set; } = Guid.NewGuid();
			public string? CallerMemberName { get; set; }
			public string? CallerFilePath { get; set; }
			public int? CallerLineNumber { get; set; }
			public IEnumerable<MethodParameter>? MethodParameters { get; set; }
			public ITraceFrame? Previous { get; set; }

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
		}

		private readonly TBuilder _builder;
		private readonly TraceFrame _traceFrame;

		protected TraceFrameBuilderBase(ITraceFrame? previousTraceFrame)
		{
			_traceFrame = new TraceFrame
			{
				Previous = previousTraceFrame
			};

			_builder = (TBuilder)this;
		}

		public ITraceFrame Build()
			=> _traceFrame;

		TBuilder ITraceFrameBuilder<TBuilder>.MethodCallId(Guid methodCallId)
		{
			_traceFrame.MethodCallId = methodCallId;
			return _builder;
		}

		public TBuilder CallerMemberName(string? callerMemberName, bool force = false)
		{
			if (force || string.IsNullOrWhiteSpace(_traceFrame.CallerMemberName))
				_traceFrame.CallerMemberName = callerMemberName;

			return _builder;
		}

		public TBuilder CallerFilePath(string? callerFilePath, bool force = false)
		{
			if (force || string.IsNullOrWhiteSpace(_traceFrame.CallerFilePath))
				_traceFrame.CallerFilePath = callerFilePath;
			return _builder;
		}

		public TBuilder CallerLineNumber(int? callerLineNumber, bool force = false)
		{
			if (force || !_traceFrame.CallerLineNumber.HasValue)
				_traceFrame.CallerLineNumber = callerLineNumber;
			return _builder;
		}

		public TBuilder MethodParameters(IEnumerable<MethodParameter>? methodParameters, bool force = false)
		{
			if (force || _traceFrame.MethodParameters == null)
				_traceFrame.MethodParameters = methodParameters;
			return _builder;
		}
	}

	public class TraceFrameBuilder : TraceFrameBuilderBase<TraceFrameBuilder>
	{
		public TraceFrameBuilder()
			: this(null)
		{
		}

		public TraceFrameBuilder(ITraceFrame? previousTraceFrame)
			: base(previousTraceFrame)
		{
		}

		//public static implicit operator TraceFrame?(TraceFrameBuilder builder)
		//{
		//	if (builder == null)
		//		return null;

		//	return builder._traceFrame;
		//}

		//public static implicit operator TraceFrameBuilder?(TraceFrame traceFrame)
		//{
		//	if (traceFrame == null)
		//		return null;

		//	return new TraceFrameBuilder(traceFrame);
		//}
	}
}

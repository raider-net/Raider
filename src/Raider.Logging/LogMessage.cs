using Microsoft.Extensions.Logging;
using Raider.Trace;
using System;
using System.Collections.Generic;
using System.Text;

namespace Raider.Logging
{
	public class LogMessage : ILogMessage
	{
		public Guid IdLogMessage { get; set; }
		public LogLevel LogLevel { get; set; }
		public int IdLogLevel => (int)LogLevel;
		public DateTimeOffset Created { get; set; }
		public ITraceInfo TraceInfo { get; set; }
		public string? LogCode { get; set; }
		public string? ClientMessage { get; set; }
		public string? InternalMessage { get; set; }
		public string? StackTrace { get; set; }
		public string? Detail { get; set; }
		public string ClientMessageWithId => ToString(true, false, false);
		public string ClientMessageWithIdAndPropName => ToString(true, true, false);
		public string FullMessage => ToString(true, true, true);
		public bool IsLogged { get; set; }
		public string? CommandQueryName { get; set; }
		public Guid? IdCommandQuery { get; set; }
		public decimal? MethodCallElapsedMilliseconds { get; set; }
		public string? PropertyName { get; set; }
		public object? ValidationFailure { get; set; }
		public string? DisplayPropertyName { get; set; }
		public bool? IsValidationError { get; set; }

		internal LogMessage(ITraceInfo traceInfo)
		{
			IdLogMessage = Guid.NewGuid();
			Created = DateTimeOffset.Now;
			TraceInfo = traceInfo ?? throw new ArgumentNullException(nameof(traceInfo));
		}

		public IDictionary<string, object?> ToDictionary()
		{
			var dict = new Dictionary<string, object?>
			{
				{ nameof(IdLogMessage), IdLogMessage },
				//{ nameof(LogLevel), LogLevel },
				{ nameof(IdLogLevel), IdLogLevel },
				{ nameof(Created), Created },
				{ nameof(TraceInfo.RuntimeUniqueKey), TraceInfo.RuntimeUniqueKey }
			};

			if (!string.IsNullOrWhiteSpace(LogCode))
				dict.Add(nameof(LogCode), LogCode);

			if (!string.IsNullOrWhiteSpace(ClientMessage))
				dict.Add(nameof(ClientMessage), ClientMessage);

			if (!string.IsNullOrWhiteSpace(InternalMessage))
				dict.Add(nameof(InternalMessage), InternalMessage);

			if (TraceInfo.TraceFrame != null)
			{
				dict.Add(nameof(TraceInfo.TraceFrame.MethodCallId), TraceInfo.TraceFrame.MethodCallId);
				dict.Add(nameof(TraceInfo.TraceFrame), $"{TraceInfo.TraceFrame}");
			}

			if (!string.IsNullOrWhiteSpace(StackTrace))
				dict.Add(nameof(StackTrace), StackTrace);

			if (!string.IsNullOrWhiteSpace(Detail))
				dict.Add(nameof(Detail), Detail);

			if (TraceInfo.IdUser.HasValue)
				dict.Add(nameof(TraceInfo.IdUser), TraceInfo.IdUser);

			if (!string.IsNullOrWhiteSpace(CommandQueryName))
				dict.Add(nameof(CommandQueryName), CommandQueryName);

			if (IdCommandQuery.HasValue)
				dict.Add(nameof(IdCommandQuery), IdCommandQuery);

			if (MethodCallElapsedMilliseconds.HasValue)
				dict.Add(nameof(MethodCallElapsedMilliseconds), MethodCallElapsedMilliseconds);

			if (!string.IsNullOrWhiteSpace(PropertyName))
				dict.Add(nameof(PropertyName), PropertyName);

			if (!string.IsNullOrWhiteSpace(DisplayPropertyName))
				dict.Add(nameof(DisplayPropertyName), DisplayPropertyName);

			if (ValidationFailure != null)
				dict.Add(nameof(ValidationFailure), ValidationFailure.ToString());

			if (IsValidationError.HasValue)
				dict.Add(nameof(IsValidationError), IsValidationError);

			if (TraceInfo.CorrelationId.HasValue)
				dict.Add(nameof(TraceInfo.CorrelationId), TraceInfo.CorrelationId.Value);

			return dict;
		}

		public override string ToString()
		{
			return FullMessage;
		}

		public string ToString(bool withId, bool withPropertyName, bool withDetail)
		{
			var sb = new StringBuilder();

			bool empty = string.IsNullOrWhiteSpace(ClientMessage);
			if (!empty)
				sb.Append(ClientMessage);

			if (withPropertyName)
			{
				if (!string.IsNullOrWhiteSpace(DisplayPropertyName))
				{
					if (empty)
						sb.Append(DisplayPropertyName);
					else
						sb.Append($" - {DisplayPropertyName}");

					empty = false;
				}
			}

			if (withId)
			{
				if (empty)
					sb.Append($"ID: {IdLogMessage}");
				else
					sb.Append($" (ID: {IdLogMessage})");

				empty = false;
			}

			if (withDetail && !string.IsNullOrWhiteSpace(InternalMessage))
			{
				if (empty)
					sb.Append(InternalMessage);
				else
					sb.Append($" | {InternalMessage}");
			}

			if (withDetail && !string.IsNullOrWhiteSpace(StackTrace))
			{
				if (empty)
					sb.Append(StackTrace);
				else
					sb.Append($" | {StackTrace}");

				empty = false;
			}

			if (withDetail && !string.IsNullOrWhiteSpace(Detail))
			{
				if (empty)
					sb.Append(Detail);
				else
					sb.Append($" | {Detail}");
			}

			return sb.ToString();
		}
	}
}

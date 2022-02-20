using Microsoft.Extensions.Logging;
using Raider.Identity;
using Raider.Trace;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
#if NETSTANDARD2_0 || NETSTANDARD2_1
using Newtonsoft.Json;
#elif NET5_0
using System.Text.Json;
#endif

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
		public Dictionary<string, string>? CustomData { get; set; }
		public List<string>? Tags { get; set; }

		internal LogMessage(ITraceInfo traceInfo)
		{
			IdLogMessage = Guid.NewGuid();
			Created = DateTimeOffset.Now;
			TraceInfo = traceInfo ?? throw new ArgumentNullException(nameof(traceInfo));
		}

		public static ILogMessage CreateLogMessage(
			LogLevel logLevel,
			Action<LogMessageBuilder> messageBuilder,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
		{
			if (messageBuilder == null)
				throw new ArgumentNullException(nameof(messageBuilder));

			var builder = new LogMessageBuilder(Raider.Trace.TraceInfo.Create((RaiderPrincipal<int>?)null, null, null, memberName, sourceFilePath, sourceLineNumber))
				.LogLevel(logLevel);

			messageBuilder.Invoke(builder);
			var message = builder.Build();

			return message;
		}

		public static IErrorMessage CreateErrorMessage(
			LogLevel logLevel,
			Action<ErrorMessageBuilder> messageBuilder,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
		{
			if (messageBuilder == null)
				throw new ArgumentNullException(nameof(messageBuilder));

			var builder = new ErrorMessageBuilder(Raider.Trace.TraceInfo.Create((RaiderPrincipal<int>?)null, null, null, memberName, sourceFilePath, sourceLineNumber))
				.LogLevel(logLevel);

			messageBuilder.Invoke(builder);
			var message = builder.Build();

			return message;
		}

		public static ILogMessage CreateTraceMessage(
			Action<LogMessageBuilder> messageBuilder,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
			=> CreateLogMessage(LogLevel.Trace, messageBuilder, memberName, sourceFilePath, sourceLineNumber);

		public static ILogMessage CreateDebugMessage(
			Action<LogMessageBuilder> messageBuilder,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
			=> CreateLogMessage(LogLevel.Debug, messageBuilder, memberName, sourceFilePath, sourceLineNumber);

		public static ILogMessage CreateInformationMessage(
			Action<LogMessageBuilder> messageBuilder,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
			=> CreateLogMessage(LogLevel.Information, messageBuilder, memberName, sourceFilePath, sourceLineNumber);

		public static ILogMessage CreateWarningMessage(
			Action<LogMessageBuilder> messageBuilder,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
			=> CreateLogMessage(LogLevel.Warning, messageBuilder, memberName, sourceFilePath, sourceLineNumber);

		public static IErrorMessage CreateErrorMessage(
			Action<ErrorMessageBuilder> messageBuilder,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
			=> CreateErrorMessage(LogLevel.Error, messageBuilder, memberName, sourceFilePath, sourceLineNumber);

		public static IErrorMessage CreateCriticalMessage(
			Action<ErrorMessageBuilder> messageBuilder,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
			=> CreateErrorMessage(LogLevel.Critical, messageBuilder, memberName, sourceFilePath, sourceLineNumber);

		public IDictionary<string, object?> ToDictionary(Serializer.ISerializer? serializer = null)
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

			if (CustomData != null && 0 < CustomData.Count)
#if NETSTANDARD2_0 || NETSTANDARD2_1
				dict.Add(nameof(CustomData), JsonConvert.SerializeObject(CustomData));
#elif NET5_0
				dict.Add(nameof(CustomData), JsonSerializer.Serialize(CustomData));
#endif

			if (Tags != null && 0 < Tags.Count)
#if NETSTANDARD2_0 || NETSTANDARD2_1
				dict.Add(nameof(Tags), JsonConvert.SerializeObject(Tags));
#elif NET5_0
				dict.Add(nameof(Tags), JsonSerializer.Serialize(Tags));
#endif

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

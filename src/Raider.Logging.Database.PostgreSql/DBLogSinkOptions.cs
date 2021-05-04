using NpgsqlTypes;
using Raider.Data;
using Raider.Database.PostgreSql;
using Raider.Extensions;
using Raider.Logging.SerilogEx;
using Serilog.Events;
using Serilog.Formatting.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace Raider.Logging.Database.PostgreSql
{
	public class DBLogSinkOptions : DbBatchWriterOptions, IBatchWriterOptions
	{
		private const string _commaDelimiter = ",";
		private readonly JsonValueFormatter _valueFormatter = new JsonValueFormatter(typeTagName: null);

		public DBLogSinkOptions()
		{
			PropertyNames = new List<string>
			{
				nameof(LogEvent.Level),
				nameof(LogEvent.Timestamp),
				nameof(LogEvent.MessageTemplate),
				nameof(LogEvent.Properties),
				nameof(LogEvent.Exception),
				nameof(ILogMessage.TraceInfo.TraceFrame.MethodCallId),
				Serilog.Core.Constants.SourceContextPropertyName,
				nameof(ILogMessage.TraceInfo.CorrelationId),
				nameof(ILogMessage.TraceInfo.RuntimeUniqueKey),
				LogEventHelper.IS_DB_LOG
			};

			PropertyTypeMapping = new Dictionary<string, NpgsqlDbType>
			{
				{ nameof(LogEvent.Level), NpgsqlDbType.Integer },
				{ nameof(LogEvent.Timestamp), NpgsqlDbType.TimestampTz },
				{ nameof(LogEvent.MessageTemplate), NpgsqlDbType.Varchar },
				{ nameof(LogEvent.Properties), NpgsqlDbType.Varchar },
				{ nameof(LogEvent.Exception), NpgsqlDbType.Varchar },
				{ nameof(ILogMessage.TraceInfo.TraceFrame.MethodCallId), NpgsqlDbType.Uuid },
				{ Serilog.Core.Constants.SourceContextPropertyName, NpgsqlDbType.Varchar },
				{ nameof(ILogMessage.TraceInfo.CorrelationId), NpgsqlDbType.Uuid },
				{ nameof(ILogMessage.TraceInfo.RuntimeUniqueKey), NpgsqlDbType.Uuid },
				{ LogEventHelper.IS_DB_LOG, NpgsqlDbType.Boolean },
			};

			PropertyColumnMapping = new Dictionary<string, string>
			{
				{ nameof(LogEvent.Level), nameof(ILogMessage.IdLogLevel) },
				{ nameof(LogEvent.Timestamp), nameof(ILogMessage.Created) },
				{ nameof(LogEvent.MessageTemplate), nameof(ILogMessage.InternalMessage) },
				{ nameof(LogEvent.Properties), nameof(ILogMessage.Detail) },
				{ nameof(LogEvent.Exception), nameof(ILogMessage.StackTrace) },
			};

			PropertyValueConverter = new Dictionary<string, Func<object?, object?>>
			{
#pragma warning disable CS8605 // Unboxing a possibly null value.
				{ nameof(LogEvent.Level), (level) => (int)level },
#pragma warning restore CS8605 // Unboxing a possibly null value.
				{ 
					nameof(LogEvent.Properties),
					(properties) =>
					{
						if (properties is not IReadOnlyDictionary<string, LogEventPropertyValue> serilogProperties)
							return null;

						var output = new StringWriter();

						output.Write("{");

						var precedingDelimiter = "";
						foreach (var property in serilogProperties)
						{
							output.Write(precedingDelimiter);
							precedingDelimiter = _commaDelimiter;
							JsonValueFormatter.WriteQuotedJsonString(property.Key, output);
							output.Write(':');
							_valueFormatter.Format(property.Value, output);
						}

						output.Write('}');
						var result = output.ToString();
						return result;
					}
				},
				{ nameof(LogEvent.Exception), (exception) => exception == null ? null : (exception as Exception)?.ToStringTrace() },
			};
		}
	}
}

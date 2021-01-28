using NpgsqlTypes;
using Raider.Database.PostgreSql;
using Raider.Extensions;
using Raider.Logging.SerilogEx.Sink;
using Serilog.Events;
using Serilog.Formatting.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace Raider.Logging.Database.PostgreSql.SerilogEx.Sink
{
	public class DBLogSinkOptions : RaiderBatchSinkOptions, IBatchedPeriodOptions
	{
		private const string _commaDelimiter = ",";
		private readonly JsonValueFormatter _valueFormatter = new JsonValueFormatter(typeTagName: null);

		public string? ConnectionString { get; set; }
		public string? SchemaName { get; set; }
		public string? TableName { get; set; }
		public List<string> PropertyNames { get; set; }
		public Dictionary<string, string>? PropertyColumnMapping { get; set; }
		public Dictionary<string, NpgsqlDbType>? PropertyTypeMapping { get; set; }
		public Dictionary<string, Func<object?, object?>>? PropertyValueConverter { get; set; }
		public bool UseQuotationMarksForTableName { get; set; } = true;
		public bool UseQuotationMarksForColumnNames { get; set; } = true;

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
				Serilog.Core.Constants.SourceContextPropertyName
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

		public DBLogSinkOptions Validate()
		{
			if (string.IsNullOrWhiteSpace(ConnectionString))
				throw new ArgumentNullException(nameof(ConnectionString));

			return this;
		}

		public BulkInsertOptions ToBulkInsertOptions()
			=> new BulkInsertOptions
			{
				SchemaName = SchemaName,
				TableName = TableName,
				PropertyNames = PropertyNames,
				PropertyColumnMapping = PropertyColumnMapping,
				PropertyTypeMapping = PropertyTypeMapping,
				PropertyValueConverter = PropertyValueConverter,
				UseQuotationMarksForTableName = UseQuotationMarksForTableName,
				UseQuotationMarksForColumnNames = UseQuotationMarksForColumnNames
			}
			.Validate(validateProperties: true);
	}
}

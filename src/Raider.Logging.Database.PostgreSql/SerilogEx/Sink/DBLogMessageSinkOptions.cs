using NpgsqlTypes;
using Raider.Database.PostgreSql;
using Raider.Logging.SerilogEx.Sink;
using System;
using System.Collections.Generic;

namespace Raider.Logging.Database.PostgreSql.SerilogEx.Sink
{
	public class DBLogMessageSinkOptions : RaiderBatchSinkOptions, IBatchedPeriodOptions
	{
		public string? ConnectionString { get; set; }
		public string? SchemaName { get; set; }
		public string? TableName { get; set; }
		public List<string> PropertyNames { get; set; }
		public Dictionary<string, string>? PropertyColumnMapping { get; set; }
		public Dictionary<string, NpgsqlDbType>? PropertyTypeMapping { get; set; }
		public Dictionary<string, Func<object?, object?>>? PropertyValueConverter { get; set; }
		public bool UseQuotationMarksForTableName { get; set; } = true;
		public bool UseQuotationMarksForColumnNames { get; set; } = true;

		public DBLogMessageSinkOptions()
		{
			PropertyNames = new List<string>
			{
				nameof(ILogMessage.IdLogLevel),
				nameof(ILogMessage.Created),
				nameof(ILogMessage.TraceInfo.RuntimeUniqueKey),
				nameof(ILogMessage.LogCode),
				nameof(ILogMessage.ClientMessage),
				nameof(ILogMessage.InternalMessage),
				nameof(ILogMessage.TraceInfo.TraceFrame.MethodCallId),
				Serilog.Core.Constants.SourceContextPropertyName,
				nameof(ILogMessage.TraceInfo.TraceFrame),
				nameof(ILogMessage.StackTrace),
				nameof(ILogMessage.Detail),
				nameof(ILogMessage.TraceInfo.IdUser),
				nameof(ILogMessage.CommandQueryName),
				nameof(ILogMessage.IdCommandQuery),
				nameof(ILogMessage.MethodCallElapsedMilliseconds),
				nameof(ILogMessage.PropertyName),
				nameof(ILogMessage.DisplayPropertyName),
				nameof(ILogMessage.IsValidationError),
				nameof(ILogMessage.TraceInfo.CorrelationId)
			};

			PropertyTypeMapping = new Dictionary<string, NpgsqlDbType>
			{
				{ nameof(ILogMessage.IdLogLevel), NpgsqlDbType.Integer },
				{ nameof(ILogMessage.Created), NpgsqlDbType.TimestampTz },
				{ nameof(ILogMessage.TraceInfo.RuntimeUniqueKey), NpgsqlDbType.Uuid },
				{ nameof(ILogMessage.LogCode), NpgsqlDbType.Bigint },
				{ nameof(ILogMessage.ClientMessage), NpgsqlDbType.Varchar },
				{ nameof(ILogMessage.InternalMessage), NpgsqlDbType.Varchar },
				{ nameof(ILogMessage.TraceInfo.TraceFrame.MethodCallId), NpgsqlDbType.Uuid },
				{ Serilog.Core.Constants.SourceContextPropertyName, NpgsqlDbType.Varchar },
				{ nameof(ILogMessage.TraceInfo.TraceFrame), NpgsqlDbType.Varchar },
				{ nameof(ILogMessage.StackTrace), NpgsqlDbType.Varchar },
				{ nameof(ILogMessage.Detail), NpgsqlDbType.Varchar },
				{ nameof(ILogMessage.TraceInfo.IdUser), NpgsqlDbType.Integer },
				{ nameof(ILogMessage.CommandQueryName), NpgsqlDbType.Varchar },
				{ nameof(ILogMessage.IdCommandQuery), NpgsqlDbType.Bigint },
				{ nameof(ILogMessage.MethodCallElapsedMilliseconds), NpgsqlDbType.Numeric },
				{ nameof(ILogMessage.PropertyName), NpgsqlDbType.Varchar },
				{ nameof(ILogMessage.DisplayPropertyName), NpgsqlDbType.Varchar },
				{ nameof(ILogMessage.IsValidationError), NpgsqlDbType.Boolean },
				{ nameof(ILogMessage.TraceInfo.CorrelationId), NpgsqlDbType.Uuid }
			};
		}

		public DBLogMessageSinkOptions Validate()
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

using NpgsqlTypes;
using Raider.AspNetCore.Logging.Dto;
using Raider.Database.PostgreSql;
using Raider.Logging.SerilogEx.Sink;
using System;
using System.Collections.Generic;

namespace Raider.AspNetCore.Logging.PostgreSql.Sink
{
	public class DBRequestSinkOptions : RaiderBatchSinkOptions, IBatchedPeriodOptions
	{
		public string? ConnectionString { get; set; }
		public string? SchemaName { get; set; }
		public string? TableName { get; set; } = nameof(Request);
		public List<string>? PropertyNames { get; set; }
		public Dictionary<string, string>? PropertyColumnMapping { get; set; }
		public Dictionary<string, NpgsqlDbType>? PropertyTypeMapping { get; set; }
		public Dictionary<string, Func<object?, object?>>? PropertyValueConverter { get; set; }
		public bool UseQuotationMarksForTableName { get; set; } = true;
		public bool UseQuotationMarksForColumnNames { get; set; } = true;

		public DBRequestSinkOptions()
		{
			PropertyNames = new List<string>
			{
				nameof(Request.RuntimeUniqueKey),
				nameof(Request.Created),
				nameof(Request.CorrelationId),
				nameof(Request.ExternalCorrelationId),
				nameof(Request.Protocol),
				nameof(Request.Scheme),
				nameof(Request.Host),
				nameof(Request.Method),
				nameof(Request.Path),
				nameof(Request.QueryString),
				nameof(Request.Headers),
				nameof(Request.Body),
				nameof(Request.BodyByteArray),
				nameof(Request.Form),
				nameof(Request.Files)
			};

			PropertyTypeMapping = new Dictionary<string, NpgsqlDbType>
			{
				{ nameof(Request.RuntimeUniqueKey), NpgsqlDbType.Uuid },
				{ nameof(Request.Created), NpgsqlDbType.TimestampTz },
				{ nameof(Request.CorrelationId), NpgsqlDbType.Uuid },
				{ nameof(Request.ExternalCorrelationId), NpgsqlDbType.Varchar },
				{ nameof(Request.Protocol), NpgsqlDbType.Varchar },
				{ nameof(Request.Scheme), NpgsqlDbType.Varchar },
				{ nameof(Request.Host), NpgsqlDbType.Varchar },
				{ nameof(Request.Method), NpgsqlDbType.Varchar },
				{ nameof(Request.Path), NpgsqlDbType.Varchar },
				{ nameof(Request.QueryString), NpgsqlDbType.Varchar },
				{ nameof(Request.Headers), NpgsqlDbType.Varchar },
				{ nameof(Request.Body), NpgsqlDbType.Varchar },
				{ nameof(Request.BodyByteArray), NpgsqlDbType.Bytea },
				{ nameof(Request.Form), NpgsqlDbType.Varchar },
				{ nameof(Request.Files), NpgsqlDbType.Varchar }
			};
		}

		public DBRequestSinkOptions Validate()
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

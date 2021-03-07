using NpgsqlTypes;
using Raider.AspNetCore.Logging.Dto;
using Raider.Database.PostgreSql;
using Raider.Logging.SerilogEx.Sink;
using System;
using System.Collections.Generic;

namespace Raider.AspNetCore.Logging.PostgreSql.Sink
{
	public class DBResponseSinkOptions : RaiderBatchSinkOptions, IBatchedPeriodOptions
	{
		public string? ConnectionString { get; set; }
		public string? SchemaName { get; set; }
		public string? TableName { get; set; } = nameof(Response);
		public List<string>? PropertyNames { get; set; }
		public Dictionary<string, string>? PropertyColumnMapping { get; set; }
		public Dictionary<string, NpgsqlDbType>? PropertyTypeMapping { get; set; }
		public Dictionary<string, Func<object?, object?>>? PropertyValueConverter { get; set; }
		public bool UseQuotationMarksForTableName { get; set; } = true;
		public bool UseQuotationMarksForColumnNames { get; set; } = true;

		public DBResponseSinkOptions()
		{
			PropertyNames = new List<string>
			{
				nameof(Response.RuntimeUniqueKey),
				nameof(Response.Created),
				nameof(Response.CorrelationId),
				nameof(Request.ExternalCorrelationId),
				nameof(Response.StatusCode),
				nameof(Response.Headers),
				nameof(Response.Body),
				nameof(Response.BodyByteArray),
				nameof(Response.Error),
				nameof(Response.ElapsedMilliseconds)
			};

			PropertyTypeMapping = new Dictionary<string, NpgsqlDbType>
			{
				{ nameof(Response.RuntimeUniqueKey), NpgsqlDbType.Uuid },
				{ nameof(Response.Created), NpgsqlDbType.TimestampTz },
				{ nameof(Response.CorrelationId), NpgsqlDbType.Uuid },
				{ nameof(Request.ExternalCorrelationId), NpgsqlDbType.Varchar },
				{ nameof(Response.StatusCode), NpgsqlDbType.Integer },
				{ nameof(Response.Headers), NpgsqlDbType.Varchar },
				{ nameof(Response.Body), NpgsqlDbType.Varchar },
				{ nameof(Response.BodyByteArray), NpgsqlDbType.Bytea },
				{ nameof(Response.Error), NpgsqlDbType.Varchar },
				{ nameof(Response.ElapsedMilliseconds), NpgsqlDbType.Numeric }
			};
		}

		public DBResponseSinkOptions Validate()
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

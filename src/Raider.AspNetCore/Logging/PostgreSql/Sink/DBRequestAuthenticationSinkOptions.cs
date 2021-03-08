using NpgsqlTypes;
using Raider.AspNetCore.Logging.Dto;
using Raider.Database.PostgreSql;
using Raider.Logging.SerilogEx.Sink;
using System;
using System.Collections.Generic;

namespace Raider.AspNetCore.Logging.PostgreSql.Sink
{
	public class DBRequestAuthenticationSinkOptions : RaiderBatchSinkOptions, IBatchedPeriodOptions
	{
		public string? ConnectionString { get; set; }
		public string? SchemaName { get; set; }
		public string? TableName { get; set; } = nameof(RequestAuthentication);
		public List<string>? PropertyNames { get; set; }
		public Dictionary<string, string>? PropertyColumnMapping { get; set; }
		public Dictionary<string, NpgsqlDbType>? PropertyTypeMapping { get; set; }
		public Dictionary<string, Func<object?, object?>>? PropertyValueConverter { get; set; }
		public bool UseQuotationMarksForTableName { get; set; } = true;
		public bool UseQuotationMarksForColumnNames { get; set; } = true;

		public DBRequestAuthenticationSinkOptions()
		{
			PropertyNames = new List<string>
			{
				nameof(RequestAuthentication.RuntimeUniqueKey),
				nameof(RequestAuthentication.Created),
				nameof(RequestAuthentication.CorrelationId),
				nameof(RequestAuthentication.ExternalCorrelationId),
				nameof(RequestAuthentication.IdUser),
				nameof(RequestAuthentication.Roles),
				nameof(RequestAuthentication.Permissions)
			};

			PropertyTypeMapping = new Dictionary<string, NpgsqlDbType>
			{
				{ nameof(RequestAuthentication.RuntimeUniqueKey), NpgsqlDbType.Uuid },
				{ nameof(RequestAuthentication.Created), NpgsqlDbType.TimestampTz },
				{ nameof(RequestAuthentication.CorrelationId), NpgsqlDbType.Uuid },
				{ nameof(RequestAuthentication.ExternalCorrelationId), NpgsqlDbType.Varchar },
				{ nameof(RequestAuthentication.IdUser), NpgsqlDbType.Integer },
				{ nameof(RequestAuthentication.Roles), NpgsqlDbType.Varchar },
				{ nameof(RequestAuthentication.Permissions), NpgsqlDbType.Varchar }
			};
		}

		public DBRequestAuthenticationSinkOptions Validate()
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

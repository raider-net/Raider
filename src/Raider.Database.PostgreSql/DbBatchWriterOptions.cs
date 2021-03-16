using NpgsqlTypes;
using Raider.Data;
using Raider.Infrastructure;
using System;
using System.Collections.Generic;

namespace Raider.Database.PostgreSql
{
	public class DbBatchWriterOptions : BatchWriterOptions, IDbBatchWriterOptions, IBatchWriterOptions
	{
		public string? ConnectionString { get; set; }
		public string? SchemaName { get; set; }
		public string? TableName { get; set; } = nameof(EnvironmentInfo);
		public List<string>? PropertyNames { get; set; }
		public Dictionary<string, string>? PropertyColumnMapping { get; set; }
		public Dictionary<string, NpgsqlDbType>? PropertyTypeMapping { get; set; }
		public Dictionary<string, Func<object?, object?>>? PropertyValueConverter { get; set; }
		public bool UseQuotationMarksForTableName { get; set; } = true;
		public bool UseQuotationMarksForColumnNames { get; set; } = true;

		public DbBatchWriterOptions()
		{
		}

		public virtual DbBatchWriterOptions Validate()
		{
			if (string.IsNullOrWhiteSpace(ConnectionString))
				throw new ArgumentNullException(nameof(ConnectionString));

			return this;
		}

		public DictionaryTableOptions ToDictionaryTableOptions(bool validateProperties = true, bool validatePropertyMapping = true)
			=> new DictionaryTableOptions
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
			.Validate(validateProperties, validatePropertyMapping);
	}
}

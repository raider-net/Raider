using NpgsqlTypes;
using Raider.Data;
using System;
using System.Collections.Generic;

namespace Raider.Database.PostgreSql
{
	public interface IDbBatchWriterOptions : IBatchWriterOptions
	{
		string? ConnectionString { get; set; }
		string? SchemaName { get; set; }
		string? TableName { get; set; }
		List<string>? PropertyNames { get; set; }
		Dictionary<string, string>? PropertyColumnMapping { get; set; }
		Dictionary<string, NpgsqlDbType>? PropertyTypeMapping { get; set; }
		Dictionary<string, Func<object?, object?>>? PropertyValueConverter { get; set; }
		bool UseQuotationMarksForTableName { get; set; }
		bool UseQuotationMarksForColumnNames { get; set; }

		DbBatchWriterOptions Validate();

		BulkInsertOptions ToBulkInsertOptions(bool validateProperties = true);
	}
}

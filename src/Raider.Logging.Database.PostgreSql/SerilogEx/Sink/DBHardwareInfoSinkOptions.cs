using NpgsqlTypes;
using Raider.Database.PostgreSql;
using Raider.Hardware;
using Raider.Logging.SerilogEx.Sink;
using System;
using System.Collections.Generic;

namespace Raider.Logging.Database.PostgreSql.SerilogEx.Sink
{
	public class DBHardwareInfoSinkOptions : RaiderBatchSinkOptions, IBatchedPeriodOptions
	{
		public string? ConnectionString { get; set; }
		public string? SchemaName { get; set; }
		public string? TableName { get; set; } = nameof(HardwareInfo);
		public List<string>? PropertyNames { get; set; }
		public Dictionary<string, string>? PropertyColumnMapping { get; set; }
		public Dictionary<string, NpgsqlDbType>? PropertyTypeMapping { get; set; }
		public Dictionary<string, Func<object?, object?>>? PropertyValueConverter { get; set; }
		public bool UseQuotationMarksForTableName { get; set; } = true;
		public bool UseQuotationMarksForColumnNames { get; set; } = true;

		public DBHardwareInfoSinkOptions()
		{
			PropertyNames = new List<string>
			{
				nameof(FlatHardwareInfo.RuntimeUniqueKey),
				nameof(FlatHardwareInfo.HWThumbprint),
				nameof(FlatHardwareInfo.TotalMemoryCapacityGB),
				nameof(FlatHardwareInfo.MemoryAvailableGB),
				nameof(FlatHardwareInfo.MemoryPercentUsed),
				nameof(FlatHardwareInfo.PercentProcessorIdleTime),
				nameof(FlatHardwareInfo.PercentProcessorTime),
				nameof(FlatHardwareInfo.OS),
				nameof(FlatHardwareInfo.HWJson)
			};

			PropertyTypeMapping = new Dictionary<string, NpgsqlDbType>
			{
				{ nameof(FlatHardwareInfo.RuntimeUniqueKey), NpgsqlDbType.Uuid },
				{ nameof(FlatHardwareInfo.HWThumbprint), NpgsqlDbType.Varchar },
				{ nameof(FlatHardwareInfo.TotalMemoryCapacityGB), NpgsqlDbType.Numeric },
				{ nameof(FlatHardwareInfo.MemoryAvailableGB), NpgsqlDbType.Numeric },
				{ nameof(FlatHardwareInfo.MemoryPercentUsed), NpgsqlDbType.Numeric },
				{ nameof(FlatHardwareInfo.PercentProcessorIdleTime), NpgsqlDbType.Numeric },
				{ nameof(FlatHardwareInfo.PercentProcessorTime), NpgsqlDbType.Numeric },
				{ nameof(FlatHardwareInfo.OS), NpgsqlDbType.Varchar },
				{ nameof(FlatHardwareInfo.HWJson), NpgsqlDbType.Text }
			};
		}

		public DBHardwareInfoSinkOptions Validate()
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

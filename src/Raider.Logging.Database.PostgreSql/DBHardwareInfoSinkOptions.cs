using NpgsqlTypes;
using Raider.Data;
using Raider.Database.PostgreSql;
using Raider.Hardware;
using System.Collections.Generic;

namespace Raider.Logging.Database.PostgreSql
{
	public class DBHardwareInfoSinkOptions : DbBatchWriterOptions, IBatchWriterOptions
	{
		public DBHardwareInfoSinkOptions()
		{
			TableName = nameof(HardwareInfo);

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
	}
}

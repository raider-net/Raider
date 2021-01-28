using NpgsqlTypes;
using Raider.Database.PostgreSql;
using Raider.Infrastructure;
using Raider.Logging.SerilogEx.Sink;
using System;
using System.Collections.Generic;

namespace Raider.Logging.Database.PostgreSql.SerilogEx.Sink
{
	public class DBEnvironmentInfoSinkOptions : RaiderBatchSinkOptions, IBatchedPeriodOptions
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

		public DBEnvironmentInfoSinkOptions()
		{
			PropertyNames = new List<string>
			{
				nameof(EnvironmentInfo.RuntimeUniqueKey),
				nameof(EnvironmentInfo.RunningEnvironment),
				nameof(EnvironmentInfo.FrameworkDescription),
				nameof(EnvironmentInfo.TargetFramework),
				nameof(EnvironmentInfo.CLRVersion),
				nameof(EnvironmentInfo.EntryAssemblyName),
				nameof(EnvironmentInfo.EntryAssemblyVersion),
				nameof(EnvironmentInfo.BaseDirectory),
				nameof(EnvironmentInfo.MachineName),
				nameof(EnvironmentInfo.CurrentAppDomainName),
				nameof(EnvironmentInfo.Is64BitOperatingSystem),
				nameof(EnvironmentInfo.Is64BitProcess),
				nameof(EnvironmentInfo.OperatingSystemArchitecture),
				nameof(EnvironmentInfo.OperatingSystemPlatform),
				nameof(EnvironmentInfo.OperatingSystemVersion),
				nameof(EnvironmentInfo.ProcessArchitecture),
				nameof(EnvironmentInfo.CommandLine)
			};

			PropertyTypeMapping = new Dictionary<string, NpgsqlDbType>
			{
				{ nameof(EnvironmentInfo.RuntimeUniqueKey), NpgsqlDbType.Uuid },
				{ nameof(EnvironmentInfo.RunningEnvironment), NpgsqlDbType.Varchar },
				{ nameof(EnvironmentInfo.FrameworkDescription), NpgsqlDbType.Varchar },
				{ nameof(EnvironmentInfo.TargetFramework), NpgsqlDbType.Varchar },
				{ nameof(EnvironmentInfo.CLRVersion), NpgsqlDbType.Varchar },
				{ nameof(EnvironmentInfo.EntryAssemblyName), NpgsqlDbType.Varchar },
				{ nameof(EnvironmentInfo.EntryAssemblyVersion), NpgsqlDbType.Varchar },
				{ nameof(EnvironmentInfo.BaseDirectory), NpgsqlDbType.Varchar },
				{ nameof(EnvironmentInfo.MachineName), NpgsqlDbType.Varchar },
				{ nameof(EnvironmentInfo.CurrentAppDomainName), NpgsqlDbType.Varchar },
				{ nameof(EnvironmentInfo.Is64BitOperatingSystem), NpgsqlDbType.Boolean },
				{ nameof(EnvironmentInfo.Is64BitProcess), NpgsqlDbType.Boolean },
				{ nameof(EnvironmentInfo.OperatingSystemArchitecture), NpgsqlDbType.Varchar },
				{ nameof(EnvironmentInfo.OperatingSystemPlatform), NpgsqlDbType.Varchar },
				{ nameof(EnvironmentInfo.OperatingSystemVersion), NpgsqlDbType.Varchar },
				{ nameof(EnvironmentInfo.ProcessArchitecture), NpgsqlDbType.Varchar },
				{ nameof(EnvironmentInfo.CommandLine), NpgsqlDbType.Varchar }
			};
		}

		public DBEnvironmentInfoSinkOptions Validate()
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

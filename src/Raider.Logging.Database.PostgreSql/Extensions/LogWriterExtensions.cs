using Raider.Logging.Database.PostgreSql;
using System;

namespace Raider.Logging.Extensions
{
	public static class LogWriterExtensions
	{
		public static LogWriterConfiguration ConfigureDbLogWriter(
			this LogWriterConfiguration loggerConfiguration,
			Action<DbLogWriterConfiguration> configuration)
		{
			if (configuration != null)
			{
				var dbLogWriterConfiguration = new DbLogWriterConfiguration();
				configuration.Invoke(dbLogWriterConfiguration);
				var writer = dbLogWriterConfiguration.CreateDbLogWriter();
				if (writer != null)
					DbLogWriter.Instance = writer;
			}

			return loggerConfiguration;
		}
	}
}

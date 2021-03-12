using Raider.AspNetCore.Logging;
using Raider.Logging;
using System;

namespace Raider.AspNetCore.Extensions
{
	public static class LogWriterExtensions
	{
		public static LogWriterConfiguration ConfigureAspNetLogWriter(
			this LogWriterConfiguration loggerConfiguration,
			Action<AspNetLogWriterConfiguration> configuration)
		{
			if (configuration != null)
			{
				var aspNetLogWriterConfiguration = new AspNetLogWriterConfiguration();
				configuration.Invoke(aspNetLogWriterConfiguration);
				var writer = aspNetLogWriterConfiguration.CreateAspNetLogWriter();
				if (writer != null)
					AspNetLogWriter.Instance = writer;
			}

			return loggerConfiguration;
		}
	}
}

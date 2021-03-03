using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Logging.EventLog;
using Serilog;
using System;

namespace Raider.Extensions
{
	public static class LoggingBuilderExtensions
	{
		public static ILoggingBuilder AddRaiderSerilog(
			this ILoggingBuilder configureLogging,
			Serilog.ILogger? logger = null,
			bool dispose = false,
			bool addConsoleLogger = true,
			Action<ConsoleLoggerOptions>? configureConsoleLogger = null,
			bool addDebugLogger = true,
			bool addEventSourceLogger = true,
			bool addEventLogLogger = true,
			Action<EventLogSettings>? configureEventLogLogger = null)
		{
			if (configureLogging == null)
				throw new ArgumentNullException(nameof(configureLogging));

			configureLogging
				.ClearProviders()
				.AddSerilog(logger, dispose);

			if (addConsoleLogger)
			{
				if (configureConsoleLogger == null)
					configureLogging.AddConsole();
				else
					configureLogging.AddConsole(configureConsoleLogger);
			}

			if (addConsoleLogger)
			{
				if (configureConsoleLogger == null)
					configureLogging.AddConsole();
				else
					configureLogging.AddConsole(configureConsoleLogger);
			}

			if (addDebugLogger)
				configureLogging.AddDebug();

			if (addEventSourceLogger)
				configureLogging.AddEventSourceLogger();

			if (addEventLogLogger)
			{
				if (configureEventLogLogger == null)
					configureLogging.AddEventLog();
				else
					configureLogging.AddEventLog(configureEventLogLogger);
			}

			return configureLogging;
		}
	}
}

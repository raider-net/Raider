﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using System;

namespace Raider.Logging.Extensions
{
	public static class ServiceCollectionExtensions
	{
		public static IServiceCollection AddRaiderLogger(this IServiceCollection services, Action<LoggerConfiguration> configurator)
			=> AddRaiderLogger(services, LogLevel.None, configurator);

		public static IServiceCollection AddRaiderLogger(this IServiceCollection services, LogLevel logEventMinimumLevel, Action<LoggerConfiguration> configurator)
		{
			switch (logEventMinimumLevel)
			{
				case LogLevel.Trace:
					LoggerSettings.LevelSwitch.MinimumLevel = Serilog.Events.LogEventLevel.Verbose;
					break;
				case LogLevel.Debug:
					LoggerSettings.LevelSwitch.MinimumLevel = Serilog.Events.LogEventLevel.Debug;
					break;
				case LogLevel.Information:
					LoggerSettings.LevelSwitch.MinimumLevel = Serilog.Events.LogEventLevel.Information;
					break;
				case LogLevel.Warning:
					LoggerSettings.LevelSwitch.MinimumLevel = Serilog.Events.LogEventLevel.Warning;
					break;
				case LogLevel.Error:
					LoggerSettings.LevelSwitch.MinimumLevel = Serilog.Events.LogEventLevel.Error;
					break;
				case LogLevel.Critical:
					LoggerSettings.LevelSwitch.MinimumLevel = Serilog.Events.LogEventLevel.Fatal;
					break;
				case LogLevel.None:
					LoggerSettings.LevelSwitch.MinimumLevel = Serilog.Events.LogEventLevel.Information;
					break;
				default:
					break;
			}

			var loggerConfiguration =
				new LoggerConfiguration()
					.MinimumLevel.ControlledBy(LoggerSettings.LevelSwitch)
					.Enrich.FromLogContext();

			configurator?.Invoke(loggerConfiguration);

			Log.Logger = loggerConfiguration.CreateLogger();

			services.AddLogging(configure => configure.AddSerilog());

			return services;
		}
	}
}
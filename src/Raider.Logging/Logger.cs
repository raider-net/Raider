using System;

namespace Raider.Logging
{
	public static class Logger
	{
		private readonly static Lazy<Microsoft.Extensions.Logging.ILoggerFactory> _loggerFactory = new(CreateLoggerFactory);

		public static Serilog.ILogger? SerilogLogger { get; internal set; }
		internal static Action<Microsoft.Extensions.Logging.ILoggingBuilder>? MicrosoftLoggingBuilder { get; set; }

		private static Microsoft.Extensions.Logging.ILoggerFactory CreateLoggerFactory()
		{
			if (MicrosoftLoggingBuilder == null)
				throw new InvalidOperationException($"{nameof(MicrosoftLoggingBuilder)} is not set. Use services.AddRaiderLogger extension first.");

			var loggerFactory = Microsoft.Extensions.Logging.LoggerFactory.Create(MicrosoftLoggingBuilder);
			return loggerFactory;
		}

		public static Microsoft.Extensions.Logging.ILogger GetLogger<T>()
		{
			var logger = Microsoft.Extensions.Logging.LoggerFactoryExtensions.CreateLogger<T>(_loggerFactory.Value);
			return logger;
		}

		public static Microsoft.Extensions.Logging.ILogger GetLogger(Type type)
		{
			var logger = Microsoft.Extensions.Logging.LoggerFactoryExtensions.CreateLogger(_loggerFactory.Value, type);
			return logger;
		}
	}
}

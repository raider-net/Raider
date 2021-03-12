using Raider.Extensions;
using Raider.Hardware;
using Raider.Infrastructure;
using Raider.Logging.Database.PostgreSql.Internal;
using System;
using System.Threading;

namespace Raider.Logging.Database.PostgreSql
{
	public class DbLogWriter : IDbLogWriter, IDisposable
	{
		private static IDbLogWriter _instance = SilentDbLogWriter.Instance;

		public static IDbLogWriter Instance
		{
			get => _instance;
			set => _instance = value ?? throw new ArgumentNullException(nameof(value));
		}

		private readonly EnvironmentInfoWriter? _environmentInfoWriter;
		private readonly HardwareInfoWriter? _hardwareInfoWriter;

		internal DbLogWriter(
			EnvironmentInfoWriter? environmentInfoWriter,
			HardwareInfoWriter? hardwareInfoWriter)
		{
			_environmentInfoWriter = environmentInfoWriter;
			_hardwareInfoWriter = hardwareInfoWriter;
		}

		public void WriteEnvironmentInfo()
			=> WriteEnvironmentInfo(EnvironmentInfoProvider.GetEnvironmentInfo());

		public void WriteEnvironmentInfo(EnvironmentInfo environmentInfo)
		{
			if (_environmentInfoWriter == null)
				throw new InvalidOperationException($"{nameof(EnvironmentInfoWriter)} was not configured");

			_environmentInfoWriter.Write(environmentInfo);
		}

		public void WriteHardwareInfo(HardwareInfo hardwareInfo)
		{
			if (hardwareInfo == null)
				return;

			if (_hardwareInfoWriter == null)
				throw new InvalidOperationException($"{nameof(HardwareInfoWriter)} was not configured");

			_hardwareInfoWriter.Write(hardwareInfo);
		}

		public static void CloseAndFlush()
		{
			var dbLogWriter = Interlocked.Exchange(ref _instance, SilentDbLogWriter.Instance);
			dbLogWriter?.Dispose();
		}

		private bool disposed;
		protected virtual void Dispose(bool disposing)
		{
			if (!disposed)
			{
				if (disposing)
				{
					try
					{
						_environmentInfoWriter?.Dispose();
					}
					catch (Exception ex)
					{
						var msg = string.Format($"{nameof(LogWriter)}: Disposing {nameof(_environmentInfoWriter)} '{_environmentInfoWriter?.GetType().FullName ?? "null"}': {ex.ToStringTrace()}");
						Serilog.Log.Logger.Error(ex, msg);
					}

					try
					{
						_hardwareInfoWriter?.Dispose();
					}
					catch (Exception ex)
					{
						var msg = string.Format($"{nameof(LogWriter)}: Disposing {nameof(_hardwareInfoWriter)} '{_hardwareInfoWriter?.GetType().FullName ?? "null"}': {ex.ToStringTrace()}");
						Serilog.Log.Logger.Error(ex, msg);
					}
				}

				disposed = true;
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
	}
}

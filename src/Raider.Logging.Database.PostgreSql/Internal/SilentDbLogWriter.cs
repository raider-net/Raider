using Raider.Hardware;
using Raider.Infrastructure;
using System;

namespace Raider.Logging.Database.PostgreSql.Internal
{
	internal class SilentDbLogWriter : IDbLogWriter, IDisposable
	{
		public static readonly IDbLogWriter Instance = new SilentDbLogWriter();

		public void WriteEnvironmentInfo() { }

		public void WriteEnvironmentInfo(EnvironmentInfo environmentInfo) { }

		public void WriteHardwareInfo(HardwareInfo hardwareInfo) { }

		public void Dispose() { }
	}
}

using Raider.Hardware;
using Raider.Infrastructure;
using System;

namespace Raider.Logging.Database.PostgreSql
{
	public interface IDbLogWriter : IDisposable
	{
		void WriteEnvironmentInfo();
		void WriteEnvironmentInfo(EnvironmentInfo environmentInfo);
		void WriteHardwareInfo(HardwareInfo hardwareInfo);
	}
}

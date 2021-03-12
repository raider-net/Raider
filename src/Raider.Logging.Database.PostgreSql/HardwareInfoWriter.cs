using Raider.Database.PostgreSql;
using Raider.Hardware;
using System;
using System.Collections.Generic;

namespace Raider.Logging.Database.PostgreSql
{
	public class HardwareInfoWriter : DbBatchWriter<HardwareInfo>, IDisposable
	{
		public HardwareInfoWriter(DBHardwareInfoSinkOptions options, Action<string, object?, object?, object?>? errorLogger = null)
			: base(options ?? new DBHardwareInfoSinkOptions(), errorLogger ?? DefaultErrorLoggerDelegate.Log)
		{
		}

		public override IDictionary<string, object?>? ToDictionary(HardwareInfo HardwareInfo)
			=> HardwareInfo.ToDictionary();
	}
}

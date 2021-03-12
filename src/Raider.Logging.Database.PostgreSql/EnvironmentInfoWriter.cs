using Raider.Database.PostgreSql;
using Raider.Infrastructure;
using System;
using System.Collections.Generic;

namespace Raider.Logging.Database.PostgreSql
{
	public class EnvironmentInfoWriter : DbBatchWriter<EnvironmentInfo>, IDisposable
	{
		public EnvironmentInfoWriter(DBEnvironmentInfoSinkOptions options, Action<string, object?, object?, object?>? errorLogger = null)
			: base(options ?? new DBEnvironmentInfoSinkOptions(), errorLogger ?? DefaultErrorLoggerDelegate.Log)
		{
		}

		public override IDictionary<string, object?>? ToDictionary(EnvironmentInfo environmentInfo)
			=> environmentInfo.ToDictionary();
	}
}

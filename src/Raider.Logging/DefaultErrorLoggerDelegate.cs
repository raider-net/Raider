using Raider.Extensions;
using System;

namespace Raider.Logging
{
	public static class DefaultErrorLoggerDelegate
	{
		public static void Log(string message, object? batchWriter, object? exception, object? @null)
		{
			string msg;
			if (exception is Exception ex)
			{
				msg = string.Format(message, batchWriter, ex.ToStringTrace());
				Serilog.Log.Logger.Error(ex, msg);
			}
			else
			{
				msg = string.Format(message, batchWriter, exception);
				Serilog.Log.Logger.Error(msg);
			}
		}
	}
}

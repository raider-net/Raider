using Raider.Logging;
using System;

namespace Raider.ServiceBus
{
	public interface IHostLog
	{
		Guid IdHost { get; }
		int IdLogLevel { get; }
		Guid RuntimeUniqueKey { get; }
		DateTime TimeCreatedUtc { get; }
		int IdHostStatus { get; }
		ILogMessage? LogMessage { get; }
		string? LogDetail { get; }
	}
}

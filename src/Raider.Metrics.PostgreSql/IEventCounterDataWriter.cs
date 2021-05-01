using System.Collections.Generic;

namespace Raider.Metrics.PostgreSql
{
	public interface IEventCounterDataWriter
	{
		IReadOnlyDictionary<string, IEventListener> EventListeners { get; }

		void EnableAllEventListeners();
		void DisableWriteForAllEventListeners();
	}
}

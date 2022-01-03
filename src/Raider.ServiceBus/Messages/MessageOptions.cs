using System;

namespace Raider.ServiceBus.Messages
{
	public class MessageOptions
	{
		/// <summary>
		/// The timespan after which the Send request will be cancelled if no response arrives.
		/// </summary>
		public TimeSpan? Timeout { get; set; }

		/// <summary>
		/// Id of the original request that launched the session. Used for tracing messages
		/// </summary>
		public Guid? IdSession { get; set; }
	}
}

using System;

namespace Raider.ServiceBus
{
	public class MessageOptions
	{
		/// <summary>
		/// Sends a message asynchronously, when the caller does not wait for the response.
		/// </summary>
		public bool Async { get; set; }

		/// <summary>
		/// The source queue identifier to store response message from target component
		/// </summary>
		public Guid? IdSourceResponseQueue { get; set; }

		/// <summary>
		/// Component this message is sent to
		/// </summary>
		public Guid IdTargetComponent { get; set; }

		/// <summary>
		/// The target queue identifier
		/// </summary>
		public Guid IdTargetQueue { get; set; }

		/// <summary>
		/// Priority of the message
		/// </summary>
		public MessagePriority Priority { get; set; } = MessagePriority.Normal;

		/// <summary>
		/// Description of the message
		/// </summary>
		public string? Description { get; set; }

		/// <summary>
		/// The timespan after which the Send request will be cancelled if no response arrives.
		/// </summary>
		public TimeSpan? Timeout { get; set; }
	}
}

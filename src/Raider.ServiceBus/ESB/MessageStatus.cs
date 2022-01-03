namespace Raider.ServiceBus
{
	public enum MessageStatus
	{
		/// <summary>
		/// The message status was not changed
		/// </summary>
		Unchanged = 0,

		/// <summary>
		/// The message was created
		/// </summary>
		Created = 1,

		/// <summary>
		/// The message is in target component's queue
		/// </summary>
		Delivered = 2,

		/// <summary>
		/// The message is being processed
		/// </summary>
		InProcess = 3,

		/// <summary>
		/// Discarded message - for example if message validity is expired
		/// </summary>
		Discarded = 4,

		/// <summary>
		/// The message processing was suspended
		/// </summary>
		Suspended = 5,

		/// <summary>
		/// The message processing was deferred
		/// </summary>
		Deferred = 6,

		/// <summary>
		/// The message processing was aborted - not processed correctly
		/// </summary>
		Aborted = 7,

		/// <summary>
		/// The error occured while processing the message
		/// </summary>
		Error = 8,

		/// <summary>
		/// The message processing was completed successfully
		/// </summary>
		Completed = 9
	}
}

namespace Raider.Messaging.Messages
{
	public enum MessageState
	{
		Pending = 1,
		InProcess = 2,
		Consumed = 3,
		Error = 4,
		Suspended = 5,
		Corrupted = 6
	}
}

namespace Raider.Messaging.Messages
{
	public enum SubscriberMessageState
	{
		Pending = 1,
		InProcess = 2,
		Consumed = 3,
		Suspended = 4,
		Corrupted = 5
	}
}

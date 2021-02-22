namespace Raider.Messaging.Messages
{
	public interface IMessageData
	{
		string Serialize();
		IMessageData? Deserialize(string? data);
	}
}

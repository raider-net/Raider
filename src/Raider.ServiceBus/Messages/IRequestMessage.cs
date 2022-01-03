namespace Raider.ServiceBus.Messages
{
	/// <summary>
	/// Marker interface for base request messages.
	/// </summary>
	public interface IBaseRequestMessage
	{
	}

	/// <summary>
	/// Marker interface for request messages.
	/// </summary>
	/// <typeparam name="TResponse">The response message type associated with the request</typeparam>
	public interface IRequestMessage<out TResponse> : IBaseRequestMessage
	{
	}

	/// <summary>
	/// Marker interface for request messages.
	/// </summary>
	public interface IRequestMessage : IRequestMessage<VoidResponseMessage>, IBaseRequestMessage
	{
	}
}

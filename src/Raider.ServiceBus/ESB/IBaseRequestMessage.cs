namespace Raider.ServiceBus
{
	/// <summary>
	/// Marker interface for messages.
	/// </summary>
	public interface IMessage
	{
	}

	/// <summary>
	/// Marker interface for base request messages.
	/// </summary>
	public interface IBaseRequestMessage : IMessage
	{
	}

	/// <summary>
	/// Marker interface for request messages with response.
	/// </summary>
	/// <typeparam name="TResponse">The response message type associated with the request</typeparam>
	public interface IRequestMessage<out TResponse> : IBaseRequestMessage
		where TResponse : IResponseMessage
	{
	}

	/// <summary>
	/// Marker interface for void request messages.
	/// </summary>
	public interface IRequestMessage : IRequestMessage<VoidResponseMessage>, IBaseRequestMessage
	{
	}

	/// <summary>
	/// Marker interface for request messages.
	/// </summary>
	public interface IResponseMessage : IMessage
	{
	}
}

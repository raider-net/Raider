using Raider.Messaging.Messages;

namespace Raider.Messaging.Adapters
{
	public interface ISendAdapter<TData> : ISubscriber<TData>, IComponent
			where TData : IMessageData
	{
	}
}

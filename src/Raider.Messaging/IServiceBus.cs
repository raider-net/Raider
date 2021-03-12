using Raider.Messaging.Messages;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace Raider.Messaging
{
	public interface IServiceBus
	{
		Task<IMessage<TData>> PublishMessageAsync<TData>(int idPublisher, TData mesage, IMessage? previousMessage = null, bool isRecovery = false, IDbTransaction? dbTransaction = null, CancellationToken token = default)
			where TData : IMessageData;
	}
}

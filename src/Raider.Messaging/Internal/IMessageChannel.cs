using Raider.Messaging.Messages;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Raider.Messaging
{
	internal interface IMessageChannel<TData>
		where TData : IMessageData
	{
		IAsyncEnumerable<IMessage<TData>> ReadAllAsync(CancellationToken ct = default);
		
		Task<bool> AddFileAsync(IMessage<TData> message, CancellationToken ct = default);
	}
}

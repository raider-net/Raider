using Microsoft.Extensions.Logging;
using Raider.Messaging.Messages;
using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace Raider.Messaging
{
	public interface IPublisher : IComponent
	{
		Type PublishMessageDataType { get; }

		internal void Initialize(IMessageBox messageBox, ILoggerFactory loggerFactory);
	}

	public interface IPublisher<TData> : IPublisher, IComponent
			where TData : IMessageData
	{
		Task<IMessage<TData>> PublishMessageAsync(TData data, IMessage? previousMessage = null, bool isRecovery = false, IDbTransaction? dbTransaction = null, CancellationToken token = default);
	}
}

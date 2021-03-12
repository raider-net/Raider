using Microsoft.Extensions.Logging;
using Raider.Messaging.Messages;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Raider.Messaging
{
	public interface IJob : IComponent
	{
		internal Task InitializeAsync(IServiceProvider serviceProvider, IMessageBox messageBox, ILoggerFactory loggerFactory, CancellationToken cancellationToken);
		Task<MessageResult> ExecuteAsync(SubscriberContext context, CancellationToken token = default);
	}
}

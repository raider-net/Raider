using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Raider.Messaging
{
	public interface IJob : IComponent
	{
		internal Task InitializeAsync(IServiceProvider serviceProvider, IServiceBusStorage storage, IMessageBox messageBox, ILoggerFactory loggerFactory, CancellationToken cancellationToken);
		Task<ComponentState> ExecuteAsync(JobContext context, CancellationToken token = default);
		Task<bool> Resume(CancellationToken cancellationToken = default);
	}
}

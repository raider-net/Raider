using Raider.Messaging.Messages;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace Raider.Messaging
{
	public interface IServiceBusStorage : IMessageBox
	{
		IServiceBusHost? ServiceBusHost { get; }
		IReadOnlyDictionary<Type, int> MessageTypeIds { get; }


		Task<IServiceBusStorageContext> CreateServiceBusStorageContextAsync(IServiceBusHost serviceBusHost, CancellationToken cancellationToken = default);
		Task SetServiceBusHost(IServiceBusStorageContext context, IServiceBusHost serviceBusHost, CancellationToken cancellationToken = default);

		Task WriteServiceBusHostStartAsync(IServiceBusStorageContext context, CancellationToken cancellationToken = default);
		Task WriteServiceBusHostEndAsync(DateTime endedUtc, CancellationToken cancellationToken = default);
		Task WriteServiceBusLogAsync(LogBase log, CancellationToken cancellationToken = default);
		//void WriteServiceBusLog(LogBase log);


		Task WriteScenarioAsync(IServiceBusStorageContext context, IScenario scenario, DateTime createdUtc, CancellationToken cancellationToken = default);


		Task WriteJobStartAsync(IServiceBusStorageContext context, IJob job, CancellationToken cancellationToken = default);
		Task WriteJobActivityAsync(IJob job, MessageResult? messageResult, LogBase? log, CancellationToken cancellationToken = default);
		Task WriteJobLogAsync(IJob job, LogBase log, CancellationToken cancellationToken = default);
		//void WriteJobLog(Guid instanceId, LogBase log);


		Task WritePublisherStartAsync(IServiceBusStorageContext context, IPublisher publisher, CancellationToken cancellationToken = default);
		Task WritePublisherActivityAsync(IPublisher publisher, LogBase? log, CancellationToken cancellationToken = default);
		Task WritePublisherLogAsync(IPublisher publisher, LogBase log, CancellationToken cancellationToken = default);
		//void WritePublisherLog(Guid instanceId, LogBase log);


		Task WriteSubscriberStartAsync(IServiceBusStorageContext context, ISubscriber subscriber, CancellationToken cancellationToken = default);
		Task WriteSubscriberActivityAsync(ISubscriber subscriber, MessageResult? messageResult, LogBase? log, CancellationToken cancellationToken = default);
		Task WriteSubscriberLogAsync(ISubscriber subscriber, LogBase log, CancellationToken cancellationToken = default);
		//void WriteSubscriberLog(Guid instanceId, LogBase log);


		Task WriteMessageStateAsync(ISubscriber subscriber, MessageResult messageResult, CancellationToken cancellationToken = default);
	}
}

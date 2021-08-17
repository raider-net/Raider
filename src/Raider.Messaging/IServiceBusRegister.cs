using Microsoft.Extensions.Logging;
using Raider.Messaging.Messages;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Raider.Messaging
{
	public interface IServiceBusRegister
	{
		bool RegistrationFinished { get; }

		internal ServiceBusMode GetMode();

		IServiceBusRegister RegisterScenario(int idScenario, string name);

		IServiceBusRegister RegisterPublisher<TData>(int idPublisher, string name, bool writeToSubscribers = true)
			where TData : IMessageData;

		IServiceBusRegister RegisterPublisher<TData>(int idPublisher, string name, int idScenario, bool writeToSubscribers = true)
			where TData : IMessageData;

		IServiceBusRegister RegisterSubscriber<TData>(Subscriber<TData> subscriber)
			where TData : IMessageData;

		IServiceBusRegister RegisterJob(Job job);



		IScenario? TryGetScenario(int idScenario);

		IPublisher<TData>? TryGetPublisher<TData>(int idPublisher)
			where TData : IMessageData;

		ISubscriber<TData>? TryGetSubscriber<TData>(int idSubscriber)
			where TData : IMessageData;

		IJob? TryGetJob(int idJob);

		internal Task InitializeComponentsAsync(
			IServiceProvider serviceProvider,
			IServiceBusStorage storage,
			IServiceBusStorageContext context,
			IMessageBox messageBox,
			ILoggerFactory loggerFactory,
			CancellationToken cancellationToken);

		internal Task StartComponentsAsync(IServiceBusStorageContext context, CancellationToken cancellationToken);

		internal List<ISubscriber> GetMessageSubscribers<TData>()
			where TData : IMessageData;
	}
}

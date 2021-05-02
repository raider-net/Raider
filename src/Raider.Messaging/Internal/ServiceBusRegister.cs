using Microsoft.Extensions.Logging;
using Raider.Messaging.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Raider.Messaging.Internal
{
	internal class ServiceBusRegister : IServiceBusRegister
	{
		private readonly bool _allowedPublishers;
		private readonly bool _allowedSubscribers;
		private readonly bool _allowedJobs;

		private readonly Dictionary<int, Scenario> _scenarios = new Dictionary<int, Scenario>();
		private readonly Dictionary<int, IPublisher> _publishers = new Dictionary<int, IPublisher>();
		private readonly Dictionary<int, ISubscriber> _subscribers = new Dictionary<int, ISubscriber>();
		private readonly Dictionary<int, IJob> _jobs = new Dictionary<int, IJob>();
		private readonly Dictionary<Type, List<ISubscriber>> _messageTypeSubscribers = new Dictionary<Type, List<ISubscriber>>();

		public ServiceBusMode Mode { get; }
		public bool RegistrationFinished { get; private set; }

		public ServiceBusRegister(ServiceBusMode mode, bool allowJobs)
		{
			Mode = mode;
			_allowedPublishers = Mode == ServiceBusMode.OnlyMessagePublishing || mode == ServiceBusMode.PublishingAndSubscribing;
			_allowedSubscribers = Mode == ServiceBusMode.OnlyMessageSubscribing || mode == ServiceBusMode.PublishingAndSubscribing;
			_allowedJobs = allowJobs;

			RegisterScenario(0, "DEFAULT");
		}

		ServiceBusMode IServiceBusRegister.GetMode()
			=> Mode;


		public void FinalizeRegistration()
		{
			if (_allowedPublishers && _publishers.Count == 0)
				throw new InvalidOperationException("No publisher registered");

			if (_allowedSubscribers && _subscribers.Count == 0)
				throw new InvalidOperationException("No subscriber registered");

			if (_allowedJobs && _jobs.Count == 0)
				throw new InvalidOperationException("No job registered");

			if (Mode == ServiceBusMode.PublishingAndSubscribing)
			{
				var publishMessageDataTypes = _publishers.Values.Select(x => x.PublishingMessageDataType).Distinct().ToDictionary(x => x, y => 0);
				var notPublishedMessageDataTypes = new List<Type>();

				foreach (var subscriber in _subscribers.Values)
				{
					if (publishMessageDataTypes.TryGetValue(subscriber.SubscribingMessageDataType, out int count))
					{
						publishMessageDataTypes[subscriber.SubscribingMessageDataType] = count + 1;
					}
					else
					{
						notPublishedMessageDataTypes.Add(subscriber.SubscribingMessageDataType);
					}
				}

				var notSubscribed = publishMessageDataTypes.Where(kvp => kvp.Value == 0).Select(kvp => kvp.Key).ToList();
				var notPublished = notPublishedMessageDataTypes.Distinct().ToList();


				if (Mode == ServiceBusMode.PublishingAndSubscribing && 0 < notSubscribed.Count)
					throw new InvalidOperationException($"Not subscribed message data types:{Environment.NewLine}{string.Join(Environment.NewLine, notSubscribed.Select(x => x.FullName))}");

				if (Mode == ServiceBusMode.PublishingAndSubscribing && 0 < notPublished.Count)
					throw new InvalidOperationException($"Not published message data types:{Environment.NewLine}{string.Join(Environment.NewLine, notPublished.Select(x => x.FullName))}");
			}

			RegistrationFinished = true;
		}


		public IServiceBusRegister RegisterScenario(int idScenario, string name)
		{
			if (RegistrationFinished)
				throw new NotSupportedException("Component registration was finished.");

			var scenario = new Scenario(idScenario, name);
			var added = _scenarios.TryAdd(scenario.IdScenario, scenario);

			if (!added)
			{
				if (_scenarios[scenario.IdScenario].Name != name)
					throw new InvalidOperationException($"{nameof(idScenario)} = {idScenario} already registered with {nameof(name)} = {name}");
			}

			return this;
		}



		public IServiceBusRegister RegisterPublisher<TData>(int idPublisher, string name)
			where TData : IMessageData
			=> RegisterPublisher<TData>(idPublisher, name, 0);

		public IServiceBusRegister RegisterPublisher<TData>(int idPublisher, string name, int idScenario)
			where TData : IMessageData
		{
			if (!_allowedPublishers)
				return this;

			if (RegistrationFinished)
				throw new NotSupportedException("Component registration was finished.");

			var publisher = new Publisher<TData>(idPublisher, name, idScenario)
			{
				Register = this
			};

			var added = _publishers.TryAdd(publisher.IdComponent, publisher);

			if (!added)
			{
				if (_publishers[publisher.IdComponent].Name != name)
					throw new InvalidOperationException($"{nameof(idPublisher)} = {idPublisher} already registered with {nameof(name)} = {name}");
			}

			return this;
		}



		public IServiceBusRegister RegisterSubscriber<TData>(Subscriber<TData> subscriber)
			where TData : IMessageData
		{
			if (!_allowedSubscribers)
				return this;

			if (RegistrationFinished)
				throw new NotSupportedException("Component registration was finished.");

			if (subscriber == null)
				throw new ArgumentNullException(nameof(subscriber));

			var added = _subscribers.TryAdd(subscriber.IdComponent, subscriber);

			if (!added)
				throw new InvalidOperationException($"{nameof(subscriber)} = {subscriber.IdComponent} already registered");

			if (_messageTypeSubscribers.TryGetValue(typeof(TData), out List<ISubscriber>? messageSubscribers))
			{
				messageSubscribers.Add(subscriber);
			}
			else
			{
				_messageTypeSubscribers.Add(typeof(TData), new List<ISubscriber> { subscriber });
			}

			subscriber.Register = this;

			return this;
		}



		public IServiceBusRegister RegisterJob(Job job)
		{
			if (!_allowedJobs)
				return this;

			if (RegistrationFinished)
				throw new NotSupportedException("Component registration was finished.");

			if (job == null)
				throw new ArgumentNullException(nameof(job));

			var added = _jobs.TryAdd(job.IdComponent, job);

			if (!added)
				throw new InvalidOperationException($"{nameof(job)} = {job.IdComponent} already registered");

			job.Register = this;

			return this;
		}





		public IScenario? TryGetScenario(int idScenario)
		{
			_scenarios.TryGetValue(idScenario, out Scenario scenario);
			return scenario;
		}

		public IPublisher<TData>? TryGetPublisher<TData>(int idPublisher)
			where TData : IMessageData
		{
			_publishers.TryGetValue(idPublisher, out IPublisher? publisher);
			return publisher as IPublisher<TData>;
		}

		public ISubscriber<TData>? TryGetSubscriber<TData>(int idSubscriber)
			where TData : IMessageData
		{
			_subscribers.TryGetValue(idSubscriber, out ISubscriber? subscriber);
			return subscriber as ISubscriber<TData>;
		}

		public IJob? TryGetJob(int idJob)
		{
			_jobs.TryGetValue(idJob, out IJob? job);
			return job;
		}

		async Task IServiceBusRegister.InitializeComponentsAsync(
			IServiceProvider serviceProvider,
			IServiceBusStorage storage,
			IServiceBusStorageContext context,
			IMessageBox messageBox,
			ILoggerFactory loggerFactory,
			CancellationToken cancellationToken)
		{
			var nowUtc = DateTime.UtcNow;
			foreach (var scenario in _scenarios.Values)
				await storage.WriteScenarioAsync(context, scenario, nowUtc, cancellationToken);

			if (_allowedPublishers)
			{
				foreach (var publisher in _publishers.Values)
					await publisher.InitializeAsync(storage, messageBox, loggerFactory, cancellationToken);
			}

			if (_allowedSubscribers)
			{
				foreach (var subscriber in _subscribers.Values)
					await subscriber.InitializeAsync(serviceProvider, storage, messageBox, loggerFactory, cancellationToken);
			}

			if (_allowedJobs)
			{
				foreach (var job in _jobs.Values)
					await job.InitializeAsync(serviceProvider, storage, messageBox, loggerFactory, cancellationToken);
			}
		}

		async Task IServiceBusRegister.StartComponentsAsync(IServiceBusStorageContext context, CancellationToken cancellationToken)
		{
			if (_allowedPublishers)
			{
				foreach (var publisher in _publishers.Values)
					await publisher.StartAsync(context, cancellationToken);
			}

			if (_allowedSubscribers)
			{
				foreach (var subscriber in _subscribers.Values)
					await subscriber.StartAsync(context, cancellationToken);
			}

			if (_allowedJobs)
			{
				foreach (var job in _jobs.Values)
					await job.StartAsync(context, cancellationToken);
			}
		}

		List<ISubscriber> IServiceBusRegister.GetMessageSubscribers<TData>()
		{
			if (_messageTypeSubscribers.TryGetValue(typeof(TData), out List<ISubscriber>? result))
				return result;
			else
				return new List<ISubscriber>();
		}
	}
}

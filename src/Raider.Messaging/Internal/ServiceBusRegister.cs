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
		private readonly ServiceBusMode _mode;
		private readonly bool _allowedPublishers;
		private readonly bool _allowedSubscribers;
		private readonly bool _allowedJobs;

		private readonly Dictionary<int, Scenario> _scenarios = new Dictionary<int, Scenario>();
		private readonly Dictionary<int, IPublisher> _publishers = new Dictionary<int, IPublisher>();
		private readonly Dictionary<int, ISubscriber> _subscribers = new Dictionary<int, ISubscriber>();
		private readonly Dictionary<int, IJob> _jobs = new Dictionary<int, IJob>();
		private readonly Dictionary<Type, List<ISubscriber>> _messageTypeSubscribers = new Dictionary<Type, List<ISubscriber>>();

		public bool RegistrationFinished { get; private set; }

		public ServiceBusRegister(ServiceBusMode mode, bool allowJobs)
		{
			_mode = mode;
			_allowedPublishers = _mode == ServiceBusMode.OnlyMessagePublishing || mode == ServiceBusMode.PublishingAndSubscribing;
			_allowedSubscribers = _mode == ServiceBusMode.OnlyMessageSubscribing || mode == ServiceBusMode.PublishingAndSubscribing;
			_allowedJobs = allowJobs;

			RegisterScenario(0, "DEFAULT");
		}

		ServiceBusMode IServiceBusRegister.GetMode()
			=> _mode;


		public void FinalizeRegistration(out List<Type> notSubscribed, out List<Type> notPublished)
		{
			if (_allowedPublishers && _publishers.Count == 0)
				throw new InvalidOperationException("No publisher registered");

			if (_allowedSubscribers && _subscribers.Count == 0)
				throw new InvalidOperationException("No subscriber registered");

			if (_mode == ServiceBusMode.PublishingAndSubscribing)
			{
				var publishMessageDataTypes = _publishers.Values.Select(x => x.PublishMessageDataType).Distinct().ToDictionary(x => x, y => 0);
				var notPublishedMessageDataTypes = new List<Type>();

				foreach (var subscriber in _subscribers.Values)
				{
					if (publishMessageDataTypes.TryGetValue(subscriber.SubscribeMessageDataType, out int count))
					{
						publishMessageDataTypes[subscriber.SubscribeMessageDataType] = count + 1;
					}
					else
					{
						notPublishedMessageDataTypes.Add(subscriber.SubscribeMessageDataType);
					}
				}

				notSubscribed = publishMessageDataTypes.Where(kvp => kvp.Value == 0).Select(kvp => kvp.Key).ToList();
				notPublished = notPublishedMessageDataTypes.Distinct().ToList();
			}
			else
			{
				notSubscribed = new List<Type>();
				notPublished = new List<Type>();
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
				throw new InvalidOperationException($"{nameof(idScenario)} = {idScenario} already registered");

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
				throw new InvalidOperationException($"{nameof(idPublisher)} = {idPublisher} already registered");

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
			return job as IJob;
		}

		void IServiceBusRegister.InitializePublishers(IMessageBox messageBox, ILoggerFactory loggerFactory)
		{
			if (!_allowedPublishers)
				return;

			foreach (var publisher in _publishers.Values)
				publisher.Initialize(messageBox, loggerFactory);
		}

		async Task IServiceBusRegister.InitializeComponentsAsync(IServiceProvider serviceProvider, IMessageBox messageBox, ILoggerFactory loggerFactory, CancellationToken cancellationToken)
		{
			if (_allowedPublishers)
			{
				foreach (var publisher in _publishers.Values)
					publisher.Initialize(messageBox, loggerFactory);
			}

			if (_allowedSubscribers)
			{
				foreach (var subscriber in _subscribers.Values)
					await subscriber.InitializeAsync(serviceProvider, messageBox, loggerFactory, cancellationToken);
			}

			if (_allowedJobs)
			{
				foreach (var job in _jobs.Values)
					await job.InitializeAsync(serviceProvider, messageBox, loggerFactory, cancellationToken);
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

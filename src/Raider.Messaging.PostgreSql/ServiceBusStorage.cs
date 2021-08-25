using Npgsql;
using Raider.Messaging.Messages;
using Raider.Messaging.PostgreSql.Database;
using Raider.Threading;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Raider.Messaging.PostgreSql
{
	public class ServiceBusStorage : IServiceBusStorage
	{
		private readonly Dictionary<Type, int> _messageTypeIds;
		private readonly DbServiceBusHost _dbServiceBusHost;
		private readonly DbServiceBusHostRuntime _dbServiceBusHostRuntime;
		private readonly DbServiceBusLog _dbServiceBusLog;
		private readonly DbScenario _dbScenario;
		private readonly DbJob _dbJob;
		private readonly DbJobInstance _dbJobInstance;
		private readonly DbJobInstanceLog _dbJobInstanceLog;
		private readonly DbPublisher _dbPublisher;
		private readonly DbPublisherInstance _dbPublisherInstance;
		private readonly DbPublisherInstanceLog _dbPublisherInstanceLog;
		private readonly DbSubscriber _dbSubscriber;
		private readonly DbSubscriberInstance _dbSubscriberInstance;
		private readonly DbSubscriberInstanceLog _dbSubscriberInstanceLog;
		private readonly DbMessageType _dbMessageType;
		private readonly DbMessage _dbMessage;
		private readonly DbSubscriberMessage _dbSubscriberMessage;
		private readonly DbMessageTempQueue _dbMessageTempQueue;
		private readonly DbSnapshot _dbSnapshot;

		public IServiceBusHost? ServiceBusHost { get; private set; }
		public IReadOnlyDictionary<Type, int> MessageTypeIds => _messageTypeIds;

		public ServiceBusStorage()
		{
			_messageTypeIds = new Dictionary<Type, int>();
			_dbServiceBusHost = new DbServiceBusHost();
			_dbServiceBusHostRuntime = new DbServiceBusHostRuntime();
			_dbServiceBusLog = new DbServiceBusLog();
			_dbScenario = new DbScenario();
			_dbJob = new DbJob();
			_dbJobInstance = new DbJobInstance();
			_dbJobInstanceLog = new DbJobInstanceLog();
			_dbPublisher = new DbPublisher();
			_dbPublisherInstance = new DbPublisherInstance();
			_dbPublisherInstanceLog = new DbPublisherInstanceLog();
			_dbSubscriber = new DbSubscriber();
			_dbSubscriberInstance = new DbSubscriberInstance();
			_dbSubscriberInstanceLog = new DbSubscriberInstanceLog();
			_dbMessage = new DbMessage();
			_dbMessageType = new DbMessageType();
			_dbSubscriberMessage = new DbSubscriberMessage();
			_dbMessageTempQueue = new DbMessageTempQueue();
			_dbSnapshot = new DbSnapshot();
		}

		//private readonly object _setConnectionStringLock = new object();
		//public void SetConncetionString(string conncetionString)
		//{
		//	if (string.IsNullOrWhiteSpace(conncetionString))
		//		throw new ArgumentNullException(nameof(conncetionString));

		//	if (!string.IsNullOrWhiteSpace(ConncetionString))
		//		throw new InvalidOperationException($"{nameof(ConncetionString)} already set.");

		//	lock(_setConnectionStringLock)
		//	{
		//		if (!string.IsNullOrWhiteSpace(ConncetionString))
		//			throw new InvalidOperationException($"{nameof(ConncetionString)} already set.");

		//		ConncetionString = conncetionString;
		//	}
		//}

		private async Task<NpgsqlConnection> CreateConnectionAsync(IServiceBusHost serviceBusHost, CancellationToken cancellationToken = default)
		{
			var connection = new NpgsqlConnection(serviceBusHost.ConnectionString);
			await connection.OpenAsync(cancellationToken);
			return connection;
		}

		public async Task<IServiceBusStorageContext> CreateServiceBusStorageContextAsync(IServiceBusHost serviceBusHost, CancellationToken cancellationToken = default)
		{
			if (serviceBusHost == null)
				throw new ArgumentNullException(nameof(serviceBusHost));

			var connection = await CreateConnectionAsync(serviceBusHost, cancellationToken);
			return new ServiceBusStorageContext(connection);
		}

		private readonly AsyncLock _setServiceBusHostLock = new();
		public async Task SetServiceBusHost(IServiceBusStorageContext context, IServiceBusHost serviceBusHost, CancellationToken cancellationToken = default)
		{
			if (context == null)
				throw new ArgumentNullException(nameof(context));

			if (serviceBusHost == null)
				throw new ArgumentNullException(nameof(serviceBusHost));

			if (ServiceBusHost != null)
				throw new InvalidOperationException($"{nameof(ServiceBusHost)} already set.");

			using (await _setServiceBusHostLock.LockAsync())
			{
				if (ServiceBusHost != null)
					throw new InvalidOperationException($"{nameof(ServiceBusHost)} already set.");

				var connection = ((ServiceBusStorageContext)context).Connection;
				var exists = await _dbServiceBusHost.ExistsAsync(connection, null, serviceBusHost, cancellationToken);

				if (!exists)
					await _dbServiceBusHost.InsertAsync(connection, null, serviceBusHost, cancellationToken);

				ServiceBusHost = serviceBusHost;
			}
		}

		public async Task WriteServiceBusHostStartAsync(IServiceBusStorageContext context, CancellationToken cancellationToken = default)
		{
			if (context == null)
				throw new ArgumentNullException(nameof(context));

			if (ServiceBusHost == null)
				throw new InvalidOperationException($"{nameof(ServiceBusHost)} was not set.");

			await _dbServiceBusHostRuntime.InsertAsync(((ServiceBusStorageContext)context).Connection, null, ServiceBusHost, cancellationToken);
		}

		public async Task WriteServiceBusHostEndAsync(DateTime endedUtc, CancellationToken cancellationToken = default)
		{
			if (ServiceBusHost == null)
				throw new InvalidOperationException($"{nameof(ServiceBusHost)} was not set.");

			using var connection = await CreateConnectionAsync(ServiceBusHost, cancellationToken);

			await _dbServiceBusHostRuntime.UpdateAsync(connection, null, ServiceBusHost, endedUtc, cancellationToken);
		}

		public async Task WriteServiceBusLogAsync(LogBase log, CancellationToken cancellationToken = default)
		{
			if (log == null)
				throw new ArgumentNullException(nameof(log));

			if (ServiceBusHost == null)
				throw new InvalidOperationException($"{nameof(ServiceBusHost)} was not set.");

			using var connection = await CreateConnectionAsync(ServiceBusHost, cancellationToken);

			await _dbServiceBusLog.InsertAsync(connection, null, ServiceBusHost, log, cancellationToken);
		}



		public async Task WriteScenarioAsync(IServiceBusStorageContext context, IScenario scenario, DateTime createdUtc, CancellationToken cancellationToken = default)
		{
			if (context == null)
				throw new ArgumentNullException(nameof(context));

			if (scenario == null)
				throw new ArgumentNullException(nameof(scenario));

			if (ServiceBusHost == null)
				throw new InvalidOperationException($"{nameof(ServiceBusHost)} was not set.");

			var connection = ((ServiceBusStorageContext)context).Connection;
			var exists = await _dbScenario.ExistsAsync(connection, null, scenario, cancellationToken);

			if (!exists)
				await _dbScenario.InsertAsync(((ServiceBusStorageContext)context).Connection, null, ServiceBusHost, scenario, createdUtc, cancellationToken);
		}



		public async Task WriteJobStartAsync(IServiceBusStorageContext context, IJob job, CancellationToken cancellationToken = default)
		{
			if (context == null)
				throw new ArgumentNullException(nameof(context));

			if (job == null)
				throw new ArgumentNullException(nameof(job));

			if (ServiceBusHost == null)
				throw new InvalidOperationException($"{nameof(ServiceBusHost)} was not set.");

			var connection = ((ServiceBusStorageContext)context).Connection;
			var exists = await _dbJob.ExistsAsync(connection, null, job, cancellationToken);

			if (!exists)
				await _dbJob.InsertAsync(((ServiceBusStorageContext)context).Connection, null, ServiceBusHost, job, cancellationToken);

			await _dbJobInstance.InsertAsync(connection, null, ServiceBusHost, job, cancellationToken);
		}

		public async Task WriteJobActivityAsync(IJob job, MessageResult? messageResult, LogBase? log, CancellationToken cancellationToken = default)
		{
			if (job == null)
				throw new ArgumentNullException(nameof(job));

			if (messageResult != null)
				throw new NotSupportedException(nameof(MessageResult)); //TODO

			if (ServiceBusHost == null)
				throw new InvalidOperationException($"{nameof(ServiceBusHost)} was not set.");

			using var connection = await CreateConnectionAsync(ServiceBusHost, cancellationToken);
			using var tran = await connection.BeginTransactionAsync(cancellationToken);

			await _dbJobInstance.UpdateAsync(connection, tran, job, cancellationToken);

			//if (messageResult != null)
			//	await _dbSnapshot.InsertOrUpdate(connection, tran, idSubscriberInstance, messageResult, cancellationToken); //TODO

			if (log != null)
				await _dbJobInstanceLog.InsertAsync(connection, tran, job, log, cancellationToken);

			tran.Commit();
		}

		public async Task WriteJobLogAsync(IJob job, LogBase log, CancellationToken cancellationToken = default)
		{
			if (job == null)
				throw new ArgumentNullException(nameof(job));

			if (log == null)
				throw new ArgumentNullException(nameof(log));

			if (ServiceBusHost == null)
				throw new InvalidOperationException($"{nameof(ServiceBusHost)} was not set.");

			using var connection = await CreateConnectionAsync(ServiceBusHost, cancellationToken);

			await _dbJobInstanceLog.InsertAsync(connection, null, job, log, cancellationToken);
		}



		private async Task<int> AddOrGetMessageTypeId(NpgsqlConnection connection, Type type, CancellationToken cancellationToken = default)
		{
			if (type == null)
				throw new ArgumentNullException(nameof(type));

			if (_messageTypeIds.TryGetValue(type, out int idMessageType))
				return idMessageType;

			var dbId = await _dbMessageType.GetIdMessageTypeAsync(connection, null, type, cancellationToken);
			if (dbId.HasValue)
			{
				_messageTypeIds.Add(type, dbId.Value);
				return dbId.Value;
			}

			if (ServiceBusHost == null)
				throw new InvalidOperationException($"{nameof(ServiceBusHost)} was not set.");

			var id = await _dbMessageType.InsertAsync(connection, null, ServiceBusHost, type, cancellationToken);
			_messageTypeIds.Add(type, id);

			return id;
		}

		private int GetMessageTypeId<T>()
		{
			var type = typeof(T);
			if (_messageTypeIds.TryGetValue(type, out int idMessageType))
				return idMessageType;

			throw new InvalidOperationException($"Unknow type {type.FullName}");
		}

		private int GetMessageTypeId(Type type)
		{
			if (type == null)
				throw new ArgumentNullException(nameof(type));

			if (_messageTypeIds.TryGetValue(type, out int idMessageType))
				return idMessageType;

			throw new InvalidOperationException($"Unknow type {type.FullName}");
		}


		public async Task WritePublisherStartAsync(IServiceBusStorageContext context, IPublisher publisher, CancellationToken cancellationToken = default)
		{
			if (context == null)
				throw new ArgumentNullException(nameof(context));

			if (publisher == null)
				throw new ArgumentNullException(nameof(publisher));

			if (ServiceBusHost == null)
				throw new InvalidOperationException($"{nameof(ServiceBusHost)} was not set.");

			var connection = ((ServiceBusStorageContext)context).Connection;
			var idMessageType = await AddOrGetMessageTypeId(connection, publisher.PublishingMessageDataType, cancellationToken);
			var exists = await _dbPublisher.ExistsAsync(connection, null, publisher, idMessageType, cancellationToken);

			if (!exists)
				await _dbPublisher.InsertAsync(((ServiceBusStorageContext)context).Connection, null, ServiceBusHost, publisher, idMessageType, cancellationToken);

			await _dbPublisherInstance.InsertAsync(connection, null, ServiceBusHost, publisher, cancellationToken);
		}

		public async Task WritePublisherActivityAsync(IPublisher publisher, LogBase? log, CancellationToken cancellationToken = default)
		{
			if (publisher == null)
				throw new ArgumentNullException(nameof(publisher));

			if (ServiceBusHost == null)
				throw new InvalidOperationException($"{nameof(ServiceBusHost)} was not set.");

			using var connection = await CreateConnectionAsync(ServiceBusHost, cancellationToken);
			using var tran = await connection.BeginTransactionAsync(cancellationToken);

			await _dbPublisherInstance.UpdateAsync(connection, tran, publisher, cancellationToken);

			if (log != null)
				await _dbPublisherInstanceLog.InsertAsync(connection, tran, publisher, log, cancellationToken);

			tran.Commit();
		}

		public async Task WritePublisherLogAsync(IPublisher publisher, LogBase log, CancellationToken cancellationToken = default)
		{
			if (publisher == null)
				throw new ArgumentNullException(nameof(publisher));

			if (log == null)
				throw new ArgumentNullException(nameof(log));

			if (ServiceBusHost == null)
				throw new InvalidOperationException($"{nameof(ServiceBusHost)} was not set.");

			using var connection = await CreateConnectionAsync(ServiceBusHost, cancellationToken);

			await _dbPublisherInstanceLog.InsertAsync(connection, null, publisher, log, cancellationToken);
		}



		public async Task WriteSubscriberStartAsync(IServiceBusStorageContext context, ISubscriber subscriber, CancellationToken cancellationToken = default)
		{
			if (context == null)
				throw new ArgumentNullException(nameof(context));

			if (subscriber == null)
				throw new ArgumentNullException(nameof(subscriber));

			if (ServiceBusHost == null)
				throw new InvalidOperationException($"{nameof(ServiceBusHost)} was not set.");

			var connection = ((ServiceBusStorageContext)context).Connection;
			var idMessageType = await AddOrGetMessageTypeId(connection, subscriber.SubscribingMessageDataType, cancellationToken);
			var exists = await _dbSubscriber.ExistsAsync(connection, null, subscriber, idMessageType, cancellationToken);

			if (!exists)
				await _dbSubscriber.InsertAsync(((ServiceBusStorageContext)context).Connection, null, ServiceBusHost, subscriber, idMessageType, cancellationToken);

			await _dbSubscriberInstance.InsertAsync(connection, null, ServiceBusHost, subscriber, cancellationToken);
		}

		public async Task WriteSubscriberActivityAsync(ISubscriber subscriber, MessageResult? messageResult, LogBase? log, CancellationToken cancellationToken = default)
		{
			if (subscriber == null)
				throw new ArgumentNullException(nameof(subscriber));

			if (ServiceBusHost == null)
				throw new InvalidOperationException($"{nameof(ServiceBusHost)} was not set.");

			using var connection = await CreateConnectionAsync(ServiceBusHost, cancellationToken);
			using var tran = await connection.BeginTransactionAsync(cancellationToken);

			await _dbSubscriberInstance.UpdateAsync(connection, tran, subscriber, cancellationToken);

			if (messageResult != null)
				await _dbSubscriberMessage.UpdateAsync(connection, tran, subscriber.IdInstance, messageResult, cancellationToken);

			if (log != null)
				await _dbSubscriberInstanceLog.InsertAsync(connection, tran, subscriber, log, cancellationToken);

			tran.Commit();
		}

		public async Task WriteSubscriberLogAsync(ISubscriber subscriber, LogBase log, CancellationToken cancellationToken = default)
		{
			if (subscriber == null)
				throw new ArgumentNullException(nameof(subscriber));

			if (log == null)
				throw new ArgumentNullException(nameof(log));

			if (ServiceBusHost == null)
				throw new InvalidOperationException($"{nameof(ServiceBusHost)} was not set.");

			using var connection = await CreateConnectionAsync(ServiceBusHost, cancellationToken);

			await _dbSubscriberInstanceLog.InsertAsync(connection, null, subscriber, log, cancellationToken);
		}



		public async Task WriteMessageStateAsync(ISubscriber subscriber, MessageResult messageResult, CancellationToken cancellationToken = default)
		{
			if (subscriber == null)
				throw new ArgumentNullException(nameof(subscriber));

			if (messageResult == null)
				throw new ArgumentNullException(nameof(messageResult));

			if (ServiceBusHost == null)
				throw new InvalidOperationException($"{nameof(ServiceBusHost)} was not set.");

			using var connection = await CreateConnectionAsync(ServiceBusHost, cancellationToken);

			await _dbSubscriberMessage.UpdateAsync(connection, null, subscriber.IdInstance, messageResult, cancellationToken);
		}




		public async Task<ISubscriberMessage<TData>?> GetSubscriberMessageFromFIFOAsync<TData>(
			ISubscriber<TData> subscriber,
			List<int> readMessageStates,
			DateTime utcNow,
			CancellationToken cancellationToken = default)
			where TData : IMessageData
		{
			if (subscriber == null)
				throw new ArgumentNullException(nameof(subscriber));

			if (ServiceBusHost == null)
				throw new InvalidOperationException($"{nameof(ServiceBusHost)} was not set.");

			ISubscriberMessage<TData>? result;
			using var connection = await CreateConnectionAsync(ServiceBusHost, cancellationToken);

			result = await _dbSubscriberMessage.GetSubscriberMessageFromFIFOAsync(connection, null, subscriber, readMessageStates, utcNow, cancellationToken);
			return result;
		}

		public async Task<ISubscriberMessage<TData>?> GetSubscriberMessageFromNonFIFOAsync<TData>(
			ISubscriber<TData> subscriber,
			List<int> readMessageStates,
			DateTime utcNow,
			CancellationToken cancellationToken = default)
			where TData : IMessageData
		{
			if (subscriber == null)
				throw new ArgumentNullException(nameof(subscriber));

			if (ServiceBusHost == null)
				throw new InvalidOperationException($"{nameof(ServiceBusHost)} was not set.");

			ISubscriberMessage<TData>? result;
			using var connection = await CreateConnectionAsync(ServiceBusHost, cancellationToken);

			result = await _dbSubscriberMessage.GetSubscriberMessageFromNonFIFOAsync(connection, null, subscriber, readMessageStates, utcNow, cancellationToken);
			return result;
		}

		public async Task WriteMessageAsync<TData>(IMessage<TData> message, IReadOnlyList<ISubscriber> subscribers, IDbTransaction? dbTransaction = null, CancellationToken cancellationToken = default)
			where TData : IMessageData
		{
			if (message == null)
				throw new ArgumentNullException(nameof(message));

			if (ServiceBusHost == null)
				throw new InvalidOperationException($"{nameof(ServiceBusHost)} was not set.");

			var idMessageType = GetMessageTypeId<TData>();

			if (dbTransaction == null)
			{
				//TODO: preco je dbTransaction == null ???? ... mala by byt vzdy vyplnena

				using var connection = await CreateConnectionAsync(ServiceBusHost, cancellationToken);
				using var tran = await connection.BeginTransactionAsync(cancellationToken);

				await _dbMessage.InsertAsync(connection, tran, message, idMessageType, cancellationToken);

				if (subscribers == null || subscribers.Count == 0)
				{
					await _dbMessageTempQueue.InsertAsync(connection, tran, message, MessageState.Pending, cancellationToken);
				}
				else
				{
					await _dbSubscriberMessage.InsertAsync(connection, tran, subscribers.Select(s => s.IdComponent).ToList(), message, MessageState.Pending, cancellationToken);
				}

				await tran.CommitAsync(cancellationToken);
			}
			else
			{
				if (dbTransaction is not NpgsqlTransaction npgsqlTran)
					throw new ArgumentException($"{nameof(dbTransaction)} must be {nameof(NpgsqlTransaction)}", nameof(dbTransaction));

				var connection = npgsqlTran.Connection;
				if (connection == null)
					throw new InvalidConstraintException("Transaction has no connection.");

				await _dbMessage.InsertAsync(connection, npgsqlTran, message, idMessageType, cancellationToken);

				if (subscribers == null || subscribers.Count == 0)
				{
					await _dbMessageTempQueue.InsertAsync(connection, npgsqlTran, message, MessageState.Pending, cancellationToken);
				}
				else
				{
					await _dbSubscriberMessage.InsertAsync(connection, npgsqlTran, subscribers.Select(s => s.IdComponent).ToList(), message, MessageState.Pending, cancellationToken);
				}
			}
		}
	}
}

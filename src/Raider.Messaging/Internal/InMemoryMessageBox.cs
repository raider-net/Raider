//using Raider.Messaging.Messages;
//using System;
//using System.Collections.Concurrent;
//using System.Collections.Generic;
//using System.Data;
//using System.Linq;
//using System.Threading;
//using System.Threading.Tasks;

//namespace Raider.Messaging.Internal
//{
//	public class InMemoryMessageBox : IMessageBox
//	{
//		private readonly ConcurrentDictionary<int, ConcurrentQueue<ISubscriberMessage>> _subscriberMessages = new ConcurrentDictionary<int, ConcurrentQueue<ISubscriberMessage>>();

//		public Task<ISubscriberMessage<TData>?> GetFirstSubscriberMessageAsync<TData>(ISubscriber<TData> subscriber, CancellationToken cancellationToken = default)
//			where TData : IMessageData
//		{
//			if (subscriber == null)
//				throw new ArgumentNullException(nameof(subscriber));

//			ISubscriberMessage? message = null;

//			if (_subscriberMessages.TryGetValue(subscriber.IdComponent, out ConcurrentQueue<ISubscriberMessage>? subscriberQueue))
//			{
//				if (subscriberQueue.TryPeek(out message))
//					return TryReadMessage(false, message, subscriber, subscriberQueue, cancellationToken);
//			}

//			return Task.FromResult(message as ISubscriberMessage<TData>);
//		}

//		public Task<ISubscriberMessage<TData>?> GetNextSubscriberMessageAsync<TData>(ISubscriber<TData> subscriber, CancellationToken cancellationToken = default)
//			where TData : IMessageData
//		{
//			if (subscriber == null)
//				throw new ArgumentNullException(nameof(subscriber));

//			ISubscriberMessage? message = null;

//			if (_subscriberMessages.TryGetValue(subscriber.IdComponent, out ConcurrentQueue<ISubscriberMessage>? subscriberQueue))
//			{
//				if (subscriberQueue.TryPeek(out message))
//					return TryReadMessage(true, message, subscriber, subscriberQueue, cancellationToken);
//			}

//			return Task.FromResult(message as ISubscriberMessage<TData>);
//		}

//		private void MarkMessageAsInProcess(ISubscriberMessage message)
//			=> message.UpdateMessage(MessageState.InProcess, message.RetryCount, message.DelayedToUtc);

//		private Task<ISubscriberMessage<TData>?> ConvertMessage<TData>(ISubscriberMessage message)
//			where TData : IMessageData
//		{
//			if (message is ISubscriberMessage<TData>)
//			{
//				return Task.FromResult(message as ISubscriberMessage<TData>);
//			}
//			else
//			{
//				throw new InvalidOperationException($"Invalid message type {message?.GetType().FullName}");
//			}
//		}

//		private Task<ISubscriberMessage<TData>?> TryReadMessage<TData>(bool tryNext, ISubscriberMessage message, ISubscriber<TData> subscriber, ConcurrentQueue<ISubscriberMessage> subscriberQueue, CancellationToken cancellationToken = default)
//			where TData : IMessageData
//		{
//			var utcNow = DateTimeOffset.UtcNow;

//			if (message.State == MessageState.Pending)
//			{
//				MarkMessageAsInProcess(message);
//				return ConvertMessage<TData>(message);
//			}


//			else if (message.State == MessageState.InProcess)
//			{
//				if (message.LastAccessUtc.Add(subscriber.TimeoutForMessageProcessingInSeconds) < utcNow)
//				{
//					MarkMessageAsInProcess(message);
//					return ConvertMessage<TData>(message);
//				}
//				else
//				{
//					if (!tryNext)
//						return Task.FromResult((ISubscriberMessage<TData>?)null);

//					var nextMessage = subscriberQueue.Skip(1).FirstOrDefault();
//					if (nextMessage == null)
//						return Task.FromResult((ISubscriberMessage<TData>?)null);

//					return TryReadMessage(tryNext, nextMessage, subscriber, subscriberQueue, cancellationToken);
//				}
//			}


//			else if (message.State == MessageState.Consumed)
//			{
//				if (!tryNext)
//					throw new InvalidOperationException($"{MessageState.Consumed} message found in queue.");

//				var nextMessage = subscriberQueue.Skip(1).FirstOrDefault();
//				if (nextMessage == null)
//					return Task.FromResult((ISubscriberMessage<TData>?)null);

//				return TryReadMessage(tryNext, nextMessage, subscriber, subscriberQueue, cancellationToken);
//			}


//			else if (message.State == MessageState.Error)
//			{
//				if (message.DelayedToUtc < utcNow)
//				{
//					MarkMessageAsInProcess(message);
//					return ConvertMessage<TData>(message);
//				}
//				else
//				{
//					if (!tryNext)
//						return Task.FromResult((ISubscriberMessage<TData>?)null);

//					var nextMessage = subscriberQueue.Skip(1).FirstOrDefault();
//					if (nextMessage == null)
//						return Task.FromResult((ISubscriberMessage<TData>?)null);

//					return TryReadMessage(tryNext, nextMessage, subscriber, subscriberQueue, cancellationToken);
//				}
//			}


//			else if (message.State == MessageState.Suspended)
//			{
//				if (!tryNext)
//					throw new InvalidOperationException($"{MessageState.Suspended} message found in queue.");

//				var nextMessage = subscriberQueue.Skip(1).FirstOrDefault();
//				if (nextMessage == null)
//					return Task.FromResult((ISubscriberMessage<TData>?)null);

//				return TryReadMessage(tryNext, nextMessage, subscriber, subscriberQueue, cancellationToken);
//			}


//			else if (message.State == MessageState.Corrupted)
//			{
//				if (!tryNext)
//					throw new InvalidOperationException($"{MessageState.Corrupted} message found in queue.");

//				var nextMessage = subscriberQueue.Skip(1).FirstOrDefault();
//				if (nextMessage == null)
//					return Task.FromResult((ISubscriberMessage<TData>?)null);

//				return TryReadMessage(tryNext, nextMessage, subscriber, subscriberQueue, cancellationToken);
//			}

//			throw new InvalidOperationException($"Invalid state {message.State}");
//		}

//		public Task SetMessageStateAsync<TData>(ISubscriberMessage<TData> subscriberMessage, MessageResult result, IDbTransaction? dbTransaction = null, CancellationToken cancellationToken = default)
//			where TData : IMessageData
//			=> SetMessageStateAsync<TData>(subscriberMessage, result.State, result.RetryCount, result.DelayedToUtc, dbTransaction, cancellationToken);

//		public Task SetMessageStateAsync<TData>(ISubscriberMessage<TData> subscriberMessage, MessageState state, int retryCount, DateTimeOffset? delayedToUtc, IDbTransaction? dbTransaction = null, CancellationToken cancellationToken = default)
//			where TData : IMessageData
//		{
//			if (subscriberMessage == null)
//				throw new ArgumentNullException(nameof(subscriberMessage));

//			MessageResult.Validate(subscriberMessage, state, retryCount, delayedToUtc);

//			if (_subscriberMessages.TryGetValue(subscriberMessage.IdSubscriber, out ConcurrentQueue<ISubscriberMessage>? subscriberQueue))
//			{
//				if (subscriberQueue.TryPeek(out ISubscriberMessage? topMessage))
//				{
//					if (topMessage.IdSubscriberMessage == subscriberMessage.IdSubscriberMessage)
//					{
//						if (state == MessageState.Consumed)
//						{
//							subscriberQueue.TryDequeue(out _); //remove from queue
//						}
//						else
//						{
//							topMessage.UpdateMessage(state, retryCount, delayedToUtc);
//						}
//					}
//					else
//					{
//						throw new InvalidOperationException($"No message '{subscriberMessage.IdSubscriberMessage}' found for subscriber: {subscriberMessage.IdSubscriber}");
//					}
//				}
//				else
//				{
//					throw new InvalidOperationException($"No message found for subscriber: {subscriberMessage.IdSubscriber}");
//				}
//			}
//			else
//			{
//				throw new InvalidOperationException($"Invalid subscriber: {subscriberMessage.IdSubscriber}");
//			}

//			return Task.CompletedTask;
//		}

//		public Task SetComponentStateAsync(IComponent component, ComponentState state, IDbTransaction? dbTransaction = null, CancellationToken cancellationToken = default)
//			=> Task.CompletedTask;

//		public Task WriteMessageAsync<TData>(IMessage<TData> message, IReadOnlyList<ISubscriber> subscribers, IDbTransaction? dbTransaction = null, CancellationToken cancellationToken = default)
//			where TData : IMessageData
//		{
//			if (message == null)
//				return Task.CompletedTask;

//			if (subscribers == null || subscribers.Count == 0)
//				throw new ArgumentNullException(nameof(subscribers));

//			foreach (var subscriber in subscribers)
//			{
//				var subscriberQueue = _subscriberMessages.GetOrAdd(subscriber.IdComponent, x => new ConcurrentQueue<ISubscriberMessage>());

//				subscriberQueue.Enqueue(
//					new SubscriberMessage<TData>(message, subscriber.IdComponent, Guid.Empty)
//					{
//						Data = (TData)message.Data?.Deserialize(message.Data?.Serialize()) //clone data
//					});
//			}

//			return Task.CompletedTask;
//		}

//		public Task<ISubscriberMessage<TData>?> GetSubscriberMessageFromFIFOAsync<TData>(ISubscriber<TData> subscriber, List<int> readMessageStates, CancellationToken cancellationToken = default) where TData : IMessageData
//		{
//			throw new NotImplementedException();
//		}

//		public Task<ISubscriberMessage<TData>?> GetSubscriberMessageFromNonFIFOAsync<TData>(ISubscriber<TData> subscriber, List<int> readMessageStates, DateTime utcNow, CancellationToken cancellationToken = default) where TData : IMessageData
//		{
//			throw new NotImplementedException();
//		}

//		public Task WriteMessageAsync<TData>(IMessage<TData> message, IReadOnlyList<ISubscriber> subscribers, IDbTransaction? dbTransaction = null, CancellationToken cancellationToken = default) where TData : IMessageData
//		{
//			throw new NotImplementedException();
//		}
//	}
//}

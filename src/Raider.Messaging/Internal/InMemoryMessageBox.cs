using Raider.Messaging.Messages;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Raider.Messaging.Internal
{
	public class InMemoryMessageBox : IMessageBox
	{
		private readonly ConcurrentDictionary<int, ConcurrentQueue<ISubscriberMessage>> _subscriberMessages = new ConcurrentDictionary<int, ConcurrentQueue<ISubscriberMessage>>();

		public Task<ISubscriberMessage<TData>?> GetFirstSubscriberMessageAsync<TData>(ISubscriber<TData> subscriber, CancellationToken cancellationToken = default)
			where TData : IMessageData
		{
			if (subscriber == null)
				throw new ArgumentNullException(nameof(subscriber));

			ISubscriberMessage? message = null;

			if (_subscriberMessages.TryGetValue(subscriber.IdComponent, out ConcurrentQueue<ISubscriberMessage>? subscriberQueue))
			{
				if (subscriberQueue.TryPeek(out message))
					return TryReadMessage(false, message, subscriber, subscriberQueue, cancellationToken);
			}

			return Task.FromResult(message as ISubscriberMessage<TData>);
		}

		public Task<ISubscriberMessage<TData>?> GetNextSubscriberMessageAsync<TData>(ISubscriber<TData> subscriber, CancellationToken cancellationToken = default)
			where TData : IMessageData
		{
			if (subscriber == null)
				throw new ArgumentNullException(nameof(subscriber));

			ISubscriberMessage? message = null;

			if (_subscriberMessages.TryGetValue(subscriber.IdComponent, out ConcurrentQueue<ISubscriberMessage>? subscriberQueue))
			{
				if (subscriberQueue.TryPeek(out message))
					return TryReadMessage(true, message, subscriber, subscriberQueue, cancellationToken);
			}

			return Task.FromResult(message as ISubscriberMessage<TData>);
		}

		private void MarkMessageAsInProcess(ISubscriberMessage message)
			=> message.UpdateMessage(SubscriberMessageState.InProcess, message.RetryCount, message.DelayedToUtc);

		private Task<ISubscriberMessage<TData>?> ConvertMessage<TData>(ISubscriberMessage message)
			where TData : IMessageData
		{
			if (message is ISubscriberMessage<TData>)
			{
				return Task.FromResult(message as ISubscriberMessage<TData>);
			}
			else
			{
				throw new InvalidOperationException($"Invalid message type {message?.GetType().FullName}");
			}
		}

		private Task<ISubscriberMessage<TData>?> TryReadMessage<TData>(bool tryNext, ISubscriberMessage message, ISubscriber<TData> subscriber, ConcurrentQueue<ISubscriberMessage> subscriberQueue, CancellationToken cancellationToken = default)
			where TData : IMessageData
		{
			var utcNow = DateTimeOffset.UtcNow;

			if (message.State == SubscriberMessageState.Pending)
			{
				MarkMessageAsInProcess(message);
				return ConvertMessage<TData>(message);
			}


			else if (message.State == SubscriberMessageState.InProcess)
			{
				if (message.LastAccessUtc.Add(subscriber.MessageInProcessTimeout) < utcNow)
				{
					MarkMessageAsInProcess(message);
					return ConvertMessage<TData>(message);
				}
				else
				{
					if (!tryNext)
						return Task.FromResult((ISubscriberMessage<TData>?)null);

					var nextMessage = subscriberQueue.Skip(1).FirstOrDefault();
					if (nextMessage == null)
						return Task.FromResult((ISubscriberMessage<TData>?)null);

					return TryReadMessage(tryNext, nextMessage, subscriber, subscriberQueue, cancellationToken);
				}
			}


			else if (message.State == SubscriberMessageState.Consumed)
			{
				if (!tryNext)
					throw new InvalidOperationException($"{SubscriberMessageState.Consumed} message found in queue.");

				var nextMessage = subscriberQueue.Skip(1).FirstOrDefault();
				if (nextMessage == null)
					return Task.FromResult((ISubscriberMessage<TData>?)null);

				return TryReadMessage(tryNext, nextMessage, subscriber, subscriberQueue, cancellationToken);
			}


			else if (message.State == SubscriberMessageState.Error)
			{
				if (message.DelayedToUtc < utcNow)
				{
					MarkMessageAsInProcess(message);
					return ConvertMessage<TData>(message);
				}
				else
				{
					if (!tryNext)
						return Task.FromResult((ISubscriberMessage<TData>?)null);

					var nextMessage = subscriberQueue.Skip(1).FirstOrDefault();
					if (nextMessage == null)
						return Task.FromResult((ISubscriberMessage<TData>?)null);

					return TryReadMessage(tryNext, nextMessage, subscriber, subscriberQueue, cancellationToken);
				}
			}


			else if (message.State == SubscriberMessageState.Suspended)
			{
				if (!tryNext)
					throw new InvalidOperationException($"{SubscriberMessageState.Suspended} message found in queue.");

				var nextMessage = subscriberQueue.Skip(1).FirstOrDefault();
				if (nextMessage == null)
					return Task.FromResult((ISubscriberMessage<TData>?)null);

				return TryReadMessage(tryNext, nextMessage, subscriber, subscriberQueue, cancellationToken);
			}


			else if (message.State == SubscriberMessageState.Corrupted)
			{
				if (!tryNext)
					throw new InvalidOperationException($"{SubscriberMessageState.Corrupted} message found in queue.");

				var nextMessage = subscriberQueue.Skip(1).FirstOrDefault();
				if (nextMessage == null)
					return Task.FromResult((ISubscriberMessage<TData>?)null);

				return TryReadMessage(tryNext, nextMessage, subscriber, subscriberQueue, cancellationToken);
			}

			throw new InvalidOperationException($"Invalid state {message.State}");
		}

		public Task SetMessageStateAsync<TData>(ISubscriberMessage<TData> subscriberMessage, SubscribedMessageResult result, CancellationToken cancellationToken = default)
			where TData : IMessageData
			=> SetMessageStateAsync<TData>(subscriberMessage, result.State, result.RetryCount, result.DelayedToUtc, cancellationToken);

		public Task SetMessageStateAsync<TData>(ISubscriberMessage<TData> subscriberMessage, SubscriberMessageState state, int retryCount, DateTimeOffset? delayedToUtc, CancellationToken cancellationToken = default)
			where TData : IMessageData
		{
			if (subscriberMessage == null)
				throw new ArgumentNullException(nameof(subscriberMessage));

			SubscribedMessageResult.Validate(subscriberMessage, state, retryCount, delayedToUtc);

			if (_subscriberMessages.TryGetValue(subscriberMessage.IdSubscriber, out ConcurrentQueue<ISubscriberMessage>? subscriberQueue))
			{
				if (subscriberQueue.TryPeek(out ISubscriberMessage? topMessage))
				{
					if (topMessage.IdSubscriberMessage == subscriberMessage.IdSubscriberMessage)
					{
						if (state == SubscriberMessageState.Consumed)
						{
							subscriberQueue.TryDequeue(out _); //remove from queue
						}
						else
						{
							topMessage.UpdateMessage(state, retryCount, delayedToUtc);
						}
					}
					else
					{
						throw new InvalidOperationException($"No message '{subscriberMessage.IdSubscriberMessage}' found for subscriber: {subscriberMessage.IdSubscriber}");
					}
				}
				else
				{
					throw new InvalidOperationException($"No message found for subscriber: {subscriberMessage.IdSubscriber}");
				}
			}
			else
			{
				throw new InvalidOperationException($"Invalid subscriber: {subscriberMessage.IdSubscriber}");
			}

			return Task.CompletedTask;
		}

		public Task SetSubscriberStateAsync(ISubscriber subscriber, ComponentState state, CancellationToken cancellationToken = default)
			=> Task.CompletedTask;

		public Task<bool> WriteAsync<TData>(List<IMessage<TData>> messages, IReadOnlyList<ISubscriber> subscribers, CancellationToken cancellationToken = default)
			where TData : IMessageData
		{
			if (messages == null)
				return Task.FromResult(false);

			if (subscribers == null || subscribers.Count == 0)
				throw new ArgumentNullException(nameof(subscribers));

			foreach (var subscriber in subscribers)
			{
				var subscriberQueue = _subscriberMessages.GetOrAdd(subscriber.IdComponent, x => new ConcurrentQueue<ISubscriberMessage>());

				foreach (var message in messages)
				{
					subscriberQueue.Enqueue(
						new SubscriberMessage<TData>(message, subscriber.IdComponent)
						{
							Data = (TData)message.Data?.Deserialize(message.Data?.Serialize()) //clone data
						});
				}
			}

			return Task.FromResult(true);
		}
	}
}

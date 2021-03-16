using System;
using System.Collections.Generic;
using System.Linq;

namespace Raider.Messaging.Messages
{
	public class MessageResult
	{
		public DateTime CreatedUtc { get; }
		public Guid IdSubscriberMessage { get; internal set; }
		public MessageState State { get; internal set; }
		public int RetryCount { get; internal set; }
		public DateTimeOffset? DelayedToUtc { get; internal set; }

		internal MessageResult()
		{
			CreatedUtc = DateTime.UtcNow;
		}

		public static MessageResult Consume(ISubscriberMessage message)
			=> new MessageResult
			{
				IdSubscriberMessage = message.IdSubscriberMessage,
				State = MessageState.Consumed,
				RetryCount = message.RetryCount,
				DelayedToUtc = message.DelayedToUtc
			};

		public static MessageResult Error(ISubscriberMessage message, TimeSpan delay)
			=> new MessageResult
			{
				IdSubscriberMessage = message.IdSubscriberMessage,
				State = MessageState.Error,
				RetryCount = message.RetryCount + 1,
				DelayedToUtc = DateTimeOffset.UtcNow.Add(delay)
			};

		public static MessageResult Error(ISubscriberMessage message, Dictionary<int, TimeSpan>? delayTable, TimeSpan defaultTimeSpan)
			=> (delayTable == null || delayTable.Count == 0 || delayTable.All(x => x.Key < 0))
			? throw new ArgumentNullException(nameof(delayTable))
			: new MessageResult
			{
				IdSubscriberMessage = message.IdSubscriberMessage,
				State = MessageState.Error,
				RetryCount = message.RetryCount + 1,
				DelayedToUtc = DateTimeOffset.UtcNow.Add(FindDelay(message.RetryCount, delayTable, defaultTimeSpan))
			};

		public static MessageResult Suspended(ISubscriberMessage message)
			=> new MessageResult
			{
				IdSubscriberMessage = message.IdSubscriberMessage,
				State = MessageState.Suspended,
				RetryCount = message.RetryCount + 1,
				DelayedToUtc = null
			};

		public static MessageResult Corrupted(ISubscriberMessage message)
			=> new MessageResult
			{
				IdSubscriberMessage = message.IdSubscriberMessage,
				State = MessageState.Corrupted,
				RetryCount = message.RetryCount,
				DelayedToUtc = null
			};

		private static TimeSpan FindDelay(int currentRetryCount, Dictionary<int, TimeSpan>? delayTable, TimeSpan defaultTimeSpan)
		{
			if (delayTable == null)
				return defaultTimeSpan;

			TimeSpan? result = null;
			int? bestDelta = null;
			foreach (var retry in delayTable.Keys.Where(x => 0 <= x))
			{
				var delta = Math.Abs(retry - currentRetryCount);
				if (bestDelta.HasValue)
				{
					if ((delta < bestDelta.Value)
						|| (delta == bestDelta.Value && delayTable[retry] < result))
					{
						bestDelta = delta;
						result = delayTable[retry];
					}
				}
				else
				{
					bestDelta = delta;
					result = delayTable[retry];
				}
			}

			return result ?? defaultTimeSpan;
		}

		internal static void Validate(ISubscriberMessage subscriberMessage, MessageState state, int retryCount, DateTimeOffset? delayedToUtc)
		{
			if (subscriberMessage == null)
				throw new ArgumentNullException(nameof(subscriberMessage));

			if (subscriberMessage.State != MessageState.InProcess
				&& subscriberMessage.State != MessageState.Error)
				throw new InvalidOperationException($"Cannot set message state {subscriberMessage.State} to {state}");

			switch (state)
			{
				case MessageState.Pending:
					throw new InvalidOperationException($"Cannot set message state {subscriberMessage.State} to {state}");
				case MessageState.InProcess:
					throw new InvalidOperationException($"Cannot set message state {subscriberMessage.State} to {state}");
				case MessageState.Consumed:
					{
						if (subscriberMessage.RetryCount == retryCount && subscriberMessage.DelayedToUtc == delayedToUtc)
						{
							return;
						}

						throw new InvalidOperationException($"Cannot set message state to {state} by changing {nameof(retryCount)} or {nameof(delayedToUtc)}");
					}
				case MessageState.Error:
					{
						if (subscriberMessage.RetryCount + 1 == retryCount
							&& ((!subscriberMessage.DelayedToUtc.HasValue && delayedToUtc.HasValue)
							|| (subscriberMessage.DelayedToUtc.HasValue && delayedToUtc.HasValue && subscriberMessage.DelayedToUtc < delayedToUtc)))
						{
							return;
						}

						throw new InvalidOperationException($"Cannot set message state to {state} withhout changing {nameof(retryCount)} and {nameof(delayedToUtc)}");
					}
				case MessageState.Suspended:
					{
						if (subscriberMessage.RetryCount + 1 == retryCount)
						{
							return;
						}

						throw new InvalidOperationException($"Cannot set message state to {state} withhout changing {nameof(retryCount)} and {nameof(delayedToUtc)}");
					}
				case MessageState.Corrupted:
					return;
				default:
					throw new InvalidOperationException($"Invalid {state}");
			}
		}
	}
}

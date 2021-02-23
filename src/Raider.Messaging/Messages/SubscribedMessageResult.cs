using System;
using System.Collections.Generic;
using System.Linq;

namespace Raider.Messaging.Messages
{
	public class SubscribedMessageResult
	{
		public SubscriberMessageState State { get; set; }
		public int RetryCount { get; set; }
		public DateTimeOffset? DelayedToUtc { get; set; }

		internal SubscribedMessageResult()
		{
		}

		public static SubscribedMessageResult Consume(ISubscriberMessage message)
			=> new SubscribedMessageResult
			{
				State = SubscriberMessageState.Consumed,
				RetryCount = message.RetryCount,
				DelayedToUtc = message.DelayedToUtc
			};

		public static SubscribedMessageResult Error(ISubscriberMessage message, TimeSpan delay)
			=> new SubscribedMessageResult
			{
				State = SubscriberMessageState.Error,
				RetryCount = message.RetryCount + 1,
				DelayedToUtc = DateTimeOffset.UtcNow.Add(delay)
			};

		public static SubscribedMessageResult Error(ISubscriberMessage message, Dictionary<int, TimeSpan>? delayTable, TimeSpan defaultTimeSpan)
			=> (delayTable == null || delayTable.Count == 0 || delayTable.All(x => x.Key < 0))
			? throw new ArgumentNullException(nameof(delayTable))
			: new SubscribedMessageResult
				{
					State = SubscriberMessageState.Error,
					RetryCount = message.RetryCount + 1,
					DelayedToUtc = DateTimeOffset.UtcNow.Add(FindDelay(message.RetryCount, delayTable, defaultTimeSpan))
				};

		public static SubscribedMessageResult Suspended(ISubscriberMessage message)
			=> new SubscribedMessageResult
			{
				State = SubscriberMessageState.Suspended,
				RetryCount = message.RetryCount + 1,
				DelayedToUtc = null
			};

		public static SubscribedMessageResult Corrupted(ISubscriberMessage message)
			=> new SubscribedMessageResult
			{
				State = SubscriberMessageState.Corrupted,
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

		internal static void Validate(ISubscriberMessage subscriberMessage, SubscriberMessageState state, int retryCount, DateTimeOffset? delayedToUtc)
		{
			if (subscriberMessage == null)
				throw new ArgumentNullException(nameof(subscriberMessage));

			if (subscriberMessage.State != SubscriberMessageState.InProcess
				&& subscriberMessage.State != SubscriberMessageState.Error)
				throw new InvalidOperationException($"Cannot set message state {subscriberMessage.State} to {state}");

			switch (state)
			{
				case SubscriberMessageState.Pending:
					throw new InvalidOperationException($"Cannot set message state {subscriberMessage.State} to {state}");
				case SubscriberMessageState.InProcess:
					throw new InvalidOperationException($"Cannot set message state {subscriberMessage.State} to {state}");
				case SubscriberMessageState.Consumed:
					{
						if (subscriberMessage.RetryCount == retryCount && subscriberMessage.DelayedToUtc == delayedToUtc)
						{
							return;
						}

						throw new InvalidOperationException($"Cannot set message state to {state} by changing {nameof(retryCount)} or {nameof(delayedToUtc)}");
					}
				case SubscriberMessageState.Error:
					{
						if (subscriberMessage.RetryCount + 1 == retryCount
							&& ((!subscriberMessage.DelayedToUtc.HasValue && delayedToUtc.HasValue)
							|| (subscriberMessage.DelayedToUtc.HasValue && delayedToUtc.HasValue && subscriberMessage.DelayedToUtc < delayedToUtc)))
						{
							return;
						}

						throw new InvalidOperationException($"Cannot set message state to {state} withhout changing {nameof(retryCount)} and {nameof(delayedToUtc)}");
					}
				case SubscriberMessageState.Suspended:
					{
						if (subscriberMessage.RetryCount + 1 == retryCount)
						{
							return;
						}

						throw new InvalidOperationException($"Cannot set message state to {state} withhout changing {nameof(retryCount)} and {nameof(delayedToUtc)}");
					}
				case SubscriberMessageState.Corrupted:
					return;
				default:
					throw new InvalidOperationException($"Invalid {state}");
			}
		}
	}
}

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
		public string? Snapshot { get; internal set; }
		public int RetryCount { get; internal set; }
		public DateTimeOffset? DelayedToUtc { get; internal set; }
		public Guid OriginalConcurrencyToken { get; internal set; }
		public Guid NewConcurrencyToken { get; internal set; }

		internal MessageResult()
		{
			CreatedUtc = DateTime.UtcNow;
		}

		public static MessageResult Consume(ISubscriberMessage message, string? snapshot = null)
			=> new()
			{
				IdSubscriberMessage = message.IdSubscriberMessage,
				State = MessageState.Consumed,
				Snapshot = snapshot ?? message.Snapshot,
				RetryCount = message.RetryCount,
				DelayedToUtc = message.DelayedToUtc,
				OriginalConcurrencyToken = message.OriginalConcurrencyToken,
				NewConcurrencyToken = message.NewConcurrencyToken
			};

		public static MessageResult Error(ISubscriberMessage message, TimeSpan delay, string? snapshot = null)
			=> new()
			{
				IdSubscriberMessage = message.IdSubscriberMessage,
				State = MessageState.Error,
				Snapshot = snapshot ?? message.Snapshot,
				RetryCount = message.RetryCount + 1,
				DelayedToUtc = DateTimeOffset.UtcNow.Add(delay),
				OriginalConcurrencyToken = message.OriginalConcurrencyToken,
				NewConcurrencyToken = message.NewConcurrencyToken
			};

		public static MessageResult Error(ISubscriberMessage message, Dictionary<int, TimeSpan>? delayTable, TimeSpan defaultTimeSpan, string? snapshot = null)
			=> (delayTable == null || delayTable.Count == 0 || delayTable.All(x => x.Key < 0))
			? throw new ArgumentNullException(nameof(delayTable))
			: new MessageResult
				{
					IdSubscriberMessage = message.IdSubscriberMessage,
					State = MessageState.Error,
					Snapshot = snapshot ?? message.Snapshot,
					RetryCount = message.RetryCount + 1,
					DelayedToUtc = DateTimeOffset.UtcNow.Add(FindDelay(message.RetryCount, delayTable, defaultTimeSpan)),
					OriginalConcurrencyToken = message.OriginalConcurrencyToken,
					NewConcurrencyToken = message.NewConcurrencyToken
				};

		public static MessageResult Suspended(ISubscriberMessage message, string? snapshot = null)
			=> new()
			{
				IdSubscriberMessage = message.IdSubscriberMessage,
				State = MessageState.Suspended,
				Snapshot = snapshot ?? message.Snapshot,
				RetryCount = message.RetryCount + 1,
				DelayedToUtc = null,
				OriginalConcurrencyToken = message.OriginalConcurrencyToken,
				NewConcurrencyToken = message.NewConcurrencyToken
			};

		public static MessageResult Corrupted(ISubscriberMessage message, string? snapshot = null)
			=> new()
			{
				IdSubscriberMessage = message.IdSubscriberMessage,
				State = MessageState.Corrupted,
				Snapshot = snapshot ?? message.Snapshot,
				RetryCount = message.RetryCount,
				DelayedToUtc = null,
				OriginalConcurrencyToken = message.OriginalConcurrencyToken,
				NewConcurrencyToken = message.NewConcurrencyToken
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

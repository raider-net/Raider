using System;

namespace Raider.Messaging.Messages
{
	public class SubscribedMessageResult
	{
		public SubscriberMessageState State { get; set; }
		public int RetryCount { get; set; }
		public DateTimeOffset? DelayedToUtc { get; set; }

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

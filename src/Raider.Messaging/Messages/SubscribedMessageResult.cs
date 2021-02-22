using System;

namespace Raider.Messaging.Messages
{
	public class SubscribedMessageResult
	{
		public SubscriberMessageState State { get; set; }
		public int RetryCount { get; set; }
		public DateTimeOffset? DelayedToUtc { get; set; }

		internal bool IsValidFor(ISubscriberMessage subscriberMessage)
		{
			if (subscriberMessage == null)
				throw new ArgumentNullException(nameof(subscriberMessage));

			if (subscriberMessage.State != SubscriberMessageState.InProcess)
				throw new InvalidOperationException($"Cannot set message state {subscriberMessage.State} to {State}");

			switch (State)
			{
				case SubscriberMessageState.Pending:
					throw new InvalidOperationException($"Cannot set message state {subscriberMessage.State} to {State}");
				case SubscriberMessageState.InProcess:
					throw new InvalidOperationException($"Cannot set message state {subscriberMessage.State} to {State}");
				case SubscriberMessageState.Consumed:
					return true;
				case SubscriberMessageState.Suspended:
					{
						if (subscriberMessage.RetryCount < RetryCount
							|| (!subscriberMessage.DelayedToUtc.HasValue && DelayedToUtc.HasValue)
							|| (subscriberMessage.DelayedToUtc.HasValue && DelayedToUtc.HasValue && subscriberMessage.DelayedToUtc < DelayedToUtc))
						{
							return true;
						}

						throw new InvalidOperationException($"Cannot set message state to {State} withhout changing {nameof(RetryCount)} or {nameof(DelayedToUtc)}");
					}
				case SubscriberMessageState.Corrupted:
					return true;
				default:
					throw new InvalidOperationException($"Invalid {State}");
			}
		}
	}
}

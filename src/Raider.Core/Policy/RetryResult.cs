using System;

namespace Raider.Policy
{
	public struct RetryResult
	{
		public bool CanRetry { get; }
		public TimeSpan? WaitDuration { get; }

		public RetryResult(bool canRetry, TimeSpan? waitDuration)
		{
			CanRetry = canRetry;
			WaitDuration = waitDuration;
		}
	}
}

using System;

namespace Raider.Data
{
	public interface IBatchWriterOptions
	{
		bool EagerlyEmitFirstEvent { get; set; }

		/// <summary>
		/// Max events count in one batch written do database
		/// </summary>
		int BatchSizeLimit { get; set; }

		/// <summary>
		/// Flush period even if BatchSizeLimit is not exceeded
		/// </summary>
		TimeSpan Period { get; set; }

		/// <summary>
		/// Minimum delay to retry, if database insert fails
		/// </summary>
		TimeSpan MinimumBackoffPeriod { get; set; }

		/// <summary>
		/// Maximum delay to retry, if database insert fails
		/// </summary>
		TimeSpan MaximumBackoffInterval { get; set; }

		/// <summary>
		/// If QueueLimit is exceeded, new events will be dropped
		/// </summary>
		int? QueueLimit { get; set; }
	}
}

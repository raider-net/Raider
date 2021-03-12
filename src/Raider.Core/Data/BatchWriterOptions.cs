using System;

namespace Raider.Data
{
	public class BatchWriterOptions : IBatchWriterOptions
	{
		/// <summary>
		/// Eagerly write the first received event to the database to check, if database table is ready
		/// </summary>
		public bool EagerlyEmitFirstEvent { get; set; } = true;

		/// <summary>
		/// Max events count in one batch written do database
		/// </summary>
		public int BatchSizeLimit { get; set; } = 1000;

		/// <summary>
		/// Flush period even if BatchSizeLimit is not exceeded
		/// </summary>
		public TimeSpan Period { get; set; } = TimeSpan.FromSeconds(2);

		/// <summary>
		/// Minimum delay to retry, if database insert fails
		/// </summary>
		public TimeSpan MinimumBackoffPeriod { get; set; } = TimeSpan.FromSeconds(5);

		/// <summary>
		/// Maximum delay to retry, if database insert fails
		/// </summary>
		public TimeSpan MaximumBackoffInterval { get; set; } = TimeSpan.FromMinutes(10);

		/// <summary>
		/// If QueueLimit is exceeded, new events will be dropped
		/// </summary>
		public int? QueueLimit { get; set; } = 100000;
	}
}

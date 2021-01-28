namespace Raider.Sql.Metadata
{
	/// <summary>
	///     A simple model for a database sequence.
	/// </summary>
	public class DatabaseSequence
	{
		/// <summary>
		///     The database that contains the sequence.
		/// </summary>
		public DatabaseModel? Database { get; set; }

		/// <summary>
		///     The sequence name.
		/// </summary>
		public string? Name { get; set; }

		/// <summary>
		///     The schema that contains the sequence, or <c>null</c> to use the default schema.
		/// </summary>
		public string? Schema { get; set; }

		/// <summary>
		///     The database/store type of the sequence, or <c>null</c> if not set.
		/// </summary>
		public string? StoreType { get; set; }

		/// <summary>
		///     The start value for the sequence, or <c>null</c> if not set.
		/// </summary>
		public long? StartValue { get; set; }

		/// <summary>
		///     The amount to increment by to generate the next value in, the sequence, or <c>null</c> if not set.
		/// </summary>
		public int? IncrementBy { get; set; }

		/// <summary>
		///     The minimum value supported by the sequence, or <c>null</c> if not set.
		/// </summary>
		public long? MinValue { get; set; }

		/// <summary>
		///     The maximum value supported by the sequence, or <c>null</c> if not set.
		/// </summary>
		public long? MaxValue { get; set; }

		/// <summary>
		///     Indicates whether or not the sequence will start over when the max value is reached, or <c>null</c> if not set.
		/// </summary>
		public bool? IsCyclic { get; set; }
	}
}

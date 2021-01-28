using System.Collections.Generic;

namespace Raider.Sql.Metadata
{
	/// <summary>
	///     A simple model for a database index.
	/// </summary>
	public class DatabaseIndex
	{
		/// <summary>
		///     The table that contains the index.
		/// </summary>
		public DatabaseTable? Table { get; set; }

		/// <summary>
		///     The index name.
		/// </summary>
		public string? Name { get; set; }

		/// <summary>
		///     The ordered list of columns that make up the index.
		/// </summary>
		public IList<DatabaseColumn> Columns { get; } = new List<DatabaseColumn>();

		/// <summary>
		///     Indicates whether or not the index constrains uniqueness.
		/// </summary>
		public bool IsUnique { get; set; }

		/// <summary>
		///     The filter expression, or <c>null</c> if the index has no filter.
		/// </summary>
		public string? Filter { get; set; }
	}
}

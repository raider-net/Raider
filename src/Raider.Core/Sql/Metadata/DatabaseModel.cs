using System;
using System.Collections.Generic;

namespace Raider.Sql.Metadata
{
	/// <summary>
	///     A simple model for a database.
	/// </summary>
	public class DatabaseModel
	{
		/// <summary>
		///     The database provider type.
		/// </summary>
		public DatabaseProviderType ProviderType { get; set; }

		/// <summary>
		///     The database identifier.
		/// </summary>
		public int? DatabaseId { get; set; }

		/// <summary>
		///     The database name.
		/// </summary>
		public string? DatabaseName { get; set; }

		/// <summary>
		///     The database collation name.
		/// </summary>
		public string? CollationName { get; set; }

		/// <summary>
		///     The database creation datetime.
		/// </summary>
		public DateTime? CreationDate { get; set; }

		/// <summary>
		///     The database schema, or <c>null</c> to use the default schema.
		/// </summary>
		public string? DefaultSchema { get; set; }

		/// <summary>
		///     The list of schemas in the database.
		/// </summary>
		public IList<DatabaseSchema> Schemas { get; } = new List<DatabaseSchema>();

		private bool built = false;
		public DatabaseModel BuildDatabase()
		{
			if (built)
				return this;

			built = true;

			if (string.IsNullOrWhiteSpace(DatabaseName))
				throw new ArgumentNullException(nameof(DatabaseName));

			return this;
		}

		public override string ToString()
		{
			return DatabaseName ?? "";
		}
	}
}

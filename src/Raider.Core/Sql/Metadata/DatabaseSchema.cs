using System;
using System.Collections.Generic;

namespace Raider.Sql.Metadata
{
	/// <summary>
	///     A simple model for a database schema.
	/// </summary>
	public class DatabaseSchema
	{
		/// <summary>
		///     The database that contains the schema.
		/// </summary>
		public DatabaseModel? Database { get; set; }

		/// <summary>
		/// <summary>
		///     The database schema identifier.
		/// </summary>
		public int? SchemaId { get; set; }

		/// <summary>
		///     The schema name.
		/// </summary>
		public string? Name { get; set; }

		/// <summary>
		///     The ordered list of tables in the schema.
		/// </summary>
		public IList<DatabaseTable> Tables { get; } = new List<DatabaseTable>();

		/// <summary>
		///     The ordered list of tables in the schema.
		/// </summary>
		public IList<DatabaseView> Views { get; } = new List<DatabaseView>();

		/// <summary>
		///     The ordered list of primary keys in the schema.
		/// </summary>
		public IList<DatabasePrimaryKey> PrimaryKeys { get; } = new List<DatabasePrimaryKey>();

		/// <summary>
		///     The ordered list of unique constraints in the schema.
		/// </summary>
		public IList<DatabaseUniqueConstraint> UniqueConstraints { get; } = new List<DatabaseUniqueConstraint>();

		/// <summary>
		///     The ordered list of foreign keys in the schema.
		/// </summary>
		public IList<DatabaseForeignKey> ForeignKeys { get; } = new List<DatabaseForeignKey>();

		private bool built = false;
		public DatabaseSchema BuildSchema()
		{
			if (built)
				return this;

			built = true;

			if (Database == null)
				throw new ArgumentNullException(nameof(Database));
			if (string.IsNullOrWhiteSpace(Name))
				throw new ArgumentNullException(nameof(Name));

			Database.Schemas.Add(this);

			return this;
		}

		public override string ToString()
		{
			return Name ?? "";
		}
	}
}

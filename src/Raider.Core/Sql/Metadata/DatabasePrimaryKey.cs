using System;
using System.Collections.Generic;

namespace Raider.Sql.Metadata
{
	/// <summary>
	///     A simple model for a database primary key.
	/// </summary>
	public class DatabasePrimaryKey
	{
		/// <summary>
		///     The schema in which is the primary key defined.
		/// </summary>
		public DatabaseSchema? Schema { get; set; }

		/// <summary>
		///     The table on which the primary key is defined.
		/// </summary>
		public DatabaseTable? Table { get; set; }

		/// <summary>
		///     The name of the primary key.
		/// </summary>
		public string? Name { get; set; }

		/// <summary>
		///     The ordered list of columns that make up the primary key.
		/// </summary>
		public IList<DatabaseColumn> Columns { get; } = new List<DatabaseColumn>();

		private bool built = false;
		public DatabasePrimaryKey BuildPrimaryKey()
		{
			if (built)
				return this;

			built = true;

			if (Schema == null)
				throw new ArgumentNullException(nameof(Schema));
			if (Table == null)
				throw new ArgumentNullException(nameof(Table));
			if (string.IsNullOrWhiteSpace(Name))
				throw new ArgumentNullException(nameof(Name));
			if (Columns.Count == 0)
				throw new ArgumentNullException(nameof(Columns));

			Schema.PrimaryKeys.Add(this);

			Table.PrimaryKey = this;

			foreach (var column in Columns)
				column.IsPrimaryKey = true;

			return this;
		}

		public override string ToString()
		{
			return Name ?? "";
		}
	}
}

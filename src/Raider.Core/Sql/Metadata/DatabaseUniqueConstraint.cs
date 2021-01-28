using System;
using System.Collections.Generic;

namespace Raider.Sql.Metadata
{
	/// <summary>
	///     A simple model for a database unique constraint.
	/// </summary>
	public class DatabaseUniqueConstraint
	{
		/// <summary>
		///     The schema in which is the unique constraint defined.
		/// </summary>
		public DatabaseSchema? Schema { get; set; }

		/// <summary>
		///     The table on which the unique constraint is defined.
		/// </summary>
		public DatabaseTable? Table { get; set; }

		/// <summary>
		///     The name of the constraint.
		/// </summary>
		public string? Name { get; set; }

		/// <summary>
		///     The ordered list of columns that make up the constraint.
		/// </summary>
		public IList<DatabaseColumn> Columns { get; } = new List<DatabaseColumn>();

		private bool built = false;
		public DatabaseUniqueConstraint BuildUniqueConstraint()
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

			Schema.UniqueConstraints.Add(this);

			Table.UniqueConstraints.Add(this);

			foreach (var column in Columns)
				column.IsUniqueConstraint = true;

			return this;
		}

		public override string ToString()
		{
			return Name ?? "";
		}
	}
}

using System;
using System.Collections.Generic;

namespace Raider.Sql.Metadata
{
	/// <summary>
	///     A simple model for a database foreign key.
	/// </summary>
	public class DatabaseForeignKey
	{
		/// <summary>
		///     The schema in which is the foreign key defined.
		/// </summary>
		public DatabaseSchema? Schema { get; set; }

		/// <summary>
		///     The table that contains the foreign key constraint.
		/// </summary>
		public DatabaseTable? Table { get; set; }

		/// <summary>
		///     The table to which the columns are constrained.
		/// </summary>
		public DatabaseTable? ForeignTable { get; set; }

		/// <summary>
		///     The ordered list of columns that are constrained.
		/// </summary>
		public IList<DatabaseColumn> Columns { get; } = new List<DatabaseColumn>();

		/// <summary>
		///     The ordered list of columns in the <see cref="ForeignTable" /> to which the <see cref="Columns" />
		///     of the foreign key are constrained.
		/// </summary>
		public IList<DatabaseColumn> ForeignColumns { get; } = new List<DatabaseColumn>();

		/// <summary>
		///     The foreign key constraint name.
		/// </summary>
		public string? Name { get; set; }

		public MatchOprions? MatchOption { get; set; }

		/// <summary>
		///     The action performed by the database when a row constrained by this foreign key
		///     is updated, or <c>null</c> if there is no action defined.
		/// </summary>
		public ReferentialAction? OnUpdateAction { get; set; }

		/// <summary>
		///     The action performed by the database when a row constrained by this foreign key
		///     is deleted, or <c>null</c> if there is no action defined.
		/// </summary>
		public ReferentialAction? OnDeleteAction { get; set; }

		private bool built = false;
		public DatabaseForeignKey BuildForeignKey()
		{
			if (built)
				return this;

			built = true;

			if (Schema == null)
				throw new ArgumentNullException(nameof(Schema));
			if (Table == null)
				throw new ArgumentNullException(nameof(Table));
			if (ForeignTable == null)
				throw new ArgumentNullException(nameof(ForeignTable));
			if (string.IsNullOrWhiteSpace(Name))
				throw new ArgumentNullException(nameof(Name));
			if (Columns.Count == 0)
				throw new ArgumentNullException(nameof(Columns));
			if (ForeignColumns.Count == 0)
				throw new ArgumentNullException(nameof(ForeignColumns));

			Schema.ForeignKeys.Add(this);

			Table.ForeignKeys.Add(this);

			foreach (var column in Columns)
				column.IsForeignKey = true;

			return this;
		}

		public override string ToString()
		{
			return Name ?? "";
		}
	}
}

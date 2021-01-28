using System;

namespace Raider.Sql.Metadata
{
	/// <summary>
	///     A simple model for a database object.
	/// </summary>
	public class DatabaseObject
	{
		/// <summary>
		///     The database object schema.
		/// </summary>
		public DatabaseSchema? Schema { get; set; }

		/// <summary>
		/// <summary>
		///     The database object identifier.
		/// </summary>
		public int? ObjectId { get; set; }

		/// <summary>
		///     The object name.
		/// </summary>
		public string? Name { get; set; }

		/// <summary>
		///     The database object type.
		/// </summary>
		public DatabaseObjectTypes ObjectType { get; set; }

		public DatabaseTable? ToTable()
		{
			if (ObjectType == DatabaseObjectTypes.Table)
				return (DatabaseTable)this;

			return null;
		}

		public DatabaseView? ToView()
		{
			if (ObjectType == DatabaseObjectTypes.View)
				return (DatabaseView)this;

			return null;
		}

		public DatabaseColumnObject? ToColumnObject()
		{
			var result = ToTable();
			if (result != null)
				return result;

			return ToView();
		}

		private bool built = false;
		protected DatabaseObject BuildDatabaseObject()
		{
			if (built)
				return this;

			built = true;

			if (Schema == null)
				throw new ArgumentNullException(nameof(Schema));
			if (string.IsNullOrWhiteSpace(Name))
				throw new ArgumentNullException(nameof(Name));

			return this;
		}

		public override string ToString()
		{
			return Name ?? "";
		}
	}
}

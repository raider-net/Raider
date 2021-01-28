using System.Collections.Generic;

namespace Raider.Sql.Metadata
{
	/// <summary>
	///     A simple model for a database table.
	/// </summary>
	public class DatabaseTable : DatabaseColumnObject
	{
		/// <summary>
		///     The primary key of the table.
		/// </summary>
		public DatabasePrimaryKey? PrimaryKey { get; set; }

		/// <summary>
		///     The list of unique constraints defined on the table.
		/// </summary>
		public IList<DatabaseUniqueConstraint> UniqueConstraints { get; } = new List<DatabaseUniqueConstraint>();

		/// <summary>
		///     The list of indexes defined on the table.
		/// </summary>
		public IList<DatabaseIndex> Indexes { get; } = new List<DatabaseIndex>();

		/// <summary>
		///     The list of foreign key constraints defined on the table.
		/// </summary>
		public IList<DatabaseForeignKey> ForeignKeys { get; } = new List<DatabaseForeignKey>();

		public DatabaseTable()
		{

		}

		public DatabaseTable(DatabaseObject obj)
		{
			Schema = obj.Schema;
			ObjectId = obj.ObjectId;
			Name = obj.Name;
			ObjectType = obj.ObjectType;
		}

		public DatabaseTable BuildTable()
		{
			BuildDatabaseObject();

			Schema.Tables.Add(this);

			return this;
		}
	}
}

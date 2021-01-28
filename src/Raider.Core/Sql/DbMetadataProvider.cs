using Raider.Sql.Metadata;
using System.Collections.Generic;

namespace Raider.Sql
{
	public abstract class DbMetadataProvider
	{
		public IReadOnlyList<DatabaseModel>? Databases { get; protected set; }
		public IReadOnlyList<DatabaseSchema>? Schemas { get; protected set; }
		public IReadOnlyList<DatabaseObject>? DbObjects { get; protected set; }
		public IReadOnlyList<DatabaseTable>? Tables { get; protected set; }
		public IReadOnlyList<DatabaseView>? Views { get; protected set; }
		public IReadOnlyList<DatabaseColumn>? Columns { get; protected set; }
		public IReadOnlyList<DatabasePrimaryKey>? PrimaryKeys { get; protected set; }
		public IReadOnlyList<DatabaseUniqueConstraint>? UniqueConstraints { get; protected set; }
		public IReadOnlyList<DatabaseForeignKey>? ForeignKeys { get; protected set; }

		protected abstract List<DatabaseModel>? GetAllDatabases();
		protected abstract List<DatabaseSchema>? GetAllSchemas();
		protected abstract List<DatabaseObject>? GetAllDatabaseObjects();
		protected abstract List<DatabaseColumn>? GetColumns();
		protected abstract List<DatabasePrimaryKey>? GetAllPrimaryKeys();
		protected abstract List<DatabaseUniqueConstraint>? GetAllUniqueConstraints();
		protected abstract List<DatabaseForeignKey>? GetAllForeignKeys();
		public abstract void LoadMetadata();
	}
}

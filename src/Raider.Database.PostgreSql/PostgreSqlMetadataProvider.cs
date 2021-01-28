using Npgsql;
using Raider.Data.Extensions;
using Raider.Enums;
using Raider.MathUtils;
using Raider.Sql;
using Raider.Sql.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Raider.Database.PostgreSql
{
	public class PostgreSqlMetadataProvider : DbMetadataProvider
	{
		/* POSTGRE SQL DATA TYPES:
		 
		SELECT pg_catalog.format_type(t.oid, NULL) AS "Name"
		FROM pg_catalog.pg_type t
		WHERE t.typrelid = 0
		  --AND NOT EXISTS(SELECT 1 FROM pg_catalog.pg_type el WHERE el.oid = t.typelem AND el.typarray = t.oid)
		  AND pg_catalog.pg_type_is_visible(t.oid)
		ORDER BY 1

			 */

		public string ConnectionString { get; }

		public PostgreSqlMetadataProvider(string connectionString)
		{
			if (string.IsNullOrWhiteSpace(connectionString))
				throw new ArgumentNullException(nameof(connectionString));

			ConnectionString = connectionString;
		}

		protected override List<DatabaseModel> GetAllDatabases()
		{
			string cmd = @"
				SELECT d.oid as ""DatabaseId"",
					d.datname as ""DatabaseName"",
					(pg_stat_file('base/' || d.oid || '/PG_VERSION')).creation as ""CreationDate"",
					d.datcollate as ""CollationName""
				FROM pg_database as d
				WHERE d.datistemplate = false
				";

			var result = new List<DatabaseModel>();
			using (NpgsqlConnection connection = new NpgsqlConnection(ConnectionString))
			{
				connection.Open();

				using (var command = new NpgsqlCommand(cmd, connection))
				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						result.Add(new DatabaseModel
						{
							ProviderType = DatabaseProviderType.PostrgeSql,
							DatabaseId = Convert.ToInt32(reader.GetValueOrNull<uint?>("DatabaseId")),
							DatabaseName = reader.GetValueOrDefault<string?>("DatabaseName"),
							CollationName = reader.GetValueOrDefault<string?>("CollationName"),
							CreationDate = (DateTime?)reader.GetValueOrNull<DateTime>("CreationDate"),
							DefaultSchema = "public"
						}
						.BuildDatabase());
					}
				}
			}
			return result;
		}

		protected override List<DatabaseSchema> GetAllSchemas()
		{
			string cmd = @"
				SELECT current_database() as ""DatabaseName"",
					oid as ""SchemaId"",
					nspname as ""SchemaName""
				FROM pg_namespace
				WHERE nspname NOT IN ('pg_toast', 'pg_temp_1', 'pg_toast_temp_1', 'pg_catalog', 'information_schema')
				";

			var result = new List<DatabaseSchema>();
			using (NpgsqlConnection connection = new NpgsqlConnection(ConnectionString))
			{
				connection.Open();

				using (var command = new NpgsqlCommand(cmd, connection))
				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						result.Add(new DatabaseSchema
						{
							Database = Databases?.FirstOrDefault(db => db.DatabaseName == reader.GetValueOrDefault<string>("DatabaseName")),
							SchemaId = Convert.ToInt32(reader.GetValueOrNull<uint>("SchemaId")),
							Name = reader.GetValueOrDefault<string>("SchemaName")
						}
						.BuildSchema());
					}
				}
			}
			return result;
		}

		private DatabaseObjectTypes ConvertToDatabaseObjectType(PostgreSqlObjectTypes pgType)
		{
			switch (pgType)
			{
				case PostgreSqlObjectTypes.r:
					return DatabaseObjectTypes.Table;
				case PostgreSqlObjectTypes.i:
					return DatabaseObjectTypes.Index;
				case PostgreSqlObjectTypes.S:
					return DatabaseObjectTypes.Sequence;
				case PostgreSqlObjectTypes.v:
					return DatabaseObjectTypes.View;
				case PostgreSqlObjectTypes.m:
					return DatabaseObjectTypes.MaterializedView;
				case PostgreSqlObjectTypes.t:
				case PostgreSqlObjectTypes.p:
				case PostgreSqlObjectTypes.c:
				case PostgreSqlObjectTypes.f:
				default:
					return DatabaseObjectTypes.UNDEFINED;
			}
		}

		protected override List<DatabaseObject> GetAllDatabaseObjects()
		{
			string cmd = @"
				SELECT cls.oid as ""ObjectId"",
					relname as ""ObjectName"",
					ns.oid as ""SchemaId"",
					relkind as ""DbType""
				FROM pg_class as cls
				INNER JOIN pg_namespace as ns ON cls.relnamespace = ns.oid
				WHERE ns.nspname NOT IN('pg_toast', 'pg_temp_1', 'pg_toast_temp_1', 'pg_catalog', 'information_schema')
				";

			var result = new List<DatabaseObject>();
			using (NpgsqlConnection connection = new NpgsqlConnection(ConnectionString))
			{
				connection.Open();

				using (var command = new NpgsqlCommand(cmd, connection))
				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						var dbObject = new DatabaseObject
						{
							Schema = Schemas?.FirstOrDefault(s => s.SchemaId == Convert.ToInt32(reader.GetValueOrNull<uint>("SchemaId"))),
							ObjectId = Convert.ToInt32(reader.GetValueOrNull<uint>("ObjectId")),
							Name = reader.GetValueOrDefault<string>("ObjectName"),
							ObjectType = ConvertToDatabaseObjectType(EnumHelper.ConvertStringToEnum<PostgreSqlObjectTypes>("" + reader.GetValueOrDefault<char>("DbType")))
						};

						if (dbObject.ObjectType == DatabaseObjectTypes.Table)
							result.Add(new DatabaseTable(dbObject));
						else if (dbObject.ObjectType == DatabaseObjectTypes.View)
							result.Add(new DatabaseView(dbObject));
						else
							result.Add(dbObject); //TODO: toto by nemalo nastat
					}
				}
			}
			return result;
		}

		public static Type StoreTypeToCsharpType(string storeType)
		{
			storeType = "_" + storeType
								.Replace(" ", "_")
								.Replace("\"", "")
								.Replace("[]", "_array");
			PostgreSqlDataTypes data_type = EnumHelper.ConvertStringToEnum<PostgreSqlDataTypes>(storeType, true);

			switch (data_type)
			{
				case PostgreSqlDataTypes._bytea:
					return typeof(byte[]);
				case PostgreSqlDataTypes._boolean:
					return typeof(bool);
				case PostgreSqlDataTypes._text:
					return typeof(string);
				case PostgreSqlDataTypes._numeric:
					return typeof(decimal);
				case PostgreSqlDataTypes._bigint:
					return typeof(long);
				case PostgreSqlDataTypes._jsonb:
					return typeof(string);
				case PostgreSqlDataTypes._smallint:
					return typeof(short);
				case PostgreSqlDataTypes._date:
					return typeof(DateTime);
				case PostgreSqlDataTypes._timestamp_without_time_zone:
					return typeof(DateTime);
				case PostgreSqlDataTypes._character:
					return typeof(string);
				case PostgreSqlDataTypes._character_varying:
					return typeof(string);
				case PostgreSqlDataTypes._integer:
					return typeof(int);
				case PostgreSqlDataTypes._xml:
					return typeof(string);
				case PostgreSqlDataTypes._uuid:
					return typeof(Guid);
				default:
					return null;
			}
		}

		protected override List<DatabaseColumn> GetColumns()
		{
			string cmd = @"
				SELECT
					table_catalog as ""DatabaseName"",
					table_schema as ""SchemaName"",
					table_name as ""TableName"",
					column_name as ""ColumnName"",
					ordinal_position as ""OrdinalPosition"",
					column_default as ""DefaultValue"",
					is_nullable as ""IsNullable"",
					data_type as ""DataType"",
					character_maximum_length as ""CharacterMaximumLength"",
					numeric_precision as ""Precision"",
					numeric_scale as ""Scale"",
					is_identity as ""IsIdentity"",
					identity_start as ""IdentityStart"",
					identity_increment as ""IdentityIncrement"",
					identity_maximum as ""LastIdentity"",
					is_generated as ""IsGenerated""
				FROM information_schema.""columns""
				WHERE table_schema NOT IN ('pg_toast', 'pg_temp_1', 'pg_toast_temp_1', 'pg_catalog', 'information_schema')
				";

			var result = new List<DatabaseColumn>();
			using (NpgsqlConnection connection = new NpgsqlConnection(ConnectionString))
			{
				connection.Open();

				using (var command = new NpgsqlCommand(cmd, connection))
				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						var storeType = reader.GetValueOrDefault<string>("DataType");
						result.Add(new DatabaseColumn
						{
							DatabaseColumnObject = DbObjects?.FirstOrDefault(s =>
															s.Schema?.Database?.DatabaseName == reader.GetValueOrDefault<string>("DatabaseName")
															&& s.Schema?.Name == reader.GetValueOrDefault<string>("SchemaName")
															&& s.Name == reader.GetValueOrDefault<string>("TableName"))
														?.ToColumnObject(),
							Name = reader.GetValueOrDefault<string>("ColumnName"),
							OrdinalPosition = reader.GetValueOrDefault<int>("OrdinalPosition"),
							DefaultValue = reader.GetValueOrDefault<string>("DefaultValue"),
							IsNullable = reader.GetValueOrDefault<string>("IsNullable")?.Equals("YES", StringComparison.OrdinalIgnoreCase) ?? false,
							StoreType = storeType,
							CsharpType = StoreTypeToCsharpType(storeType),
							CharacterMaximumLength = reader.GetValueOrDefault<int>("CharacterMaximumLength"),
							Precision = reader.GetValueOrDefault<int>("Precision"),
							Scale = reader.GetValueOrDefault<int>("Scale"),
							IsIdentity = reader.GetValueOrDefault<string>("IsIdentity")?.Equals("YES", StringComparison.OrdinalIgnoreCase) ?? false,
							IdentityStart = MathHelper.LongParseSafe(reader.GetValueOrDefault<string?>("IdentityStart")),
							IdentityIncrement = MathHelper.LongParseSafe(reader.GetValueOrDefault<string?>("IdentityIncrement")),
							LastIdentity = MathHelper.LongParseSafe(reader.GetValueOrDefault<string?>("LastIdentity")),
							ValueGenerated = EnumHelper.ConvertStringToEnum<ValueGenerated>(reader.GetValueOrDefault<string>("IsGenerated") ?? ValueGenerated.Never.ToString(), true)
						}
						.BuildColumn());
					}
				}
			}
			return result;
		}

		protected override List<DatabasePrimaryKey> GetAllPrimaryKeys()
		{
			string cmd = @"
				SELECT kc.*
				FROM information_schema.table_constraints tc
				JOIN information_schema.key_column_usage kc ON kc.table_name = tc.table_name AND kc.table_schema = tc.table_schema AND kc.constraint_name = tc.constraint_name
				WHERE tc.constraint_type = 'PRIMARY KEY'
				ORDER BY tc.table_schema,
					tc.table_name,
					tc.constraint_name,
					kc.ordinal_position,
					kc.position_in_unique_constraint
				";

			var result = new List<DatabasePrimaryKey>();
			using (NpgsqlConnection connection = new NpgsqlConnection(ConnectionString))
			{
				connection.Open();

				using (var command = new NpgsqlCommand(cmd, connection))
				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						var table = Tables?.FirstOrDefault(t => t.Schema.Name == reader.GetValueOrDefault<string>("table_schema")
																&& t.Name == reader.GetValueOrDefault<string>("table_name"));
						if (table == null)
							throw new Exception($"{nameof(table)} == null");

						var primaryKeyName = reader.GetValueOrDefault<string>("constraint_name");
						var primaryKeyExists = table?.PrimaryKey?.Name == primaryKeyName && !string.IsNullOrWhiteSpace(primaryKeyName);

						DatabasePrimaryKey primaryKey = primaryKeyExists
							? table.PrimaryKey
							: new DatabasePrimaryKey
							{
								Schema = Schemas?.FirstOrDefault(s => s.Name == reader.GetValueOrDefault<string>("constraint_schema")),
								Table = table,
								Name = primaryKeyName
							};

						var column = table.Columns.FirstOrDefault(c => c.Name == reader.GetValueOrDefault<string>("column_name"));
						column.PrimaryKeyOrdinalPosition = reader.GetValueOrDefault<int>("ordinal_position");
						primaryKey.Columns.Add(column);

						primaryKey.BuildPrimaryKey();

						if (!primaryKeyExists)
							result.Add(primaryKey);
					}
				}
			}
			return result;
		}

		protected override List<DatabaseUniqueConstraint> GetAllUniqueConstraints()
		{
			string cmd = @"
				SELECT kc.*
				FROM information_schema.table_constraints tc
				JOIN information_schema.key_column_usage kc ON kc.table_name = tc.table_name AND kc.table_schema = tc.table_schema AND kc.constraint_name = tc.constraint_name
				WHERE tc.constraint_type = 'UNIQUE'
				ORDER BY tc.table_schema,
					tc.table_name,
					tc.constraint_name,
					kc.ordinal_position,
					kc.position_in_unique_constraint
				";

			var result = new List<DatabaseUniqueConstraint>();
			using (NpgsqlConnection connection = new NpgsqlConnection(ConnectionString))
			{
				connection.Open();

				using (var command = new NpgsqlCommand(cmd, connection))
				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						var table = Tables?.FirstOrDefault(t => t.Schema.Name == reader.GetValueOrDefault<string>("table_schema")
																&& t.Name == reader.GetValueOrDefault<string>("table_name"));
						if (table == null)
							throw new Exception($"{nameof(table)} == null");

						var uniqueConstraintName = reader.GetValueOrDefault<string>("constraint_name");
						DatabaseUniqueConstraint uniqueConstraint = table?.UniqueConstraints?.FirstOrDefault(uq => uq.Name == uniqueConstraintName && !string.IsNullOrWhiteSpace(uniqueConstraintName));

						bool isNew = false;
						if (uniqueConstraint == null)
						{
							uniqueConstraint = new DatabaseUniqueConstraint
							{
								Schema = Schemas?.FirstOrDefault(s => s.Name == reader.GetValueOrDefault<string>("constraint_schema")),
								Table = table,
								Name = uniqueConstraintName
							};
							isNew = true;
						}

						uniqueConstraint.Columns.Add(table.Columns.FirstOrDefault(c => c.Name == reader.GetValueOrDefault<string>("column_name")));

						uniqueConstraint.BuildUniqueConstraint();

						if (isNew)
							result.Add(uniqueConstraint);
					}
				}
			}
			return result;
		}

		protected override List<DatabaseForeignKey> GetAllForeignKeys()
		{
			string cmd = @"
				SELECT kc.*,
					cc.table_schema AS foreign_table_schema,
					cc.table_name AS foreign_table_name,
					cc.column_name AS foreign_column_name,
					rc.match_option,
					rc.update_rule,
					rc.delete_rule
				FROM information_schema.table_constraints tc
				JOIN information_schema.key_column_usage kc ON kc.table_name = tc.table_name AND kc.table_schema = tc.table_schema AND kc.constraint_name = tc.constraint_name
				JOIN information_schema.constraint_column_usage cc ON cc.constraint_schema = tc.constraint_schema AND cc.constraint_name = tc.constraint_name
				JOIN information_schema.referential_constraints rc ON rc.constraint_schema = tc.constraint_schema AND rc.constraint_name = tc.constraint_name
				WHERE tc.constraint_type = 'FOREIGN KEY'
				ORDER BY tc.table_schema,
					tc.table_name,
					tc.constraint_name,
					kc.ordinal_position,
					kc.position_in_unique_constraint
				";

			var result = new List<DatabaseForeignKey>();
			using (NpgsqlConnection connection = new NpgsqlConnection(ConnectionString))
			{
				connection.Open();

				using (var command = new NpgsqlCommand(cmd, connection))
				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						var table = Tables?.FirstOrDefault(t => t.Schema.Name == reader.GetValueOrDefault<string>("table_schema")
																&& t.Name == reader.GetValueOrDefault<string>("table_name"));
						if (table == null)
							throw new Exception($"{nameof(table)} == null");

						var foreignTable = Tables?.FirstOrDefault(t => t.Schema.Name == reader.GetValueOrDefault<string>("foreign_table_schema")
																&& t.Name == reader.GetValueOrDefault<string>("foreign_table_name"));
						if (foreignTable == null)
							throw new Exception($"{nameof(foreignTable)} == null");

						var foreignKey = new DatabaseForeignKey
						{
							Schema = Schemas?.FirstOrDefault(s => s.Name == reader.GetValueOrDefault<string>("constraint_schema")),
							Table = table,
							ForeignTable = foreignTable,
							Name = reader.GetValueOrDefault<string>("constraint_name"),
							MatchOption = EnumHelper.ConvertStringToEnum<MatchOprions>(reader.GetValueOrDefault<string>("match_option")?.Replace(" ", ""), true),
							OnUpdateAction = EnumHelper.ConvertStringToEnum<ReferentialAction>(reader.GetValueOrDefault<string>("update_rule")?.Replace(" ", ""), true),
							OnDeleteAction = EnumHelper.ConvertStringToEnum<ReferentialAction>(reader.GetValueOrDefault<string>("delete_rule")?.Replace(" ", ""), true)
						};

						foreignKey.Columns.Add(table.Columns.FirstOrDefault(c => c.Name == reader.GetValueOrDefault<string>("column_name")));
						foreignKey.ForeignColumns.Add(foreignTable.Columns.FirstOrDefault(c => c.Name == reader.GetValueOrDefault<string>("foreign_column_name")));

						result.Add(foreignKey.BuildForeignKey());
					}
				}
			}
			return result;
		}

		public override void LoadMetadata()
		{
			Databases = GetAllDatabases();
			Schemas = GetAllSchemas();
			DbObjects = GetAllDatabaseObjects();

			Tables = DbObjects.Where(obj => obj.ObjectType == DatabaseObjectTypes.Table).Select(obj => obj.ToTable().BuildTable()).ToList();
			Views = DbObjects.Where(obj => obj.ObjectType == DatabaseObjectTypes.View).Select(obj => obj.ToView().BuildView()).ToList();

			Columns = GetColumns();
			PrimaryKeys = GetAllPrimaryKeys();
			UniqueConstraints = GetAllUniqueConstraints();
			ForeignKeys = GetAllForeignKeys();
		}
	}
}

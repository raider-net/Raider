using NpgsqlTypes;
using System;
using System.Collections.Generic;

namespace Raider.Database.PostgreSql
{
	public class DictionaryTableOptions
	{
		public string? SchemaName { get; set; }
		public string? TableName { get; set; }
		public List<string>? PropertyNames { get; set; }
		public Dictionary<string, string>? PropertyColumnMapping { get; set; }
		public Dictionary<string, NpgsqlDbType>? PropertyTypeMapping { get; set; }
		public Dictionary<string, Func<object?, object?>>? PropertyValueConverter { get; set; }
		public bool UseQuotationMarksForTableName { get; set; } = true;
		public bool UseQuotationMarksForColumnNames { get; set; } = true;
		public bool PropertyTypeMappingIsRequired { get; set; }

		public DictionaryTableOptions Validate(bool validateProperties, bool validatePropertyMapping)
		{
			if (string.IsNullOrWhiteSpace(SchemaName))
				throw new ArgumentNullException(nameof(SchemaName));

			if (string.IsNullOrWhiteSpace(TableName))
				throw new ArgumentNullException(nameof(TableName));

			if (validateProperties && (PropertyNames == null || PropertyNames.Count == 0))
				throw new ArgumentNullException(nameof(PropertyNames));

			if (PropertyNames != null)
				foreach (var propertyName in PropertyNames)
					if (string.IsNullOrWhiteSpace(propertyName))
						throw new ArgumentException("NULL column name is not valid.", nameof(PropertyNames));

			if (PropertyColumnMapping != null)
				foreach (var kvp in PropertyColumnMapping)
					if (string.IsNullOrWhiteSpace(kvp.Value))
						throw new ArgumentException($"Property {kvp.Key} has NULL column mapping.", nameof(PropertyColumnMapping));

			if (validatePropertyMapping && (PropertyTypeMapping == null || PropertyTypeMapping.Count == 0))
				throw new ArgumentNullException(nameof(PropertyTypeMapping));

			return this;
		}
	}
}

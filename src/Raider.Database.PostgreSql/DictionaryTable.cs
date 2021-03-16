using Npgsql;
using NpgsqlTypes;
using Raider.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Raider.Database.PostgreSql
{
	public class DictionaryTable
	{
		public string SchemaName { get; }
		public string TableName { get; }
		public IReadOnlyList<string> PropertyNames { get; }
		public IReadOnlyDictionary<string, int> PropertyIndex { get; }
		public IReadOnlyDictionary<string, string> PropertyColumnMapping { get; }
		public IReadOnlyDictionary<string, NpgsqlDbType> PropertyTypeMapping { get; }
		public IReadOnlyDictionary<string, Func<object?, object?>>? PropertyValueConverter { get; }
		public bool UseQuotationMarksForTableName { get; }
		public bool UseQuotationMarksForColumnNames { get; }

		public IReadOnlyList<string> ColumnNames { get; }
		public IReadOnlyDictionary<string, NpgsqlDbType>? ColumnTypes { get; }


		public DictionaryTable(DictionaryTableOptions options)
			: this(options, false)
		{
		}

		protected DictionaryTable(DictionaryTableOptions options, bool propertyTypeMappingIsRequired)
		{
			if (options == null)
				throw new ArgumentNullException(nameof(options));

			if (propertyTypeMappingIsRequired)
				options.PropertyTypeMappingIsRequired = true;

			options.Validate(true, false);

			SchemaName = options.SchemaName ?? "";
			TableName = options.TableName ?? "";
			PropertyNames = options.PropertyNames?.ToList() ?? throw new InvalidOperationException($"{nameof(options)}.{nameof(options.PropertyNames)} == null");
			var pcMapping = options.PropertyColumnMapping?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
			PropertyTypeMapping = options.PropertyTypeMapping?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value) ?? new Dictionary<string, NpgsqlDbType>();
			PropertyValueConverter = options.PropertyValueConverter?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
			UseQuotationMarksForTableName = options.UseQuotationMarksForTableName;
			UseQuotationMarksForColumnNames = options.UseQuotationMarksForColumnNames;

			var idx = 1;
			PropertyIndex = PropertyNames.Select(x => new { PropertyName = x, Index = idx++ }).ToDictionary(k => k.PropertyName, v => v.Index);

			if (pcMapping == null || pcMapping.Count == 0)
			{
				ColumnNames = PropertyNames;
				PropertyColumnMapping = PropertyNames.ToDictionary(k => k, v => v);

				if (options.PropertyTypeMappingIsRequired)
					ColumnTypes = PropertyNames.ToDictionary(p => p, p => ConvertType(p));
			}
			else
			{
				var columnNames = new List<string>();
				var columnTypes = new Dictionary<string, NpgsqlDbType>();

				foreach (var propertyName in PropertyNames)
				{
					if (pcMapping.TryGetValue(propertyName, out string? columnName))
						columnNames.Add(columnName);
					else
					{
						columnNames.Add(propertyName);
						pcMapping.Add(propertyName, propertyName);
					}

					if (options.PropertyTypeMappingIsRequired)
						columnTypes.Add(propertyName, ConvertType(propertyName));
				}

				PropertyColumnMapping = pcMapping;
				ColumnNames = columnNames;

				if (options.PropertyTypeMappingIsRequired)
					ColumnTypes = columnTypes;
			}
		}

		public string GetQualifiedTableName()
		{
			if (UseQuotationMarksForTableName)
				return $"\"{TableName}\"";
			else
				return TableName;
		}

		public string GetQualifiedColumnName(string columnName)
		{
			if (UseQuotationMarksForColumnNames)
				return $"\"{columnName}\"";
			else
				return columnName;
		}

		public string GetColumns()
		{
			string columns;
			if (UseQuotationMarksForColumnNames)
				columns = $"\"{string.Join("\", \"", ColumnNames)}\"";
			else
				columns = string.Join(", ", ColumnNames);

			return columns;
		}

		public string PropertyNameToColumnName(string propertyName)
			=> PropertyColumnMapping.TryGetValue(propertyName, out string? columnName)
				? columnName
				: throw new ArgumentException($"PropertyName {propertyName} is not a valid property mapped to any column.", nameof(propertyName));

		public string GetColumns(List<string>? propertyNames)
		{
			if (propertyNames == null || propertyNames.Count == 0)
				return GetColumns();

			var columnNames = propertyNames.Select(x => PropertyNameToColumnName(x));

			string columns;
			if (UseQuotationMarksForColumnNames)
				columns = $"\"{string.Join("\", \"", columnNames)}\"";
			else
				columns = string.Join(", ", columnNames);

			return columns;
		}

		public static string GetParameterName(int index)
			=> $"@p{index}";

		public string GetColumnSetters(List<string>? propertyNames)
			=> (propertyNames == null || propertyNames.Count == 0)
			? string.Join(", ", PropertyNames.Select(propertyName => $"{GetQualifiedColumnName(propertyName)}={GetParameterName(PropertyIndex[propertyName])}"))
			: string.Join(", ", propertyNames.Select(propertyName => PropertyColumnMapping.TryGetValue(propertyName, out _) ? $"{GetQualifiedColumnName(propertyName)}={GetParameterName(PropertyIndex[propertyName])}" : throw new ArgumentException($"PropertyName {propertyName} is not a valid property mapped to any column.", nameof(propertyNames))));

		public string GetColumnParameters(List<string>? propertyNames)
			=> (propertyNames == null || propertyNames.Count == 0)
			? $"{string.Join(", ", PropertyNames.Select(propertyName => GetParameterName(PropertyIndex[propertyName])))}"
			: $"{string.Join(", ", propertyNames.Select(propertyName => PropertyColumnMapping.TryGetValue(propertyName, out _) ? GetParameterName(PropertyIndex[propertyName]) : throw new ArgumentException($"PropertyName {propertyName} is not a valid property mapped to any column.", nameof(propertyNames))))}";

		public string ToInsertSql(string? returnningColumnName = null, List<string>? propertyNames = null)
			=> $"INSERT INTO {SchemaName}.{(UseQuotationMarksForTableName ? "\"" : "")}{TableName}{(UseQuotationMarksForTableName ? "\"" : "")} ({GetColumns(propertyNames)})  VALUES({GetColumnParameters(propertyNames)}){(string.IsNullOrWhiteSpace(returnningColumnName) ? "" : $"RETURNING {returnningColumnName}")}";

		public string ToUpdateSql(List<string>? propertyNames = null, string ? where = null)
			=> $"UPDATE {SchemaName}.{(UseQuotationMarksForTableName ? "\"" : "")}{TableName}{(UseQuotationMarksForTableName ? "\"" : "")} SET {GetColumnSetters(propertyNames)}{(string.IsNullOrWhiteSpace(where) ? "" : $" WHERE {where}")}";

		public string ToCopySql()
			=> $"COPY {SchemaName}.{(UseQuotationMarksForTableName ? "\"" : "")}{TableName}{(UseQuotationMarksForTableName ? "\"" : "")} ({GetColumns()}) FROM STDIN (FORMAT BINARY)";

		public void SetParameters(NpgsqlCommand command, IDictionary<string, object?> data)
		{
			if (command == null)
				throw new ArgumentNullException(nameof(command));

			if (data == null)
				throw new ArgumentNullException(nameof(data));

			foreach (var propertyName in PropertyNames)
			{
				if (data.TryGetValue(propertyName, out object? value))
				{
					if (PropertyValueConverter != null && PropertyValueConverter.TryGetValue(propertyName, out Func<object?, object?>? converter))
						value = converter(value);
				}

				command.Parameters.AddWithValue(GetParameterName(PropertyIndex[propertyName]), value ?? DBNull.Value);
			}

			foreach (var kvp in data)
				if (kvp.Key.StartsWith("@"))
					command.Parameters.AddWithValue(kvp.Key, kvp.Value ?? DBNull.Value);
		}

		public NpgsqlDbType ConvertType(string memberName)
		{
			if (PropertyTypeMapping.TryGetValue(memberName, out NpgsqlDbType result))
				return result;
			else
				throw new InvalidOperationException($"Property '{memberName}' has no type defined in options.{nameof(DictionaryTableOptions.PropertyTypeMapping)}");
		}
	}
}

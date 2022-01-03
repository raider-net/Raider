using Npgsql;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Raider.Database.PostgreSql
{
	public class BulkInsert : DictionaryTable, IDisposable
	{
		private readonly NpgsqlConnection? _connection;
		private readonly bool _isInternalConnection;
		private bool _internalConnectionIsOpened;

		public BulkInsert(DictionaryTableOptions options)
			: this(options, (NpgsqlConnection?)null)
		{
		}

		public BulkInsert(DictionaryTableOptions options, string connectionString)
			: this(options, (NpgsqlConnection?)null)
		{
			_connection = new NpgsqlConnection(connectionString);
			_isInternalConnection = true;
		}

		public BulkInsert(DictionaryTableOptions options, NpgsqlConnection? connection)
			: base(options, true)
		{
			if (options == null)
				throw new ArgumentNullException(nameof(options));

			options.Validate(true, true);

			_connection = connection;
			_isInternalConnection = false;
		}

		[DebuggerHidden]
		[DebuggerStepThrough]
		public async Task<ulong> WriteBatchAsync(IEnumerable<IDictionary<string, object?>?>? rows, string connectionString, CancellationToken cancellationToken = default)
		{
			if (rows == null || !rows.Any())
				return (ulong)0;

			if (string.IsNullOrWhiteSpace(connectionString))
				throw new ArgumentNullException(nameof(connectionString));

			await using var connection = new NpgsqlConnection(connectionString);
			await connection.OpenAsync(cancellationToken);
			return await WriteBatchAsync(rows, connection, false, false, cancellationToken);
		}

		[DebuggerHidden]
		[DebuggerStepThrough]
		public Task<ulong> WriteBatchAsync(IEnumerable<IDictionary<string, object?>> rows, bool openConnection = false, bool disposeConnection = false, CancellationToken cancellationToken = default)
		{
			if (rows == null || !rows.Any())
				return Task.FromResult((ulong)0);

			if (_connection == null)
				throw new InvalidOperationException("No DB connection was defined");

			return WriteBatchAsync(rows, _connection, openConnection, disposeConnection, cancellationToken);
		}

		[DebuggerHidden]
		[DebuggerStepThrough]
		public async Task<ulong> WriteBatchAsync(IEnumerable<IDictionary<string, object?>?> rows, NpgsqlConnection connection, bool openConnection = false, bool disposeConnection = false, CancellationToken cancellationToken = default)
		{
			ulong result = 0;

			if (rows == null || !rows.Any())
				return result;

			if (connection == null)
				throw new ArgumentNullException(nameof(connection));

			if (ColumnTypes == null)
				throw new InvalidOperationException($"{nameof(ColumnTypes)} == null");

			if (openConnection || (_isInternalConnection && !_internalConnectionIsOpened))
			{
				await connection.OpenAsync(cancellationToken);
				_internalConnectionIsOpened = true;
			}

			using (var writer = connection.BeginBinaryImport(ToCopySql()))
			{
				foreach (var row in rows)
				{
					await writer.StartRowAsync(cancellationToken);

					foreach (var propertyName in PropertyNames)
					{
						if (row != null && row.TryGetValue(propertyName, out object? value))
						{
							if (PropertyValueConverter != null && PropertyValueConverter.TryGetValue(propertyName, out Func<object?, object?>? converter))
								value = converter(value);
						}
						else
						{
							value = null;
						}

						await writer.WriteAsync(value, ColumnTypes[propertyName], cancellationToken);
					}
				}

				result = await writer.CompleteAsync(cancellationToken);
			}

			if (disposeConnection)
				await connection.DisposeAsync();

			return result;
		}

		private bool disposed;
		protected virtual void Dispose(bool disposing)
		{
			if (!disposed)
			{
				if (disposing)
				{
					if (_isInternalConnection)
						_connection?.Dispose();
				}

				disposed = true;
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
	}






	/*
	 USAGE:
			var now = DateTime.Now;
			var listErr = new List<Error>();
			for (int i = 0; i < 10000; i++)
			{
				listErr.Add(new Error
				{
					Created = now.AddSeconds(5),
					IdErrorSource = 1,
					IdErrorSeverity = 1,
					IdUser = 2,
					Message = "sjdlfjslk s klhs lskjdhf lksdl fkhsl dhlfsk x",
					Detail = "abababa ab ba ab ba   ab ab ab ab ba ab ab",
					CorrelationId = "skldjfhskldjhf-klshd",
					Permission = "tokeeen1"
				});
			}

			using (var bi = new BulkInsert<Error>(
				new DictionaryTableOptions
				{
					SchemaName = "aud",
					TableName = "Error",
					PropertyNames = new List<string>
					{
						//nameof(Error.IdError),
						nameof(Error.Created),
						nameof(Error.IdErrorSource),
						nameof(Error.IdErrorSeverity),
						nameof(Error.IdUser),
						nameof(Error.Message),
						nameof(Error.Detail),
						nameof(Error.IdOperationEntry),
						nameof(Error.CorrelationId),
						nameof(Error.Permission),
					}
				}))
			{
				await bi.WriteBatch(listErr, "Host=localhost;Database=database;Username=name;Password=pwd");
			}
	 */

	//public class BulkInsert<T> : IDisposable
	//	where T : class
	//{
	//	private readonly ObjectWrapper _objectWrapper;
	//	private readonly string _schemaName;
	//	private readonly string _tableName;
	//	private readonly List<string> _propertyNames;
	//	private readonly Dictionary<string, string> _propertyColumnMapping;
	//	private readonly Dictionary<string, NpgsqlDbType> _propertyTypeMapping;
	//	private readonly Dictionary<string, Func<object, object>> _propertyValueConverter;
	//	private readonly bool _useQuotationMarksForTableName;
	//	private readonly bool _useQuotationMarksForColumnNames;

	//	private readonly List<string> _columnNames;
	//	private readonly Dictionary<string, NpgsqlDbType> _columnTypes;

	//	private readonly NpgsqlConnection _connection;
	//	private readonly bool _isInternalConnection;
	//	private bool _internalConnectionIsOpened;

	//	public BulkInsert(DictionaryTableOptions options)
	//		: this(options, (NpgsqlConnection)null)
	//	{
	//	}

	//	public BulkInsert(DictionaryTableOptions options, string connectionString)
	//		: this(options, (NpgsqlConnection)null)
	//	{
	//		_connection = new NpgsqlConnection(connectionString);
	//		_isInternalConnection = true;
	//	}

	//	public BulkInsert(DictionaryTableOptions options, NpgsqlConnection connection)
	//	{
	//		if (options == null)
	//			throw new ArgumentNullException(nameof(options));

	//		options.Validate(validateProperties: false);

	//		_objectWrapper = ObjectWrapper.Create<T>(includeAllBaseTypes: true);
	//		_schemaName = options.SchemaName;
	//		_tableName = options.TableName;
	//		_propertyNames = options.PropertyNames?.ToList();
	//		_propertyColumnMapping = options.PropertyColumnMapping?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
	//		_propertyTypeMapping = options.PropertyTypeMapping?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
	//		_propertyValueConverter = options.PropertyValueConverter?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
	//		_useQuotationMarksForTableName = options.UseQuotationMarksForTableName;
	//		_useQuotationMarksForColumnNames = options.UseQuotationMarksForColumnNames;

	//		if (_propertyNames == null)
	//			_propertyNames = _objectWrapper.GetAllPropertyNames();

	//		if (_propertyColumnMapping == null || _propertyColumnMapping.Count == 0)
	//		{
	//			_columnNames = _propertyNames;
	//			_columnTypes = _propertyNames.ToDictionary(p => p, p => ConvertType(p));
	//		}
	//		else
	//		{
	//			_columnNames = new List<string>();
	//			_columnTypes = new Dictionary<string, NpgsqlDbType>();

	//			foreach (var propertyName in _propertyNames)
	//			{
	//				if (_propertyColumnMapping.TryGetValue(propertyName, out string? columnName))
	//					_columnNames.Add(columnName);
	//				else
	//					_columnNames.Add(propertyName);

	//				_columnTypes.Add(propertyName, ConvertType(propertyName));
	//			}
	//		}

	//		_connection = connection;
	//		_isInternalConnection = false;
	//	}

	//	public async Task WriteBatch(IEnumerable<T> rows, string connectionString)
	//	{
	//		if (rows == null || rows.Count() == 0)
	//			return;

	//		await using (var connection = new NpgsqlConnection(connectionString))
	//		{
	//			await connection.OpenAsync();
	//			await WriteBatch(rows, connection, false, false);
	//		}
	//	}

	//	public Task WriteBatch(IEnumerable<T> rows, bool openConnection = false, bool disposeConnection = false)
	//	{
	//		if (rows == null || rows.Count() == 0)
	//			return Task.CompletedTask;

	//		if (_connection == null)
	//			throw new InvalidOperationException("No DB connection was defined");

	//		return WriteBatch(rows, _connection, openConnection, disposeConnection);
	//	}

	//	public async Task WriteBatch(IEnumerable<T> rows, NpgsqlConnection connection, bool openConnection = false, bool disposeConnection = false)
	//	{
	//		if (rows == null || rows.Count() == 0)
	//			return;

	//		if (connection == null)
	//			throw new ArgumentNullException(nameof(connection));

	//		if (openConnection || (_isInternalConnection && !_internalConnectionIsOpened))
	//		{
	//			await connection.OpenAsync();
	//			_internalConnectionIsOpened = true;
	//		}

	//		string columns;
	//		if (_useQuotationMarksForColumnNames)
	//			columns = $"\"{string.Join("\", \"", _columnNames)}\"";
	//		else
	//			columns = string.Join(", ", _columnNames);

	//		var copyCommand = $"COPY {_schemaName}.{(_useQuotationMarksForTableName ? "\"" : "")}{_tableName}{(_useQuotationMarksForTableName ? "\"" : "")} ({columns}) FROM STDIN (FORMAT BINARY)";
	//		using (var writer = connection.BeginBinaryImport(copyCommand))
	//		{
	//			foreach (var row in rows)
	//			{
	//				await writer.StartRowAsync();

	//				_objectWrapper.SetTargetInstance(row);
	//				foreach (var propertyName in _propertyNames)
	//				{
	//					var value = _objectWrapper[propertyName];

	//					if (_propertyValueConverter != null && _propertyValueConverter.TryGetValue(propertyName, out Func<object, object>? converter))
	//						value = converter(value);

	//					await writer.WriteAsync(value, _columnTypes[propertyName]);
	//				}
	//			}

	//			await writer.CompleteAsync();
	//		}

	//		if (disposeConnection)
	//			connection.Dispose();
	//	}

	//	private NpgsqlDbType ConvertType(string memberName)
	//	{
	//		if (_propertyTypeMapping != null && _propertyTypeMapping.TryGetValue(memberName, out NpgsqlDbType result))
	//			return result;

	//		var csharpType = _objectWrapper.GetMemberType(memberName);
	//		if (csharpType == null)
	//			throw new ArgumentException($"Invalid property name {memberName}", nameof(DictionaryTableOptions.PropertyNames));

	//		var underlyingType = csharpType.GetUnderlyingNullableType();

	//		if (underlyingType == typeof(long))
	//			return NpgsqlDbType.Bigint;

	//		else if (underlyingType == typeof(bool))
	//			return NpgsqlDbType.Boolean;

	//		else if (underlyingType == typeof(byte) || underlyingType == typeof(byte[]))
	//			return NpgsqlDbType.Bytea;

	//		else if (underlyingType == typeof(double))
	//			return NpgsqlDbType.Double;

	//		else if (underlyingType == typeof(int))
	//			return NpgsqlDbType.Integer;

	//		else if (underlyingType == typeof(decimal))
	//			return NpgsqlDbType.Numeric;

	//		else if (underlyingType == typeof(float))
	//			return NpgsqlDbType.Real;

	//		else if (underlyingType == typeof(short))
	//			return NpgsqlDbType.Smallint;

	//		else if (underlyingType == typeof(string))
	//			return NpgsqlDbType.Varchar;

	//		else if (underlyingType == typeof(DateTime))
	//			return NpgsqlDbType.Timestamp;

	//		else if (underlyingType == typeof(DateTimeOffset))
	//			return NpgsqlDbType.TimestampTz;

	//		else if (underlyingType == typeof(Guid))
	//			return NpgsqlDbType.Uuid;

	//		throw new ArgumentException($"Invalid property type {underlyingType.FullName} for {memberName}", nameof(DictionaryTableOptions.PropertyNames));
	//	}

	//	public void Dispose()
	//	{
	//		if (_isInternalConnection)
	//			_connection?.Dispose();
	//	}
	//}
}

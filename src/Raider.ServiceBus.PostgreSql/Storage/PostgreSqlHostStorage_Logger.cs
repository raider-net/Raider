using Microsoft.Extensions.Logging;
using Npgsql;
using Raider.Infrastructure;
using Raider.Logging;
using Raider.ServiceBus.BusLogger;
using Raider.ServiceBus.PostgreSql.Messages.Storage.Model;
using Raider.Trace;
using Raider.Transactions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Raider.ServiceBus.PostgreSql.Storage
{
	internal abstract partial class PostgreSqlHostStorage : IHostLogger
	{
		public void LogTrace(
			ITraceInfo traceInfo,
			Guid idHost,
			HostStatus hostStatus,
			Action<LogMessageBuilder> messageBuilder,
			string? detail = null,
			ITransactionContext? transactionContext = null)
		{
			try
			{
				bool isLocalTransaction = true;
				if (transactionContext == null)
					transactionContext = CreateTransactionContext();
				else
					isLocalTransaction = false;

				var connection = transactionContext.GetItem<NpgsqlConnection>(nameof(NpgsqlConnection));
				var transaction = transactionContext.GetItemIfExists<NpgsqlTransaction>(nameof(NpgsqlTransaction));

				var builder = new LogMessageBuilder(traceInfo).LogLevel(LogLevel.Trace);
				messageBuilder?.Invoke(builder);
				var message = builder.Build();

				var dbHostLog = new DbHostLog
				{
					IdHost = idHost,
					IdLogLevel = (int)LogLevel.Trace,
					RuntimeUniqueKey = EnvironmentInfo.RUNTIME_UNIQUE_KEY,
					TimeCreatedUtc = DateTime.UtcNow,
					IdHostStatus = (int)hostStatus,
					LogMessage = message,
					LogDetail = detail
				};

				var sql = DbHostLog.GetInsertSql(_options);

				using var cmd = new NpgsqlCommand(sql, connection);
				if (transaction != null)
					cmd.Transaction = transaction;

				var table = DbHostLog.GetDictionaryTable(_options);
				table.SetParameters(cmd, dbHostLog.ToDictionary(_serialzier));

				var result = cmd.ExecuteNonQuery();

				if (result != 1)
					throw new InvalidOperationException($"{nameof(LogTrace)}: {nameof(cmd.ExecuteNonQuery)} returns {result} | {nameof(idHost)} = {idHost}");

				if (hostStatus != HostStatus.Unchanged)
				{
					try
					{
						UpdateHost(idHost, hostStatus, null, transactionContext);
					}
					catch (Exception ex)
					{
						_baseHostLogger.LogError(traceInfo, idHost, hostStatus, x => x.ExceptionInfo(ex), detail, transactionContext);
					}
				}

				if (isLocalTransaction)
					transactionContext.Commit();
			}
			catch (Exception ex)
			{
				_baseHostLogger.LogTrace(traceInfo, idHost, hostStatus, messageBuilder, detail, transactionContext);
				_baseHostLogger.LogCritical(traceInfo, idHost, hostStatus, x => x.ExceptionInfo(ex), detail, transactionContext);
				throw;
			}
		}

		public void LogDebug(
			ITraceInfo traceInfo,
			Guid idHost,
			HostStatus hostStatus,
			Action<LogMessageBuilder> messageBuilder,
			string? detail = null,
			ITransactionContext? transactionContext = null)
		{
			try
			{
				bool isLocalTransaction = true;
				if (transactionContext == null)
					transactionContext = CreateTransactionContext();
				else
					isLocalTransaction = false;

				var connection = transactionContext.GetItem<NpgsqlConnection>(nameof(NpgsqlConnection));
				var transaction = transactionContext.GetItemIfExists<NpgsqlTransaction>(nameof(NpgsqlTransaction));

				var builder = new LogMessageBuilder(traceInfo).LogLevel(LogLevel.Debug);
				messageBuilder?.Invoke(builder);
				var message = builder.Build();

				var dbHostLog = new DbHostLog
				{
					IdHost = idHost,
					IdLogLevel = (int)LogLevel.Debug,
					RuntimeUniqueKey = EnvironmentInfo.RUNTIME_UNIQUE_KEY,
					TimeCreatedUtc = DateTime.UtcNow,
					IdHostStatus = (int)hostStatus,
					LogMessage = message,
					LogDetail = detail
				};

				var sql = DbHostLog.GetInsertSql(_options);

				using var cmd = new NpgsqlCommand(sql, connection);
				if (transaction != null)
					cmd.Transaction = transaction;

				var table = DbHostLog.GetDictionaryTable(_options);
				table.SetParameters(cmd, dbHostLog.ToDictionary(_serialzier));

				var result = cmd.ExecuteNonQuery();

				if (result != 1)
					throw new InvalidOperationException($"{nameof(LogDebug)}: {nameof(cmd.ExecuteNonQuery)} returns {result} | {nameof(idHost)} = {idHost}");

				if (hostStatus != HostStatus.Unchanged)
				{
					try
					{
						UpdateHost(idHost, hostStatus, null, transactionContext);
					}
					catch (Exception ex)
					{
						_baseHostLogger.LogError(traceInfo, idHost, hostStatus, x => x.ExceptionInfo(ex), detail, transactionContext);
					}
				}

				if (isLocalTransaction)
					transactionContext.Commit();
			}
			catch (Exception ex)
			{
				_baseHostLogger.LogDebug(traceInfo, idHost, hostStatus, messageBuilder, detail, transactionContext);
				_baseHostLogger.LogCritical(traceInfo, idHost, hostStatus, x => x.ExceptionInfo(ex), detail, transactionContext);
				throw;
			}
		}

		public void LogInformation(
			ITraceInfo traceInfo,
			Guid idHost,
			HostStatus hostStatus,
			Action<LogMessageBuilder> messageBuilder,
			string? detail = null,
			ITransactionContext? transactionContext = null)
		{
			try
			{
				bool isLocalTransaction = true;
				if (transactionContext == null)
					transactionContext = CreateTransactionContext();
				else
					isLocalTransaction = false;

				var connection = transactionContext.GetItem<NpgsqlConnection>(nameof(NpgsqlConnection));
				var transaction = transactionContext.GetItemIfExists<NpgsqlTransaction>(nameof(NpgsqlTransaction));

				var builder = new LogMessageBuilder(traceInfo).LogLevel(LogLevel.Information);
				messageBuilder?.Invoke(builder);
				var message = builder.Build();

				var dbHostLog = new DbHostLog
				{
					IdHost = idHost,
					IdLogLevel = (int)LogLevel.Information,
					RuntimeUniqueKey = EnvironmentInfo.RUNTIME_UNIQUE_KEY,
					TimeCreatedUtc = DateTime.UtcNow,
					IdHostStatus = (int)hostStatus,
					LogMessage = message,
					LogDetail = detail
				};

				var sql = DbHostLog.GetInsertSql(_options);

				using var cmd = new NpgsqlCommand(sql, connection);
				if (transaction != null)
					cmd.Transaction = transaction;

				var table = DbHostLog.GetDictionaryTable(_options);
				table.SetParameters(cmd, dbHostLog.ToDictionary(_serialzier));

				var result = cmd.ExecuteNonQuery();

				if (result != 1)
					throw new InvalidOperationException($"{nameof(LogInformation)}: {nameof(cmd.ExecuteNonQuery)} returns {result} | {nameof(idHost)} = {idHost}");

				if (hostStatus != HostStatus.Unchanged)
				{
					try
					{
						UpdateHost(idHost, hostStatus, null, transactionContext);
					}
					catch (Exception ex)
					{
						_baseHostLogger.LogError(traceInfo, idHost, hostStatus, x => x.ExceptionInfo(ex), detail, transactionContext);
					}
				}

				if (isLocalTransaction)
					transactionContext.Commit();
			}
			catch (Exception ex)
			{
				_baseHostLogger.LogInformation(traceInfo, idHost, hostStatus, messageBuilder, detail, transactionContext);
				_baseHostLogger.LogCritical(traceInfo, idHost, hostStatus, x => x.ExceptionInfo(ex), detail, transactionContext);
				throw;
			}
		}

		public void LogWarning(
			ITraceInfo traceInfo,
			Guid idHost,
			HostStatus hostStatus,
			Action<LogMessageBuilder> messageBuilder,
			string? detail = null,
			ITransactionContext? transactionContext = null)
		{
			try
			{
				bool isLocalTransaction = true;
				if (transactionContext == null)
					transactionContext = CreateTransactionContext();
				else
					isLocalTransaction = false;

				var connection = transactionContext.GetItem<NpgsqlConnection>(nameof(NpgsqlConnection));
				var transaction = transactionContext.GetItemIfExists<NpgsqlTransaction>(nameof(NpgsqlTransaction));

				var builder = new LogMessageBuilder(traceInfo).LogLevel(LogLevel.Warning);
				messageBuilder?.Invoke(builder);
				var message = builder.Build();

				var dbHostLog = new DbHostLog
				{
					IdHost = idHost,
					IdLogLevel = (int)LogLevel.Warning,
					RuntimeUniqueKey = EnvironmentInfo.RUNTIME_UNIQUE_KEY,
					TimeCreatedUtc = DateTime.UtcNow,
					IdHostStatus = (int)hostStatus,
					LogMessage = message,
					LogDetail = detail
				};

				var sql = DbHostLog.GetInsertSql(_options);

				using var cmd = new NpgsqlCommand(sql, connection);
				if (transaction != null)
					cmd.Transaction = transaction;

				var table = DbHostLog.GetDictionaryTable(_options);
				table.SetParameters(cmd, dbHostLog.ToDictionary(_serialzier));

				var result = cmd.ExecuteNonQuery();

				if (result != 1)
					throw new InvalidOperationException($"{nameof(LogWarning)}: {nameof(cmd.ExecuteNonQuery)} returns {result} | {nameof(idHost)} = {idHost}");

				if (hostStatus != HostStatus.Unchanged)
				{
					try
					{
						UpdateHost(idHost, hostStatus, null, transactionContext);
					}
					catch (Exception ex)
					{
						_baseHostLogger.LogError(traceInfo, idHost, hostStatus, x => x.ExceptionInfo(ex), detail, transactionContext);
					}
				}

				if (isLocalTransaction)
					transactionContext.Commit();
			}
			catch (Exception ex)
			{
				_baseHostLogger.LogWarning(traceInfo, idHost, hostStatus, messageBuilder, detail, transactionContext);
				_baseHostLogger.LogCritical(traceInfo, idHost, hostStatus, x => x.ExceptionInfo(ex), detail, transactionContext);
				throw;
			}
		}

		public void LogError(
			ITraceInfo traceInfo,
			Guid idHost,
			HostStatus hostStatus,
			Action<ErrorMessageBuilder> messageBuilder,
			string? detail = null,
			ITransactionContext? transactionContext = null)
		{
			try
			{
				bool isLocalTransaction = true;
				if (transactionContext == null)
					transactionContext = CreateTransactionContext();
				else
					isLocalTransaction = false;

				var connection = transactionContext.GetItem<NpgsqlConnection>(nameof(NpgsqlConnection));
				var transaction = transactionContext.GetItemIfExists<NpgsqlTransaction>(nameof(NpgsqlTransaction));

				var builder = new ErrorMessageBuilder(traceInfo).LogLevel(LogLevel.Error);
				messageBuilder?.Invoke(builder);
				var message = builder.Build();

				var dbHostLog = new DbHostLog
				{
					IdHost = idHost,
					IdLogLevel = (int)LogLevel.Error,
					RuntimeUniqueKey = EnvironmentInfo.RUNTIME_UNIQUE_KEY,
					TimeCreatedUtc = DateTime.UtcNow,
					IdHostStatus = (int)hostStatus,
					LogMessage = message,
					LogDetail = detail
				};

				var sql = DbHostLog.GetInsertSql(_options);

				using var cmd = new NpgsqlCommand(sql, connection);
				if (transaction != null)
					cmd.Transaction = transaction;

				var table = DbHostLog.GetDictionaryTable(_options);
				table.SetParameters(cmd, dbHostLog.ToDictionary(_serialzier));

				var result = cmd.ExecuteNonQuery();

				if (result != 1)
					throw new InvalidOperationException($"{nameof(LogError)}: {nameof(cmd.ExecuteNonQuery)} returns {result} | {nameof(idHost)} = {idHost}");

				if (hostStatus != HostStatus.Unchanged)
				{
					try
					{
						UpdateHost(idHost, hostStatus, null, transactionContext);
					}
					catch (Exception ex)
					{
						_baseHostLogger.LogError(traceInfo, idHost, hostStatus, x => x.ExceptionInfo(ex), detail, transactionContext);
					}
				}

				if (isLocalTransaction)
					transactionContext.Commit();
			}
			catch (Exception ex)
			{
				_baseHostLogger.LogError(traceInfo, idHost, hostStatus, messageBuilder, detail, transactionContext);
				_baseHostLogger.LogCritical(traceInfo, idHost, hostStatus, x => x.ExceptionInfo(ex), detail, transactionContext);
				throw;
			}
		}

		public void LogCritical(
			ITraceInfo traceInfo,
			Guid idHost,
			HostStatus hostStatus,
			Action<ErrorMessageBuilder> messageBuilder,
			string? detail = null,
			ITransactionContext? transactionContext = null)
		{
			try
			{
				bool isLocalTransaction = true;
				if (transactionContext == null)
					transactionContext = CreateTransactionContext();
				else
					isLocalTransaction = false;

				var connection = transactionContext.GetItem<NpgsqlConnection>(nameof(NpgsqlConnection));
				var transaction = transactionContext.GetItemIfExists<NpgsqlTransaction>(nameof(NpgsqlTransaction));

				var builder = new ErrorMessageBuilder(traceInfo).LogLevel(LogLevel.Critical);
				messageBuilder?.Invoke(builder);
				var message = builder.Build();

				var dbHostLog = new DbHostLog
				{
					IdHost = idHost,
					IdLogLevel = (int)LogLevel.Critical,
					RuntimeUniqueKey = EnvironmentInfo.RUNTIME_UNIQUE_KEY,
					TimeCreatedUtc = DateTime.UtcNow,
					IdHostStatus = (int)hostStatus,
					LogMessage = message,
					LogDetail = detail
				};

				var sql = DbHostLog.GetInsertSql(_options);

				using var cmd = new NpgsqlCommand(sql, connection);
				if (transaction != null)
					cmd.Transaction = transaction;

				var table = DbHostLog.GetDictionaryTable(_options);
				table.SetParameters(cmd, dbHostLog.ToDictionary(_serialzier));

				var result = cmd.ExecuteNonQuery();

				if (result != 1)
					throw new InvalidOperationException($"{nameof(LogCritical)}: {nameof(cmd.ExecuteNonQuery)} returns {result} | {nameof(idHost)} = {idHost}");

				if (hostStatus != HostStatus.Unchanged)
				{
					try
					{
						UpdateHost(idHost, hostStatus, null, transactionContext);
					}
					catch (Exception ex)
					{
						_baseHostLogger.LogError(traceInfo, idHost, hostStatus, x => x.ExceptionInfo(ex), detail, transactionContext);
					}
				}

				if (isLocalTransaction)
					transactionContext.Commit();
			}
			catch (Exception ex)
			{
				_baseHostLogger.LogCritical(traceInfo, idHost, hostStatus, messageBuilder, detail, transactionContext);
				_baseHostLogger.LogCritical(traceInfo, idHost, hostStatus, x => x.ExceptionInfo(ex), detail, transactionContext);
				throw;
			}
		}

		public async Task LogTraceAsync(
			ITraceInfo traceInfo,
			Guid idHost,
			HostStatus hostStatus,
			Action<LogMessageBuilder> messageBuilder,
			string? detail = null,
			ITransactionContext? transactionContext = null,
			CancellationToken cancellationToken = default)
		{
			try
			{
				bool isLocalTransaction = true;
				if (transactionContext == null)
					transactionContext = await CreateTransactionContextAsync(cancellationToken);
				else
					isLocalTransaction = false;

				var connection = transactionContext.GetItem<NpgsqlConnection>(nameof(NpgsqlConnection));
				var transaction = transactionContext.GetItemIfExists<NpgsqlTransaction>(nameof(NpgsqlTransaction));

				var builder = new LogMessageBuilder(traceInfo).LogLevel(LogLevel.Trace);
				messageBuilder?.Invoke(builder);
				var message = builder.Build();

				var dbHostLog = new DbHostLog
				{
					IdHost = idHost,
					IdLogLevel = (int)LogLevel.Trace,
					RuntimeUniqueKey = EnvironmentInfo.RUNTIME_UNIQUE_KEY,
					TimeCreatedUtc = DateTime.UtcNow,
					IdHostStatus = (int)hostStatus,
					LogMessage = message,
					LogDetail = detail
				};

				var sql = DbHostLog.GetInsertSql(_options);

				using var cmd = new NpgsqlCommand(sql, connection);
				if (transaction != null)
					cmd.Transaction = transaction;

				var table = DbHostLog.GetDictionaryTable(_options);
				table.SetParameters(cmd, dbHostLog.ToDictionary(_serialzier));

				var result = await cmd.ExecuteNonQueryAsync(cancellationToken);

				if (result != 1)
					throw new InvalidOperationException($"{nameof(LogTraceAsync)}: {nameof(cmd.ExecuteNonQueryAsync)} returns {result} | {nameof(idHost)} = {idHost}");

				if (hostStatus != HostStatus.Unchanged)
				{
					try
					{
						await UpdateHostAsync(idHost, hostStatus, null, transactionContext, cancellationToken);
					}
					catch (Exception ex)
					{
						await _baseHostLogger.LogErrorAsync(traceInfo, idHost, hostStatus, x => x.ExceptionInfo(ex), detail, transactionContext, cancellationToken);
					}
				}

				if (isLocalTransaction)
					await transactionContext.CommitAsync(cancellationToken);
			}
			catch (Exception ex)
			{
				await _baseHostLogger.LogTraceAsync(traceInfo, idHost, hostStatus, messageBuilder, detail, transactionContext, cancellationToken);
				await _baseHostLogger.LogCriticalAsync(traceInfo, idHost, hostStatus, x => x.ExceptionInfo(ex), detail, transactionContext, cancellationToken);
				throw;
			}
		}

		public async Task LogDebugAsync(
			ITraceInfo traceInfo,
			Guid idHost,
			HostStatus hostStatus,
			Action<LogMessageBuilder> messageBuilder,
			string? detail = null,
			ITransactionContext? transactionContext = null,
			CancellationToken cancellationToken = default)
		{
			try
			{
				bool isLocalTransaction = true;
				if (transactionContext == null)
					transactionContext = await CreateTransactionContextAsync(cancellationToken);
				else
					isLocalTransaction = false;

				var connection = transactionContext.GetItem<NpgsqlConnection>(nameof(NpgsqlConnection));
				var transaction = transactionContext.GetItemIfExists<NpgsqlTransaction>(nameof(NpgsqlTransaction));

				var builder = new LogMessageBuilder(traceInfo).LogLevel(LogLevel.Debug);
				messageBuilder?.Invoke(builder);
				var message = builder.Build();

				var dbHostLog = new DbHostLog
				{
					IdHost = idHost,
					IdLogLevel = (int)LogLevel.Debug,
					RuntimeUniqueKey = EnvironmentInfo.RUNTIME_UNIQUE_KEY,
					TimeCreatedUtc = DateTime.UtcNow,
					IdHostStatus = (int)hostStatus,
					LogMessage = message,
					LogDetail = detail
				};

				var sql = DbHostLog.GetInsertSql(_options);

				using var cmd = new NpgsqlCommand(sql, connection);
				if (transaction != null)
					cmd.Transaction = transaction;

				var table = DbHostLog.GetDictionaryTable(_options);
				table.SetParameters(cmd, dbHostLog.ToDictionary(_serialzier));

				var result = await cmd.ExecuteNonQueryAsync(cancellationToken);

				if (result != 1)
					throw new InvalidOperationException($"{nameof(LogDebugAsync)}: {nameof(cmd.ExecuteNonQueryAsync)} returns {result} | {nameof(idHost)} = {idHost}");

				if (hostStatus != HostStatus.Unchanged)
				{
					try
					{
						await UpdateHostAsync(idHost, hostStatus, null, transactionContext, cancellationToken);
					}
					catch (Exception ex)
					{
						await _baseHostLogger.LogErrorAsync(traceInfo, idHost, hostStatus, x => x.ExceptionInfo(ex), detail, transactionContext, cancellationToken);
					}
				}

				if (isLocalTransaction)
					await transactionContext.CommitAsync(cancellationToken);
			}
			catch (Exception ex)
			{
				await _baseHostLogger.LogDebugAsync(traceInfo, idHost, hostStatus, messageBuilder, detail, transactionContext, cancellationToken);
				await _baseHostLogger.LogCriticalAsync(traceInfo, idHost, hostStatus, x => x.ExceptionInfo(ex), detail, transactionContext, cancellationToken);
				throw;
			}
		}

		public async Task LogInformationAsync(
			ITraceInfo traceInfo,
			Guid idHost,
			HostStatus hostStatus,
			Action<LogMessageBuilder> messageBuilder,
			string? detail = null,
			ITransactionContext? transactionContext = null,
			CancellationToken cancellationToken = default)
		{
			try
			{
				bool isLocalTransaction = true;
				if (transactionContext == null)
					transactionContext = await CreateTransactionContextAsync(cancellationToken);
				else
					isLocalTransaction = false;

				var connection = transactionContext.GetItem<NpgsqlConnection>(nameof(NpgsqlConnection));
				var transaction = transactionContext.GetItemIfExists<NpgsqlTransaction>(nameof(NpgsqlTransaction));

				var builder = new LogMessageBuilder(traceInfo).LogLevel(LogLevel.Information);
				messageBuilder?.Invoke(builder);
				var message = builder.Build();

				var dbHostLog = new DbHostLog
				{
					IdHost = idHost,
					IdLogLevel = (int)LogLevel.Information,
					RuntimeUniqueKey = EnvironmentInfo.RUNTIME_UNIQUE_KEY,
					TimeCreatedUtc = DateTime.UtcNow,
					IdHostStatus = (int)hostStatus,
					LogMessage = message,
					LogDetail = detail
				};

				var sql = DbHostLog.GetInsertSql(_options);

				using var cmd = new NpgsqlCommand(sql, connection);
				if (transaction != null)
					cmd.Transaction = transaction;

				var table = DbHostLog.GetDictionaryTable(_options);
				table.SetParameters(cmd, dbHostLog.ToDictionary(_serialzier));

				var result = await cmd.ExecuteNonQueryAsync(cancellationToken);

				if (result != 1)
					throw new InvalidOperationException($"{nameof(LogInformationAsync)}: {nameof(cmd.ExecuteNonQueryAsync)} returns {result} | {nameof(idHost)} = {idHost}");

				if (hostStatus != HostStatus.Unchanged)
				{
					try
					{
						await UpdateHostAsync(idHost, hostStatus, null, transactionContext, cancellationToken);
					}
					catch (Exception ex)
					{
						await _baseHostLogger.LogErrorAsync(traceInfo, idHost, hostStatus, x => x.ExceptionInfo(ex), detail, transactionContext, cancellationToken);
					}
				}

				if (isLocalTransaction)
					await transactionContext.CommitAsync(cancellationToken);
			}
			catch (Exception ex)
			{
				await _baseHostLogger.LogInformationAsync(traceInfo, idHost, hostStatus, messageBuilder, detail, transactionContext, cancellationToken);
				await _baseHostLogger.LogCriticalAsync(traceInfo, idHost, hostStatus, x => x.ExceptionInfo(ex), detail, transactionContext, cancellationToken);
				throw;
			}
		}

		public async Task LogWarningAsync(
			ITraceInfo traceInfo,
			Guid idHost,
			HostStatus hostStatus,
			Action<LogMessageBuilder> messageBuilder,
			string? detail = null,
			ITransactionContext? transactionContext = null,
			CancellationToken cancellationToken = default)
		{
			try
			{
				bool isLocalTransaction = true;
				if (transactionContext == null)
					transactionContext = await CreateTransactionContextAsync(cancellationToken);
				else
					isLocalTransaction = false;

				var connection = transactionContext.GetItem<NpgsqlConnection>(nameof(NpgsqlConnection));
				var transaction = transactionContext.GetItemIfExists<NpgsqlTransaction>(nameof(NpgsqlTransaction));

				var builder = new LogMessageBuilder(traceInfo).LogLevel(LogLevel.Warning);
				messageBuilder?.Invoke(builder);
				var message = builder.Build();

				var dbHostLog = new DbHostLog
				{
					IdHost = idHost,
					IdLogLevel = (int)LogLevel.Warning,
					RuntimeUniqueKey = EnvironmentInfo.RUNTIME_UNIQUE_KEY,
					TimeCreatedUtc = DateTime.UtcNow,
					IdHostStatus = (int)hostStatus,
					LogMessage = message,
					LogDetail = detail
				};

				var sql = DbHostLog.GetInsertSql(_options);

				using var cmd = new NpgsqlCommand(sql, connection);
				if (transaction != null)
					cmd.Transaction = transaction;

				var table = DbHostLog.GetDictionaryTable(_options);
				table.SetParameters(cmd, dbHostLog.ToDictionary(_serialzier));

				var result = await cmd.ExecuteNonQueryAsync(cancellationToken);

				if (result != 1)
					throw new InvalidOperationException($"{nameof(LogWarningAsync)}: {nameof(cmd.ExecuteNonQueryAsync)} returns {result} | {nameof(idHost)} = {idHost}");

				if (hostStatus != HostStatus.Unchanged)
				{
					try
					{
						await UpdateHostAsync(idHost, hostStatus, null, transactionContext, cancellationToken);
					}
					catch (Exception ex)
					{
						await _baseHostLogger.LogErrorAsync(traceInfo, idHost, hostStatus, x => x.ExceptionInfo(ex), detail, transactionContext, cancellationToken);
					}
				}

				if (isLocalTransaction)
					await transactionContext.CommitAsync(cancellationToken);
			}
			catch (Exception ex)
			{
				await _baseHostLogger.LogWarningAsync(traceInfo, idHost, hostStatus, messageBuilder, detail, transactionContext, cancellationToken);
				await _baseHostLogger.LogCriticalAsync(traceInfo, idHost, hostStatus, x => x.ExceptionInfo(ex), detail, transactionContext, cancellationToken);
				throw;
			}
		}

		public async Task LogErrorAsync(
			ITraceInfo traceInfo,
			Guid idHost,
			HostStatus hostStatus,
			Action<ErrorMessageBuilder> messageBuilder,
			string? detail = null,
			ITransactionContext? transactionContext = null,
			CancellationToken cancellationToken = default)
		{
			try
			{
				bool isLocalTransaction = true;
				if (transactionContext == null)
					transactionContext = await CreateTransactionContextAsync(cancellationToken);
				else
					isLocalTransaction = false;

				var connection = transactionContext.GetItem<NpgsqlConnection>(nameof(NpgsqlConnection));
				var transaction = transactionContext.GetItemIfExists<NpgsqlTransaction>(nameof(NpgsqlTransaction));

				var builder = new ErrorMessageBuilder(traceInfo).LogLevel(LogLevel.Error);
				messageBuilder?.Invoke(builder);
				var message = builder.Build();

				var dbHostLog = new DbHostLog
				{
					IdHost = idHost,
					IdLogLevel = (int)LogLevel.Error,
					RuntimeUniqueKey = EnvironmentInfo.RUNTIME_UNIQUE_KEY,
					TimeCreatedUtc = DateTime.UtcNow,
					IdHostStatus = (int)hostStatus,
					LogMessage = message,
					LogDetail = detail
				};

				var sql = DbHostLog.GetInsertSql(_options);

				using var cmd = new NpgsqlCommand(sql, connection);
				if (transaction != null)
					cmd.Transaction = transaction;

				var table = DbHostLog.GetDictionaryTable(_options);
				table.SetParameters(cmd, dbHostLog.ToDictionary(_serialzier));

				var result = await cmd.ExecuteNonQueryAsync(cancellationToken);

				if (result != 1)
					throw new InvalidOperationException($"{nameof(LogErrorAsync)}: {nameof(cmd.ExecuteNonQueryAsync)} returns {result} | {nameof(idHost)} = {idHost}");

				if (hostStatus != HostStatus.Unchanged)
				{
					try
					{
						await UpdateHostAsync(idHost, hostStatus, null, transactionContext, cancellationToken);
					}
					catch (Exception ex)
					{
						await _baseHostLogger.LogErrorAsync(traceInfo, idHost, hostStatus, x => x.ExceptionInfo(ex), detail, transactionContext, cancellationToken);
					}
				}

				if (isLocalTransaction)
					await transactionContext.CommitAsync(cancellationToken);
			}
			catch (Exception ex)
			{
				await _baseHostLogger.LogErrorAsync(traceInfo, idHost, hostStatus, messageBuilder, detail, transactionContext, cancellationToken);
				await _baseHostLogger.LogCriticalAsync(traceInfo, idHost, hostStatus, x => x.ExceptionInfo(ex), detail, transactionContext, cancellationToken);
				throw;
			}
		}

		public async Task LogCriticalAsync(
			ITraceInfo traceInfo,
			Guid idHost,
			HostStatus hostStatus,
			Action<ErrorMessageBuilder> messageBuilder,
			string? detail = null,
			ITransactionContext? transactionContext = null,
			CancellationToken cancellationToken = default)
		{
			try
			{
				bool isLocalTransaction = true;
				if (transactionContext == null)
					transactionContext = await CreateTransactionContextAsync(cancellationToken);
				else
					isLocalTransaction = false;

				var connection = transactionContext.GetItem<NpgsqlConnection>(nameof(NpgsqlConnection));
				var transaction = transactionContext.GetItemIfExists<NpgsqlTransaction>(nameof(NpgsqlTransaction));

				var builder = new ErrorMessageBuilder(traceInfo).LogLevel(LogLevel.Critical);
				messageBuilder?.Invoke(builder);
				var message = builder.Build();

				var dbHostLog = new DbHostLog
				{
					IdHost = idHost,
					IdLogLevel = (int)LogLevel.Critical,
					RuntimeUniqueKey = EnvironmentInfo.RUNTIME_UNIQUE_KEY,
					TimeCreatedUtc = DateTime.UtcNow,
					IdHostStatus = (int)hostStatus,
					LogMessage = message,
					LogDetail = detail
				};

				var sql = DbHostLog.GetInsertSql(_options);

				using var cmd = new NpgsqlCommand(sql, connection);
				if (transaction != null)
					cmd.Transaction = transaction;

				var table = DbHostLog.GetDictionaryTable(_options);
				table.SetParameters(cmd, dbHostLog.ToDictionary(_serialzier));

				var result = await cmd.ExecuteNonQueryAsync(cancellationToken);

				if (result != 1)
					throw new InvalidOperationException($"{nameof(LogCriticalAsync)}: {nameof(cmd.ExecuteNonQueryAsync)} returns {result} | {nameof(idHost)} = {idHost}");

				if (hostStatus != HostStatus.Unchanged)
				{
					try
					{
						await UpdateHostAsync(idHost, hostStatus, null, transactionContext, cancellationToken);
					}
					catch (Exception ex)
					{
						await _baseHostLogger.LogErrorAsync(traceInfo, idHost, hostStatus, x => x.ExceptionInfo(ex), detail, transactionContext, cancellationToken);
					}
				}

				if (isLocalTransaction)
					await transactionContext.CommitAsync(cancellationToken);
			}
			catch (Exception ex)
			{
				await _baseHostLogger.LogCriticalAsync(traceInfo, idHost, hostStatus, messageBuilder, detail, transactionContext, cancellationToken);
				await _baseHostLogger.LogCriticalAsync(traceInfo, idHost, hostStatus, x => x.ExceptionInfo(ex), detail, transactionContext, cancellationToken);
				throw;
			}
		}
	}
}

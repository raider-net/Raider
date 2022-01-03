using Microsoft.Extensions.Logging;
using Npgsql;
using Raider.Infrastructure;
using Raider.Logging;
using Raider.ServiceBus.BusLogger;
using Raider.ServiceBus.Messages;
using Raider.ServiceBus.PostgreSql.Messages.Storage.Model;
using Raider.ServiceBus.PostgreSql.Storage;
using Raider.Trace;
using Raider.Transactions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Raider.ServiceBus.PostgreSql.Events.Storage
{
	internal partial class PostgreSqlEventBusStorage : PostgreSqlHostStorage, IHandlerMessageLogger
	{
		public void LogTrace(
			ITraceInfo traceInfo,
			Guid idMessage,
			MessageStatus messageStatus,
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

				var messageLog = new DbHandlerMessageLog
				{
					IdHandlerMessage = idMessage,
					IdLogLevel = (int)LogLevel.Trace,
					RuntimeUniqueKey = EnvironmentInfo.RUNTIME_UNIQUE_KEY,
					TimeCreatedUtc = DateTime.UtcNow,
					IdMessageStatus = (int)messageStatus,
					LogMessage = message,
					LogDetail = detail
				};

				var sql = DbHandlerMessageLog.GetInsertSql(_options);

				using var cmd = new NpgsqlCommand(sql, connection);
				if (transaction != null)
					cmd.Transaction = transaction;

				var table = DbHandlerMessageLog.GetDictionaryTable(_options);
				table.SetParameters(cmd, messageLog.ToDictionary(_serialzier));

				var result = cmd.ExecuteNonQuery();

				if (result != 1)
					throw new InvalidOperationException($"{nameof(LogTrace)}: {nameof(cmd.ExecuteNonQuery)} returns {result} | {nameof(idMessage)} = {idMessage}");

				if (messageStatus != MessageStatus.Unchanged)
				{
					try
					{
						UpdateEventStatus(idMessage, messageStatus, transactionContext);
					}
					catch (Exception ex)
					{
						_baseHandlerEventLogger.LogError(traceInfo, idMessage, messageStatus, x => x.ExceptionInfo(ex), detail, transactionContext);
					}
				}

				if (isLocalTransaction)
					transactionContext.Commit();
			}
			catch (Exception ex)
			{
				_baseHandlerEventLogger.LogTrace(traceInfo, idMessage, messageStatus, messageBuilder, detail, transactionContext);
				_baseHandlerEventLogger.LogCritical(traceInfo, idMessage, messageStatus, x => x.ExceptionInfo(ex), detail, transactionContext);
				throw;
			}
		}

		public void LogDebug(
			ITraceInfo traceInfo,
			Guid idMessage,
			MessageStatus messageStatus,
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

				var messageLog = new DbHandlerMessageLog
				{
					IdHandlerMessage = idMessage,
					IdLogLevel = (int)LogLevel.Debug,
					RuntimeUniqueKey = EnvironmentInfo.RUNTIME_UNIQUE_KEY,
					TimeCreatedUtc = DateTime.UtcNow,
					IdMessageStatus = (int)messageStatus,
					LogMessage = message,
					LogDetail = detail
				};

				var sql = DbHandlerMessageLog.GetInsertSql(_options);

				using var cmd = new NpgsqlCommand(sql, connection);
				if (transaction != null)
					cmd.Transaction = transaction;

				var table = DbHandlerMessageLog.GetDictionaryTable(_options);
				table.SetParameters(cmd, messageLog.ToDictionary(_serialzier));

				var result = cmd.ExecuteNonQuery();

				if (result != 1)
					throw new InvalidOperationException($"{nameof(LogDebug)}: {nameof(cmd.ExecuteNonQuery)} returns {result} | {nameof(idMessage)} = {idMessage}");

				if (messageStatus != MessageStatus.Unchanged)
				{
					try
					{
						UpdateEventStatus(idMessage, messageStatus, transactionContext);
					}
					catch (Exception ex)
					{
						_baseHandlerEventLogger.LogError(traceInfo, idMessage, messageStatus, x => x.ExceptionInfo(ex), detail, transactionContext);
					}
				}

				if (isLocalTransaction)
					transactionContext.Commit();
			}
			catch (Exception ex)
			{
				_baseHandlerEventLogger.LogDebug(traceInfo, idMessage, messageStatus, messageBuilder, detail, transactionContext);
				_baseHandlerEventLogger.LogCritical(traceInfo, idMessage, messageStatus, x => x.ExceptionInfo(ex), detail, transactionContext);
				throw;
			}
		}

		public void LogInformation(
			ITraceInfo traceInfo,
			Guid idMessage,
			MessageStatus messageStatus,
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

				var messageLog = new DbHandlerMessageLog
				{
					IdHandlerMessage = idMessage,
					IdLogLevel = (int)LogLevel.Information,
					RuntimeUniqueKey = EnvironmentInfo.RUNTIME_UNIQUE_KEY,
					TimeCreatedUtc = DateTime.UtcNow,
					IdMessageStatus = (int)messageStatus,
					LogMessage = message,
					LogDetail = detail
				};

				var sql = DbHandlerMessageLog.GetInsertSql(_options);

				using var cmd = new NpgsqlCommand(sql, connection);
				if (transaction != null)
					cmd.Transaction = transaction;

				var table = DbHandlerMessageLog.GetDictionaryTable(_options);
				table.SetParameters(cmd, messageLog.ToDictionary(_serialzier));

				var result = cmd.ExecuteNonQuery();

				if (result != 1)
					throw new InvalidOperationException($"{nameof(LogInformation)}: {nameof(cmd.ExecuteNonQuery)} returns {result} | {nameof(idMessage)} = {idMessage}");

				if (messageStatus != MessageStatus.Unchanged)
				{
					try
					{
						UpdateEventStatus(idMessage, messageStatus, transactionContext);
					}
					catch (Exception ex)
					{
						_baseHandlerEventLogger.LogError(traceInfo, idMessage, messageStatus, x => x.ExceptionInfo(ex), detail, transactionContext);
					}
				}

				if (isLocalTransaction)
					transactionContext.Commit();
			}
			catch (Exception ex)
			{
				_baseHandlerEventLogger.LogInformation(traceInfo, idMessage, messageStatus, messageBuilder, detail, transactionContext);
				_baseHandlerEventLogger.LogCritical(traceInfo, idMessage, messageStatus, x => x.ExceptionInfo(ex), detail, transactionContext);
				throw;
			}
		}

		public void LogWarning(
			ITraceInfo traceInfo,
			Guid idMessage,
			MessageStatus messageStatus,
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

				var messageLog = new DbHandlerMessageLog
				{
					IdHandlerMessage = idMessage,
					IdLogLevel = (int)LogLevel.Warning,
					RuntimeUniqueKey = EnvironmentInfo.RUNTIME_UNIQUE_KEY,
					TimeCreatedUtc = DateTime.UtcNow,
					IdMessageStatus = (int)messageStatus,
					LogMessage = message,
					LogDetail = detail
				};

				var sql = DbHandlerMessageLog.GetInsertSql(_options);

				using var cmd = new NpgsqlCommand(sql, connection);
				if (transaction != null)
					cmd.Transaction = transaction;

				var table = DbHandlerMessageLog.GetDictionaryTable(_options);
				table.SetParameters(cmd, messageLog.ToDictionary(_serialzier));

				var result = cmd.ExecuteNonQuery();

				if (result != 1)
					throw new InvalidOperationException($"{nameof(LogWarning)}: {nameof(cmd.ExecuteNonQuery)} returns {result} | {nameof(idMessage)} = {idMessage}");

				if (messageStatus != MessageStatus.Unchanged)
				{
					try
					{
						UpdateEventStatus(idMessage, messageStatus, transactionContext);
					}
					catch (Exception ex)
					{
						_baseHandlerEventLogger.LogError(traceInfo, idMessage, messageStatus, x => x.ExceptionInfo(ex), detail, transactionContext);
					}
				}

				if (isLocalTransaction)
					transactionContext.Commit();
			}
			catch (Exception ex)
			{
				_baseHandlerEventLogger.LogWarning(traceInfo, idMessage, messageStatus, messageBuilder, detail, transactionContext);
				_baseHandlerEventLogger.LogCritical(traceInfo, idMessage, messageStatus, x => x.ExceptionInfo(ex), detail, transactionContext);
				throw;
			}
		}

		public void LogError(
			ITraceInfo traceInfo,
			Guid idMessage,
			MessageStatus messageStatus,
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

				var messageLog = new DbHandlerMessageLog
				{
					IdHandlerMessage = idMessage,
					IdLogLevel = (int)LogLevel.Error,
					RuntimeUniqueKey = EnvironmentInfo.RUNTIME_UNIQUE_KEY,
					TimeCreatedUtc = DateTime.UtcNow,
					IdMessageStatus = (int)messageStatus,
					LogMessage = message,
					LogDetail = detail
				};

				var sql = DbHandlerMessageLog.GetInsertSql(_options);

				using var cmd = new NpgsqlCommand(sql, connection);
				if (transaction != null)
					cmd.Transaction = transaction;

				var table = DbHandlerMessageLog.GetDictionaryTable(_options);
				table.SetParameters(cmd, messageLog.ToDictionary(_serialzier));

				var result = cmd.ExecuteNonQuery();

				if (result != 1)
					throw new InvalidOperationException($"{nameof(LogError)}: {nameof(cmd.ExecuteNonQuery)} returns {result} | {nameof(idMessage)} = {idMessage}");

				if (messageStatus != MessageStatus.Unchanged)
				{
					try
					{
						UpdateEventStatus(idMessage, messageStatus, transactionContext);
					}
					catch (Exception ex)
					{
						_baseHandlerEventLogger.LogError(traceInfo, idMessage, messageStatus, x => x.ExceptionInfo(ex), detail, transactionContext);
					}
				}

				if (isLocalTransaction)
					transactionContext.Commit();
			}
			catch (Exception ex)
			{
				_baseHandlerEventLogger.LogError(traceInfo, idMessage, messageStatus, messageBuilder, detail, transactionContext);
				_baseHandlerEventLogger.LogCritical(traceInfo, idMessage, messageStatus, x => x.ExceptionInfo(ex), detail, transactionContext);
				throw;
			}
		}

		public void LogCritical(
			ITraceInfo traceInfo,
			Guid idMessage,
			MessageStatus messageStatus,
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

				var messageLog = new DbHandlerMessageLog
				{
					IdHandlerMessage = idMessage,
					IdLogLevel = (int)LogLevel.Critical,
					RuntimeUniqueKey = EnvironmentInfo.RUNTIME_UNIQUE_KEY,
					TimeCreatedUtc = DateTime.UtcNow,
					IdMessageStatus = (int)messageStatus,
					LogMessage = message,
					LogDetail = detail
				};

				var sql = DbHandlerMessageLog.GetInsertSql(_options);

				using var cmd = new NpgsqlCommand(sql, connection);
				if (transaction != null)
					cmd.Transaction = transaction;

				var table = DbHandlerMessageLog.GetDictionaryTable(_options);
				table.SetParameters(cmd, messageLog.ToDictionary(_serialzier));

				var result = cmd.ExecuteNonQuery();

				if (result != 1)
					throw new InvalidOperationException($"{nameof(LogCritical)}: {nameof(cmd.ExecuteNonQuery)} returns {result} | {nameof(idMessage)} = {idMessage}");

				if (messageStatus != MessageStatus.Unchanged)
				{
					try
					{
						UpdateEventStatus(idMessage, messageStatus, transactionContext);
					}
					catch (Exception ex)
					{
						_baseHandlerEventLogger.LogError(traceInfo, idMessage, messageStatus, x => x.ExceptionInfo(ex), detail, transactionContext);
					}
				}

				if (isLocalTransaction)
					transactionContext.Commit();
			}
			catch (Exception ex)
			{
				_baseHandlerEventLogger.LogCritical(traceInfo, idMessage, messageStatus, messageBuilder, detail, transactionContext);
				_baseHandlerEventLogger.LogCritical(traceInfo, idMessage, messageStatus, x => x.ExceptionInfo(ex), detail, transactionContext);
				throw;
			}
		}

		public async Task LogTraceAsync(
			ITraceInfo traceInfo,
			Guid idMessage,
			MessageStatus messageStatus,
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

				var messageLog = new DbHandlerMessageLog
				{
					IdHandlerMessage = idMessage,
					IdLogLevel = (int)LogLevel.Trace,
					RuntimeUniqueKey = EnvironmentInfo.RUNTIME_UNIQUE_KEY,
					TimeCreatedUtc = DateTime.UtcNow,
					IdMessageStatus = (int)messageStatus,
					LogMessage = message,
					LogDetail = detail
				};

				var sql = DbHandlerMessageLog.GetInsertSql(_options);

				using var cmd = new NpgsqlCommand(sql, connection);
				if (transaction != null)
					cmd.Transaction = transaction;

				var table = DbHandlerMessageLog.GetDictionaryTable(_options);
				table.SetParameters(cmd, messageLog.ToDictionary(_serialzier));

				var result = await cmd.ExecuteNonQueryAsync(cancellationToken);

				if (result != 1)
					throw new InvalidOperationException($"{nameof(LogTraceAsync)}: {nameof(cmd.ExecuteNonQueryAsync)} returns {result} | {nameof(idMessage)} = {idMessage}");

				if (messageStatus != MessageStatus.Unchanged)
				{
					try
					{
						await UpdateEventStatusAsync(idMessage, messageStatus, transactionContext, cancellationToken);
					}
					catch (Exception ex)
					{
						await _baseHandlerEventLogger.LogErrorAsync(traceInfo, idMessage, messageStatus, x => x.ExceptionInfo(ex), detail, transactionContext, cancellationToken);
					}
				}

				if (isLocalTransaction)
					await transactionContext.CommitAsync(cancellationToken);
			}
			catch (Exception ex)
			{
				await _baseHandlerEventLogger.LogTraceAsync(traceInfo, idMessage, messageStatus, messageBuilder, detail, transactionContext, cancellationToken);
				await _baseHandlerEventLogger.LogCriticalAsync(traceInfo, idMessage, messageStatus, x => x.ExceptionInfo(ex), detail, transactionContext, cancellationToken);
				throw;
			}
		}

		public async Task LogDebugAsync(
			ITraceInfo traceInfo,
			Guid idMessage,
			MessageStatus messageStatus,
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

				var messageLog = new DbHandlerMessageLog
				{
					IdHandlerMessage = idMessage,
					IdLogLevel = (int)LogLevel.Debug,
					RuntimeUniqueKey = EnvironmentInfo.RUNTIME_UNIQUE_KEY,
					TimeCreatedUtc = DateTime.UtcNow,
					IdMessageStatus = (int)messageStatus,
					LogMessage = message,
					LogDetail = detail
				};

				var sql = DbHandlerMessageLog.GetInsertSql(_options);

				using var cmd = new NpgsqlCommand(sql, connection);
				if (transaction != null)
					cmd.Transaction = transaction;

				var table = DbHandlerMessageLog.GetDictionaryTable(_options);
				table.SetParameters(cmd, messageLog.ToDictionary(_serialzier));

				var result = await cmd.ExecuteNonQueryAsync(cancellationToken);

				if (result != 1)
					throw new InvalidOperationException($"{nameof(LogDebugAsync)}: {nameof(cmd.ExecuteNonQueryAsync)} returns {result} | {nameof(idMessage)} = {idMessage}");

				if (messageStatus != MessageStatus.Unchanged)
				{
					try
					{
						await UpdateEventStatusAsync(idMessage, messageStatus, transactionContext, cancellationToken);
					}
					catch (Exception ex)
					{
						await _baseHandlerEventLogger.LogErrorAsync(traceInfo, idMessage, messageStatus, x => x.ExceptionInfo(ex), detail, transactionContext, cancellationToken);
					}
				}

				if (isLocalTransaction)
					await transactionContext.CommitAsync(cancellationToken);
			}
			catch (Exception ex)
			{
				await _baseHandlerEventLogger.LogDebugAsync(traceInfo, idMessage, messageStatus, messageBuilder, detail, transactionContext, cancellationToken);
				await _baseHandlerEventLogger.LogCriticalAsync(traceInfo, idMessage, messageStatus, x => x.ExceptionInfo(ex), detail, transactionContext, cancellationToken);
				throw;
			}
		}

		public async Task LogInformationAsync(
			ITraceInfo traceInfo,
			Guid idMessage,
			MessageStatus messageStatus,
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

				var messageLog = new DbHandlerMessageLog
				{
					IdHandlerMessage = idMessage,
					IdLogLevel = (int)LogLevel.Information,
					RuntimeUniqueKey = EnvironmentInfo.RUNTIME_UNIQUE_KEY,
					TimeCreatedUtc = DateTime.UtcNow,
					IdMessageStatus = (int)messageStatus,
					LogMessage = message,
					LogDetail = detail
				};

				var sql = DbHandlerMessageLog.GetInsertSql(_options);

				using var cmd = new NpgsqlCommand(sql, connection);
				if (transaction != null)
					cmd.Transaction = transaction;

				var table = DbHandlerMessageLog.GetDictionaryTable(_options);
				table.SetParameters(cmd, messageLog.ToDictionary(_serialzier));

				var result = await cmd.ExecuteNonQueryAsync(cancellationToken);

				if (result != 1)
					throw new InvalidOperationException($"{nameof(LogInformationAsync)}: {nameof(cmd.ExecuteNonQueryAsync)} returns {result} | {nameof(idMessage)} = {idMessage}");

				if (messageStatus != MessageStatus.Unchanged)
				{
					try
					{
						await UpdateEventStatusAsync(idMessage, messageStatus, transactionContext, cancellationToken);
					}
					catch (Exception ex)
					{
						await _baseHandlerEventLogger.LogErrorAsync(traceInfo, idMessage, messageStatus, x => x.ExceptionInfo(ex), detail, transactionContext, cancellationToken);
					}
				}

				if (isLocalTransaction)
					await transactionContext.CommitAsync(cancellationToken);
			}
			catch (Exception ex)
			{
				await _baseHandlerEventLogger.LogInformationAsync(traceInfo, idMessage, messageStatus, messageBuilder, detail, transactionContext, cancellationToken);
				await _baseHandlerEventLogger.LogCriticalAsync(traceInfo, idMessage, messageStatus, x => x.ExceptionInfo(ex), detail, transactionContext, cancellationToken);
				throw;
			}
		}

		public async Task LogWarningAsync(
			ITraceInfo traceInfo,
			Guid idMessage,
			MessageStatus messageStatus,
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

				var messageLog = new DbHandlerMessageLog
				{
					IdHandlerMessage = idMessage,
					IdLogLevel = (int)LogLevel.Warning,
					RuntimeUniqueKey = EnvironmentInfo.RUNTIME_UNIQUE_KEY,
					TimeCreatedUtc = DateTime.UtcNow,
					IdMessageStatus = (int)messageStatus,
					LogMessage = message,
					LogDetail = detail
				};

				var sql = DbHandlerMessageLog.GetInsertSql(_options);

				using var cmd = new NpgsqlCommand(sql, connection);
				if (transaction != null)
					cmd.Transaction = transaction;

				var table = DbHandlerMessageLog.GetDictionaryTable(_options);
				table.SetParameters(cmd, messageLog.ToDictionary(_serialzier));

				var result = await cmd.ExecuteNonQueryAsync(cancellationToken);

				if (result != 1)
					throw new InvalidOperationException($"{nameof(LogWarningAsync)}: {nameof(cmd.ExecuteNonQueryAsync)} returns {result} | {nameof(idMessage)} = {idMessage}");

				if (messageStatus != MessageStatus.Unchanged)
				{
					try
					{
						await UpdateEventStatusAsync(idMessage, messageStatus, transactionContext, cancellationToken);
					}
					catch (Exception ex)
					{
						await _baseHandlerEventLogger.LogErrorAsync(traceInfo, idMessage, messageStatus, x => x.ExceptionInfo(ex), detail, transactionContext, cancellationToken);
					}
				}

				if (isLocalTransaction)
					await transactionContext.CommitAsync(cancellationToken);
			}
			catch (Exception ex)
			{
				await _baseHandlerEventLogger.LogWarningAsync(traceInfo, idMessage, messageStatus, messageBuilder, detail, transactionContext, cancellationToken);
				await _baseHandlerEventLogger.LogCriticalAsync(traceInfo, idMessage, messageStatus, x => x.ExceptionInfo(ex), detail, transactionContext, cancellationToken);
				throw;
			}
		}

		public async Task LogErrorAsync(
			ITraceInfo traceInfo,
			Guid idMessage,
			MessageStatus messageStatus,
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

				var messageLog = new DbHandlerMessageLog
				{
					IdHandlerMessage = idMessage,
					IdLogLevel = (int)LogLevel.Error,
					RuntimeUniqueKey = EnvironmentInfo.RUNTIME_UNIQUE_KEY,
					TimeCreatedUtc = DateTime.UtcNow,
					IdMessageStatus = (int)messageStatus,
					LogMessage = message,
					LogDetail = detail
				};

				var sql = DbHandlerMessageLog.GetInsertSql(_options);

				using var cmd = new NpgsqlCommand(sql, connection);
				if (transaction != null)
					cmd.Transaction = transaction;

				var table = DbHandlerMessageLog.GetDictionaryTable(_options);
				table.SetParameters(cmd, messageLog.ToDictionary(_serialzier));

				var result = await cmd.ExecuteNonQueryAsync(cancellationToken);

				if (result != 1)
					throw new InvalidOperationException($"{nameof(LogErrorAsync)}: {nameof(cmd.ExecuteNonQueryAsync)} returns {result} | {nameof(idMessage)} = {idMessage}");

				if (messageStatus != MessageStatus.Unchanged)
				{
					try
					{
						await UpdateEventStatusAsync(idMessage, messageStatus, transactionContext, cancellationToken);
					}
					catch (Exception ex)
					{
						await _baseHandlerEventLogger.LogErrorAsync(traceInfo, idMessage, messageStatus, x => x.ExceptionInfo(ex), detail, transactionContext, cancellationToken);
					}
				}

				if (isLocalTransaction)
					await transactionContext.CommitAsync(cancellationToken);
			}
			catch (Exception ex)
			{
				await _baseHandlerEventLogger.LogErrorAsync(traceInfo, idMessage, messageStatus, messageBuilder, detail, transactionContext, cancellationToken);
				await _baseHandlerEventLogger.LogCriticalAsync(traceInfo, idMessage, messageStatus, x => x.ExceptionInfo(ex), detail, transactionContext, cancellationToken);
				throw;
			}
		}

		public async Task LogCriticalAsync(
			ITraceInfo traceInfo,
			Guid idMessage,
			MessageStatus messageStatus,
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

				var messageLog = new DbHandlerMessageLog
				{
					IdHandlerMessage = idMessage,
					IdLogLevel = (int)LogLevel.Critical,
					RuntimeUniqueKey = EnvironmentInfo.RUNTIME_UNIQUE_KEY,
					TimeCreatedUtc = DateTime.UtcNow,
					IdMessageStatus = (int)messageStatus,
					LogMessage = message,
					LogDetail = detail
				};

				var sql = DbHandlerMessageLog.GetInsertSql(_options);

				using var cmd = new NpgsqlCommand(sql, connection);
				if (transaction != null)
					cmd.Transaction = transaction;

				var table = DbHandlerMessageLog.GetDictionaryTable(_options);
				table.SetParameters(cmd, messageLog.ToDictionary(_serialzier));

				var result = await cmd.ExecuteNonQueryAsync(cancellationToken);

				if (result != 1)
					throw new InvalidOperationException($"{nameof(LogCriticalAsync)}: {nameof(cmd.ExecuteNonQueryAsync)} returns {result} | {nameof(idMessage)} = {idMessage}");

				if (messageStatus != MessageStatus.Unchanged)
				{
					try
					{
						await UpdateEventStatusAsync(idMessage, messageStatus, transactionContext, cancellationToken);
					}
					catch (Exception ex)
					{
						await _baseHandlerEventLogger.LogErrorAsync(traceInfo, idMessage, messageStatus, x => x.ExceptionInfo(ex), detail, transactionContext, cancellationToken);
					}
				}

				if (isLocalTransaction)
					await transactionContext.CommitAsync(cancellationToken);
			}
			catch (Exception ex)
			{
				await _baseHandlerEventLogger.LogCriticalAsync(traceInfo, idMessage, messageStatus, messageBuilder, detail, transactionContext, cancellationToken);
				await _baseHandlerEventLogger.LogCriticalAsync(traceInfo, idMessage, messageStatus, x => x.ExceptionInfo(ex), detail, transactionContext, cancellationToken);
				throw;
			}
		}
	}
}

using Microsoft.Extensions.Logging;
using Npgsql;
using Raider.Infrastructure;
using Raider.Logging;
using Raider.ServiceBus.BusLogger;
using Raider.ServiceBus.Components;
using Raider.ServiceBus.PostgreSql.Messages.Storage.Model;
using Raider.Trace;
using Raider.Transactions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Raider.ServiceBus.PostgreSql.Storage
{
	internal partial class PostgreSqlServiceBusStorage : IComponentLogger
	{
		public async Task LogTraceAsync(
			ITraceInfo traceInfo,
			Guid idComponent,
			ComponentStatus componentStatus,
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

				var dbComponentLog = new DbComponentLog
				{
					IdComponent = idComponent,
					IdLogLevel = (int)LogLevel.Trace,
					RuntimeUniqueKey = EnvironmentInfo.RUNTIME_UNIQUE_KEY,
					TimeCreatedUtc = DateTime.UtcNow,
					IdComponentStatus = (int)componentStatus,
					LogMessage = message,
					LogDetail = detail
				};

				var sql = DbComponentLog.GetInsertSql(_options);

				using var cmd = new NpgsqlCommand(sql, connection);
				if (transaction != null)
					cmd.Transaction = transaction;

				var table = DbComponentLog.GetDictionaryTable(_options);
				table.SetParameters(cmd, dbComponentLog.ToDictionary(_serialzier));

				var result = await cmd.ExecuteNonQueryAsync(cancellationToken);

				if (result != 1)
					throw new InvalidOperationException($"{nameof(LogTraceAsync)}: {nameof(cmd.ExecuteNonQueryAsync)} returns {result} | {nameof(idComponent)} = {idComponent}");

				if (componentStatus != ComponentStatus.Unchanged)
				{
					try
					{
						await UpdateComponentStatusAsync(idComponent, componentStatus, transactionContext, cancellationToken);
					}
					catch (Exception ex)
					{
						await _baseComponentLogger.LogErrorAsync(traceInfo, idComponent, componentStatus, x => x.ExceptionInfo(ex), detail, transactionContext, cancellationToken);
					}
				}

				if (isLocalTransaction)
					await transactionContext.CommitAsync(cancellationToken);
			}
			catch (Exception ex)
			{
				await _baseComponentLogger.LogTraceAsync(traceInfo, idComponent, componentStatus, messageBuilder, detail, transactionContext, cancellationToken);
				await _baseComponentLogger.LogCriticalAsync(traceInfo, idComponent, componentStatus, x => x.ExceptionInfo(ex), detail, transactionContext, cancellationToken);
				throw;
			}
		}

		public async Task LogDebugAsync(
			ITraceInfo traceInfo,
			Guid idComponent,
			ComponentStatus componentStatus,
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

				var dbComponentLog = new DbComponentLog
				{
					IdComponent = idComponent,
					IdLogLevel = (int)LogLevel.Debug,
					RuntimeUniqueKey = EnvironmentInfo.RUNTIME_UNIQUE_KEY,
					TimeCreatedUtc = DateTime.UtcNow,
					IdComponentStatus = (int)componentStatus,
					LogMessage = message,
					LogDetail = detail
				};

				var sql = DbComponentLog.GetInsertSql(_options);

				using var cmd = new NpgsqlCommand(sql, connection);
				if (transaction != null)
					cmd.Transaction = transaction;

				var table = DbComponentLog.GetDictionaryTable(_options);
				table.SetParameters(cmd, dbComponentLog.ToDictionary(_serialzier));

				var result = await cmd.ExecuteNonQueryAsync(cancellationToken);

				if (result != 1)
					throw new InvalidOperationException($"{nameof(LogDebugAsync)}: {nameof(cmd.ExecuteNonQueryAsync)} returns {result} | {nameof(idComponent)} = {idComponent}");

				if (componentStatus != ComponentStatus.Unchanged)
				{
					try
					{
						await UpdateComponentStatusAsync(idComponent, componentStatus, transactionContext, cancellationToken);
					}
					catch (Exception ex)
					{
						await _baseComponentLogger.LogErrorAsync(traceInfo, idComponent, componentStatus, x => x.ExceptionInfo(ex), detail, transactionContext, cancellationToken);
					}
				}

				if (isLocalTransaction)
					await transactionContext.CommitAsync(cancellationToken);
			}
			catch (Exception ex)
			{
				await _baseComponentLogger.LogDebugAsync(traceInfo, idComponent, componentStatus, messageBuilder, detail, transactionContext, cancellationToken);
				await _baseComponentLogger.LogCriticalAsync(traceInfo, idComponent, componentStatus, x => x.ExceptionInfo(ex), detail, transactionContext, cancellationToken);
				throw;
			}
		}

		public async Task LogInformationAsync(
			ITraceInfo traceInfo,
			Guid idComponent,
			ComponentStatus componentStatus,
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

				var dbComponentLog = new DbComponentLog
				{
					IdComponent = idComponent,
					IdLogLevel = (int)LogLevel.Information,
					RuntimeUniqueKey = EnvironmentInfo.RUNTIME_UNIQUE_KEY,
					TimeCreatedUtc = DateTime.UtcNow,
					IdComponentStatus = (int)componentStatus,
					LogMessage = message,
					LogDetail = detail
				};

				var sql = DbComponentLog.GetInsertSql(_options);

				using var cmd = new NpgsqlCommand(sql, connection);
				if (transaction != null)
					cmd.Transaction = transaction;

				var table = DbComponentLog.GetDictionaryTable(_options);
				table.SetParameters(cmd, dbComponentLog.ToDictionary(_serialzier));

				var result = await cmd.ExecuteNonQueryAsync(cancellationToken);

				if (result != 1)
					throw new InvalidOperationException($"{nameof(LogInformationAsync)}: {nameof(cmd.ExecuteNonQueryAsync)} returns {result} | {nameof(idComponent)} = {idComponent}");

				if (componentStatus != ComponentStatus.Unchanged)
				{
					try
					{
						await UpdateComponentStatusAsync(idComponent, componentStatus, transactionContext, cancellationToken);
					}
					catch (Exception ex)
					{
						await _baseComponentLogger.LogErrorAsync(traceInfo, idComponent, componentStatus, x => x.ExceptionInfo(ex), detail, transactionContext, cancellationToken);
					}
				}

				if (isLocalTransaction)
					await transactionContext.CommitAsync(cancellationToken);
			}
			catch (Exception ex)
			{
				await _baseComponentLogger.LogInformationAsync(traceInfo, idComponent, componentStatus, messageBuilder, detail, transactionContext, cancellationToken);
				await _baseComponentLogger.LogCriticalAsync(traceInfo, idComponent, componentStatus, x => x.ExceptionInfo(ex), detail, transactionContext, cancellationToken);
				throw;
			}
		}

		public async Task LogWarningAsync(
			ITraceInfo traceInfo,
			Guid idComponent,
			ComponentStatus componentStatus,
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

				var dbComponentLog = new DbComponentLog
				{
					IdComponent = idComponent,
					IdLogLevel = (int)LogLevel.Warning,
					RuntimeUniqueKey = EnvironmentInfo.RUNTIME_UNIQUE_KEY,
					TimeCreatedUtc = DateTime.UtcNow,
					IdComponentStatus = (int)componentStatus,
					LogMessage = message,
					LogDetail = detail
				};

				var sql = DbComponentLog.GetInsertSql(_options);

				using var cmd = new NpgsqlCommand(sql, connection);
				if (transaction != null)
					cmd.Transaction = transaction;

				var table = DbComponentLog.GetDictionaryTable(_options);
				table.SetParameters(cmd, dbComponentLog.ToDictionary(_serialzier));

				var result = await cmd.ExecuteNonQueryAsync(cancellationToken);

				if (result != 1)
					throw new InvalidOperationException($"{nameof(LogWarningAsync)}: {nameof(cmd.ExecuteNonQueryAsync)} returns {result} | {nameof(idComponent)} = {idComponent}");

				if (componentStatus != ComponentStatus.Unchanged)
				{
					try
					{
						await UpdateComponentStatusAsync(idComponent, componentStatus, transactionContext, cancellationToken);
					}
					catch (Exception ex)
					{
						await _baseComponentLogger.LogErrorAsync(traceInfo, idComponent, componentStatus, x => x.ExceptionInfo(ex), detail, transactionContext, cancellationToken);
					}
				}

				if (isLocalTransaction)
					await transactionContext.CommitAsync(cancellationToken);
			}
			catch (Exception ex)
			{
				await _baseComponentLogger.LogWarningAsync(traceInfo, idComponent, componentStatus, messageBuilder, detail, transactionContext, cancellationToken);
				await _baseComponentLogger.LogCriticalAsync(traceInfo, idComponent, componentStatus, x => x.ExceptionInfo(ex), detail, transactionContext, cancellationToken);
				throw;
			}
		}

		public async Task LogErrorAsync(
			ITraceInfo traceInfo,
			Guid idComponent,
			ComponentStatus componentStatus,
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

				var dbComponentLog = new DbComponentLog
				{
					IdComponent = idComponent,
					IdLogLevel = (int)LogLevel.Error,
					RuntimeUniqueKey = EnvironmentInfo.RUNTIME_UNIQUE_KEY,
					TimeCreatedUtc = DateTime.UtcNow,
					IdComponentStatus = (int)componentStatus,
					LogMessage = message,
					LogDetail = detail
				};

				var sql = DbComponentLog.GetInsertSql(_options);

				using var cmd = new NpgsqlCommand(sql, connection);
				if (transaction != null)
					cmd.Transaction = transaction;

				var table = DbComponentLog.GetDictionaryTable(_options);
				table.SetParameters(cmd, dbComponentLog.ToDictionary(_serialzier));

				var result = await cmd.ExecuteNonQueryAsync(cancellationToken);

				if (result != 1)
					throw new InvalidOperationException($"{nameof(LogErrorAsync)}: {nameof(cmd.ExecuteNonQueryAsync)} returns {result} | {nameof(idComponent)} = {idComponent}");

				if (componentStatus != ComponentStatus.Unchanged)
				{
					try
					{
						await UpdateComponentStatusAsync(idComponent, componentStatus, transactionContext, cancellationToken);
					}
					catch (Exception ex)
					{
						await _baseComponentLogger.LogErrorAsync(traceInfo, idComponent, componentStatus, x => x.ExceptionInfo(ex), detail, transactionContext, cancellationToken);
					}
				}

				if (isLocalTransaction)
					await transactionContext.CommitAsync(cancellationToken);
			}
			catch (Exception ex)
			{
				await _baseComponentLogger.LogErrorAsync(traceInfo, idComponent, componentStatus, messageBuilder, detail, transactionContext, cancellationToken);
				await _baseComponentLogger.LogCriticalAsync(traceInfo, idComponent, componentStatus, x => x.ExceptionInfo(ex), detail, transactionContext, cancellationToken);
				throw;
			}
		}

		public async Task LogCriticalAsync(
			ITraceInfo traceInfo,
			Guid idComponent,
			ComponentStatus componentStatus,
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

				var dbComponentLog = new DbComponentLog
				{
					IdComponent = idComponent,
					IdLogLevel = (int)LogLevel.Critical,
					RuntimeUniqueKey = EnvironmentInfo.RUNTIME_UNIQUE_KEY,
					TimeCreatedUtc = DateTime.UtcNow,
					IdComponentStatus = (int)componentStatus,
					LogMessage = message,
					LogDetail = detail
				};

				var sql = DbComponentLog.GetInsertSql(_options);

				using var cmd = new NpgsqlCommand(sql, connection);
				if (transaction != null)
					cmd.Transaction = transaction;

				var table = DbComponentLog.GetDictionaryTable(_options);
				table.SetParameters(cmd, dbComponentLog.ToDictionary(_serialzier));

				var result = await cmd.ExecuteNonQueryAsync(cancellationToken);

				if (result != 1)
					throw new InvalidOperationException($"{nameof(LogCriticalAsync)}: {nameof(cmd.ExecuteNonQueryAsync)} returns {result} | {nameof(idComponent)} = {idComponent}");

				if (componentStatus != ComponentStatus.Unchanged)
				{
					try
					{
						await UpdateComponentStatusAsync(idComponent, componentStatus, transactionContext, cancellationToken);
					}
					catch (Exception ex)
					{
						await _baseComponentLogger.LogErrorAsync(traceInfo, idComponent, componentStatus, x => x.ExceptionInfo(ex), detail, transactionContext, cancellationToken);
					}
				}

				if (isLocalTransaction)
					await transactionContext.CommitAsync(cancellationToken);
			}
			catch (Exception ex)
			{
				await _baseComponentLogger.LogCriticalAsync(traceInfo, idComponent, componentStatus, messageBuilder, detail, transactionContext, cancellationToken);
				await _baseComponentLogger.LogCriticalAsync(traceInfo, idComponent, componentStatus, x => x.ExceptionInfo(ex), detail, transactionContext, cancellationToken);
				throw;
			}
		}
	}
}

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql;
using NpgsqlTypes;
using Raider.Infrastructure;
using Raider.Logging;
using Raider.Serializer;
using Raider.ServiceBus.BusLogger;
using Raider.ServiceBus.Components;
using Raider.ServiceBus.Messages;
using Raider.ServiceBus.Model;
using Raider.ServiceBus.PostgreSql.Messages.Storage.Model;
using Raider.ServiceBus.Resolver;
using Raider.Trace;
using Raider.Transactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Raider.ServiceBus.PostgreSql.Storage
{
	internal partial class PostgreSqlServiceBusStorage : PostgreSqlHostStorage, IMessageLogger
	{
		public async Task LogTraceAsync(
			ITraceInfo traceInfo,
			Guid idMessage,
			Guid? idComponent,
			MessageStatus messageStatus,
			Action<LogMessageBuilder> messageBuilder,
			string? detail = null,
			int? retryCount = null,
			DateTime? delayedToUtc = null,
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
						await UpdateMessageStatusAsync(idMessage, messageStatus, transactionContext, cancellationToken);
					}
					catch (Exception ex)
					{
						await _baseMessageLogger.LogErrorAsync(traceInfo, idMessage, idComponent, messageStatus, x => x.ExceptionInfo(ex), detail, null, null, transactionContext, cancellationToken);
					}
				}

				if (isLocalTransaction)
					await transactionContext.CommitAsync(cancellationToken);
			}
			catch (Exception ex)
			{
				await _baseMessageLogger.LogTraceAsync(traceInfo, idMessage, idComponent, messageStatus, messageBuilder, detail, null, null, transactionContext, cancellationToken);
				await _baseMessageLogger.LogCriticalAsync(traceInfo, idMessage, idComponent, messageStatus, x => x.ExceptionInfo(ex), detail, null, null, transactionContext, cancellationToken);
				throw;
			}
		}

		public async Task LogDebugAsync(
			ITraceInfo traceInfo,
			Guid idMessage,
			Guid? idComponent,
			MessageStatus messageStatus,
			Action<LogMessageBuilder> messageBuilder,
			string? detail = null,
			int? retryCount = null,
			DateTime? delayedToUtc = null,
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
						await UpdateMessageStatusAsync(idMessage, messageStatus, transactionContext, cancellationToken);
					}
					catch (Exception ex)
					{
						await _baseMessageLogger.LogErrorAsync(traceInfo, idMessage, idComponent, messageStatus, x => x.ExceptionInfo(ex), detail, null, null, transactionContext, cancellationToken);
					}
				}

				if (isLocalTransaction)
					await transactionContext.CommitAsync(cancellationToken);
			}
			catch (Exception ex)
			{
				await _baseMessageLogger.LogDebugAsync(traceInfo, idMessage, idComponent, messageStatus, messageBuilder, detail, null, null, transactionContext, cancellationToken);
				await _baseMessageLogger.LogCriticalAsync(traceInfo, idMessage, idComponent, messageStatus, x => x.ExceptionInfo(ex), detail, null, null, transactionContext, cancellationToken);
				throw;
			}
		}

		public async Task LogInformationAsync(
			ITraceInfo traceInfo,
			Guid idMessage,
			Guid? idComponent,
			MessageStatus messageStatus,
			Action<LogMessageBuilder> messageBuilder,
			string? detail = null,
			int? retryCount = null,
			DateTime? delayedToUtc = null,
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
						await UpdateMessageStatusAsync(idMessage, messageStatus, transactionContext, cancellationToken);
					}
					catch (Exception ex)
					{
						await _baseMessageLogger.LogErrorAsync(traceInfo, idMessage, idComponent, messageStatus, x => x.ExceptionInfo(ex), detail, null, null, transactionContext, cancellationToken);
					}
				}

				if (isLocalTransaction)
					await transactionContext.CommitAsync(cancellationToken);
			}
			catch (Exception ex)
			{
				await _baseMessageLogger.LogInformationAsync(traceInfo, idMessage, idComponent, messageStatus, messageBuilder, detail, null, null, transactionContext, cancellationToken);
				await _baseMessageLogger.LogCriticalAsync(traceInfo, idMessage, idComponent, messageStatus, x => x.ExceptionInfo(ex), detail, null, null, transactionContext, cancellationToken);
				throw;
			}
		}

		public async Task LogWarningAsync(
			ITraceInfo traceInfo,
			Guid idMessage,
			Guid? idComponent,
			MessageStatus messageStatus,
			Action<LogMessageBuilder> messageBuilder,
			string? detail = null,
			int? retryCount = null,
			DateTime? delayedToUtc = null,
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
						await UpdateMessageStatusAsync(idMessage, messageStatus, transactionContext, cancellationToken);
					}
					catch (Exception ex)
					{
						await _baseMessageLogger.LogErrorAsync(traceInfo, idMessage, idComponent, messageStatus, x => x.ExceptionInfo(ex), detail, null, null, transactionContext, cancellationToken);
					}
				}

				if (isLocalTransaction)
					await transactionContext.CommitAsync(cancellationToken);
			}
			catch (Exception ex)
			{
				await _baseMessageLogger.LogWarningAsync(traceInfo, idMessage, idComponent, messageStatus, messageBuilder, detail, null, null, transactionContext, cancellationToken);
				await _baseMessageLogger.LogCriticalAsync(traceInfo, idMessage, idComponent, messageStatus, x => x.ExceptionInfo(ex), detail, null, null, transactionContext, cancellationToken);
				throw;
			}
		}

		public async Task LogErrorAsync(
			ITraceInfo traceInfo,
			Guid idMessage,
			Guid? idComponent,
			MessageStatus messageStatus,
			Action<ErrorMessageBuilder> messageBuilder,
			string? detail = null,
			int? retryCount = null,
			DateTime? delayedToUtc = null,
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
						await UpdateMessageStatusAsync(idMessage, messageStatus, transactionContext, cancellationToken);
					}
					catch (Exception ex)
					{
						await _baseMessageLogger.LogErrorAsync(traceInfo, idMessage, idComponent, messageStatus, x => x.ExceptionInfo(ex), detail, null, null, transactionContext, cancellationToken);
					}
				}

				if (isLocalTransaction)
					await transactionContext.CommitAsync(cancellationToken);
			}
			catch (Exception ex)
			{
				await _baseMessageLogger.LogErrorAsync(traceInfo, idMessage, idComponent, messageStatus, messageBuilder, detail, null, null, transactionContext, cancellationToken);
				await _baseMessageLogger.LogCriticalAsync(traceInfo, idMessage, idComponent, messageStatus, x => x.ExceptionInfo(ex), detail, null, null, transactionContext, cancellationToken);
				throw;
			}
		}

		public async Task LogCriticalAsync(
			ITraceInfo traceInfo,
			Guid idMessage,
			Guid? idComponent,
			MessageStatus messageStatus,
			Action<ErrorMessageBuilder> messageBuilder,
			string? detail = null,
			int? retryCount = null,
			DateTime? delayedToUtc = null,
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
						await UpdateMessageStatusAsync(idMessage, messageStatus, transactionContext, cancellationToken);
					}
					catch (Exception ex)
					{
						await _baseMessageLogger.LogErrorAsync(traceInfo, idMessage, idComponent, messageStatus, x => x.ExceptionInfo(ex), detail, null, null, transactionContext, cancellationToken);
					}
				}

				if (isLocalTransaction)
					await transactionContext.CommitAsync(cancellationToken);
			}
			catch (Exception ex)
			{
				await _baseMessageLogger.LogCriticalAsync(traceInfo, idMessage, idComponent, messageStatus, messageBuilder, detail, null, null, transactionContext, cancellationToken);
				await _baseMessageLogger.LogCriticalAsync(traceInfo, idMessage, idComponent, messageStatus, x => x.ExceptionInfo(ex), detail, null, null, transactionContext, cancellationToken);
				throw;
			}
		}
	}
}

using Microsoft.Extensions.Logging;
using Raider.Logging;
using Raider.Logging.Extensions;
using Raider.ServiceBus.Messages;
using Raider.Trace;
using Raider.Transactions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Raider.ServiceBus.BusLogger
{
	public class BaseHandlerMessageLogger : IHandlerMessageLogger
	{
		private readonly ILogger _logger;

		public BaseHandlerMessageLogger(ILogger<BaseHandlerMessageLogger> logger)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		private static Action<LogMessageBuilder> AppendToBuilder(
			Action<LogMessageBuilder> messageBuilder,
			Guid idMessage,
			MessageStatus messageStatus,
			string? detail)
		{
			messageBuilder += x => x
				.AddCustomData(nameof(idMessage), idMessage.ToString())
				.AddCustomData(nameof(messageStatus), ((int)messageStatus).ToString());

			if (!string.IsNullOrWhiteSpace(detail))
				messageBuilder +=
					x => x.AddCustomData(nameof(detail), detail);

			return messageBuilder;
		}

		private static Action<ErrorMessageBuilder> AppendToBuilder(
			Action<ErrorMessageBuilder> messageBuilder,
			Guid idMessage,
			MessageStatus messageStatus,
			string? detail)
		{
			messageBuilder += x => x
				.AddCustomData(nameof(idMessage), idMessage.ToString())
				.AddCustomData(nameof(messageStatus), ((int)messageStatus).ToString());

			if (!string.IsNullOrWhiteSpace(detail))
				messageBuilder +=
					x => x.AddCustomData(nameof(detail), detail);

			return messageBuilder;
		}

		public void LogTrace(
			ITraceInfo traceInfo,
			Guid idMessage,
			MessageStatus messageStatus,
			Action<LogMessageBuilder> messageBuilder,
			string? detail = null,
			ITransactionContext? transactionContext = null)
		{
			AppendToBuilder(messageBuilder, idMessage, messageStatus, detail);
			_logger.LogTraceMessage(traceInfo, messageBuilder);
		}

		public void LogDebug(
			ITraceInfo traceInfo,
			Guid idMessage,
			MessageStatus messageStatus,
			Action<LogMessageBuilder> messageBuilder,
			string? detail = null,
			ITransactionContext? transactionContext = null)
		{
			AppendToBuilder(messageBuilder, idMessage, messageStatus, detail);
			_logger.LogDebugMessage(traceInfo, messageBuilder);
		}

		public void LogInformation(
			ITraceInfo traceInfo,
			Guid idMessage,
			MessageStatus messageStatus,
			Action<LogMessageBuilder> messageBuilder,
			string? detail = null,
			ITransactionContext? transactionContext = null)
		{
			AppendToBuilder(messageBuilder, idMessage, messageStatus, detail);
			_logger.LogInformationMessage(traceInfo, messageBuilder);
		}

		public void LogWarning(
			ITraceInfo traceInfo,
			Guid idMessage,
			MessageStatus messageStatus,
			Action<LogMessageBuilder> messageBuilder,
			string? detail = null,
			ITransactionContext? transactionContext = null)
		{
			AppendToBuilder(messageBuilder, idMessage, messageStatus, detail);
			_logger.LogWarningMessage(traceInfo, messageBuilder);
		}

		public void LogError(
			ITraceInfo traceInfo,
			Guid idMessage,
			MessageStatus messageStatus,
			Action<ErrorMessageBuilder> messageBuilder,
			string? detail = null,
			ITransactionContext? transactionContext = null)
		{
			AppendToBuilder(messageBuilder, idMessage, messageStatus, detail);
			_logger.LogErrorMessage(traceInfo, messageBuilder);
		}

		public void LogCritical(
			ITraceInfo traceInfo,
			Guid idMessage,
			MessageStatus messageStatus,
			Action<ErrorMessageBuilder> messageBuilder,
			string? detail = null,
			ITransactionContext? transactionContext = null)
		{
			AppendToBuilder(messageBuilder, idMessage, messageStatus, detail);
			_logger.LogCriticalMessage(traceInfo, messageBuilder);
		}

		public Task LogTraceAsync(
			ITraceInfo traceInfo,
			Guid idMessage,
			MessageStatus messageStatus,
			Action<LogMessageBuilder> messageBuilder,
			string? detail = null,
			ITransactionContext? transactionContext = null,
			CancellationToken cancellationToken = default)
		{
			AppendToBuilder(messageBuilder, idMessage, messageStatus, detail);
			_logger.LogTraceMessage(traceInfo, messageBuilder);
			return Task.CompletedTask;
		}

		public Task LogDebugAsync(
			ITraceInfo traceInfo,
			Guid idMessage,
			MessageStatus messageStatus,
			Action<LogMessageBuilder> messageBuilder,
			string? detail = null,
			ITransactionContext? transactionContext = null,
			CancellationToken cancellationToken = default)
		{
			AppendToBuilder(messageBuilder, idMessage, messageStatus, detail);
			_logger.LogDebugMessage(traceInfo, messageBuilder);
			return Task.CompletedTask;
		}

		public Task LogInformationAsync(
			ITraceInfo traceInfo,
			Guid idMessage,
			MessageStatus messageStatus,
			Action<LogMessageBuilder> messageBuilder,
			string? detail = null,
			ITransactionContext? transactionContext = null,
			CancellationToken cancellationToken = default)
		{
			AppendToBuilder(messageBuilder, idMessage, messageStatus, detail);
			_logger.LogInformationMessage(traceInfo, messageBuilder);
			return Task.CompletedTask;
		}

		public Task LogWarningAsync(
			ITraceInfo traceInfo,
			Guid idMessage,
			MessageStatus messageStatus,
			Action<LogMessageBuilder> messageBuilder,
			string? detail = null,
			ITransactionContext? transactionContext = null,
			CancellationToken cancellationToken = default)
		{
			AppendToBuilder(messageBuilder, idMessage, messageStatus, detail);
			_logger.LogWarningMessage(traceInfo, messageBuilder);
			return Task.CompletedTask;
		}

		public Task LogErrorAsync(
			ITraceInfo traceInfo,
			Guid idMessage,
			MessageStatus messageStatus,
			Action<ErrorMessageBuilder> messageBuilder,
			string? detail = null,
			ITransactionContext? transactionContext = null,
			CancellationToken cancellationToken = default)
		{
			AppendToBuilder(messageBuilder, idMessage, messageStatus, detail);
			_logger.LogErrorMessage(traceInfo, messageBuilder);
			return Task.CompletedTask;
		}

		public Task LogCriticalAsync(
			ITraceInfo traceInfo,
			Guid idMessage,
			MessageStatus messageStatus,
			Action<ErrorMessageBuilder> messageBuilder,
			string? detail = null,
			ITransactionContext? transactionContext = null,
			CancellationToken cancellationToken = default)
		{
			AppendToBuilder(messageBuilder, idMessage, messageStatus, detail);
			_logger.LogCriticalMessage(traceInfo, messageBuilder);
			return Task.CompletedTask;
		}
	}
}

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
	public class BaseMessageLogger : IMessageLogger
	{
		private readonly ILogger _logger;

		public BaseMessageLogger(ILogger<BaseMessageLogger> logger)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		private static Action<LogMessageBuilder> AppendToBuilder(Action<LogMessageBuilder> messageBuilder, Guid idMessage, Guid? idComponent, MessageStatus messageStatus, string? detail, int? retryCount, DateTime? delayedToUtc)
		{
			messageBuilder += x => x
				.AddCustomData(nameof(idMessage), idMessage.ToString())
				.AddCustomData(nameof(messageStatus), ((int)messageStatus).ToString());

			if (idComponent.HasValue)
				messageBuilder += x => x
				.AddCustomData(nameof(idComponent), idComponent.Value.ToString());

			if (!string.IsNullOrWhiteSpace(detail))
				messageBuilder +=
					x => x.AddCustomData(nameof(detail), detail);

			if (retryCount.HasValue)
					messageBuilder += x => x
					.AddCustomData(nameof(retryCount), retryCount.Value.ToString());

			if (delayedToUtc.HasValue)
				messageBuilder +=
					x => x.AddCustomData(nameof(delayedToUtc), delayedToUtc.Value.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"));

			return messageBuilder;
		}

		private static Action<ErrorMessageBuilder> AppendToBuilder(Action<ErrorMessageBuilder> messageBuilder, Guid idMessage, Guid? idComponent, MessageStatus messageStatus, string? detail, int? retryCount, DateTime? delayedToUtc)
		{
			messageBuilder += x => x
				.AddCustomData(nameof(idMessage), idMessage.ToString())
				.AddCustomData(nameof(messageStatus), ((int)messageStatus).ToString());

			if (idComponent.HasValue)
				messageBuilder += x => x
				.AddCustomData(nameof(idComponent), idComponent.Value.ToString());

			if (!string.IsNullOrWhiteSpace(detail))
				messageBuilder +=
					x => x.AddCustomData(nameof(detail), detail);

			if (retryCount.HasValue)
					messageBuilder += x => x
					.AddCustomData(nameof(retryCount), retryCount.Value.ToString());

			if (delayedToUtc.HasValue)
				messageBuilder +=
					x => x.AddCustomData(nameof(delayedToUtc), delayedToUtc.Value.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"));

			return messageBuilder;
		}

		public Task LogTraceAsync(
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
			AppendToBuilder(messageBuilder, idMessage, idComponent, messageStatus, detail, retryCount, delayedToUtc);
			_logger.LogTraceMessage(traceInfo, messageBuilder);
			return Task.CompletedTask;
		}

		public Task LogDebugAsync(
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
			AppendToBuilder(messageBuilder, idMessage, idComponent, messageStatus, detail, retryCount, delayedToUtc);
			_logger.LogDebugMessage(traceInfo, messageBuilder);
			return Task.CompletedTask;
		}

		public Task LogInformationAsync(
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
			AppendToBuilder(messageBuilder, idMessage, idComponent, messageStatus, detail, retryCount, delayedToUtc);
			_logger.LogInformationMessage(traceInfo, messageBuilder);
			return Task.CompletedTask;
		}

		public Task LogWarningAsync(
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
			AppendToBuilder(messageBuilder, idMessage, idComponent, messageStatus, detail, retryCount, delayedToUtc);
			_logger.LogWarningMessage(traceInfo, messageBuilder);
			return Task.CompletedTask;
		}

		public Task LogErrorAsync(
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
			AppendToBuilder(messageBuilder, idMessage, idComponent, messageStatus, detail, retryCount, delayedToUtc);
			_logger.LogErrorMessage(traceInfo, messageBuilder);
			return Task.CompletedTask;
		}

		public Task LogCriticalAsync(
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
			AppendToBuilder(messageBuilder, idMessage, idComponent, messageStatus, detail, retryCount, delayedToUtc);
			_logger.LogCriticalMessage(traceInfo, messageBuilder);
			return Task.CompletedTask;
		}
	}
}

using Microsoft.Extensions.Logging;
using Raider.Logging;
using Raider.Logging.Extensions;
using Raider.Trace;
using Raider.Transactions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Raider.ServiceBus.BusLogger
{
	public class BaseHostLogger : IHostLogger
	{
		private readonly ILogger _logger;

		public BaseHostLogger(ILogger<BaseHostLogger> logger)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		private static Action<LogMessageBuilder> AppendToBuilder(Action<LogMessageBuilder> messageBuilder, Guid idHost, HostStatus hostStatus, string? detail)
		{
			messageBuilder += x => x
				.AddCustomData(nameof(idHost), idHost.ToString())
				.AddCustomData(nameof(hostStatus), ((int)hostStatus).ToString());

			if (!string.IsNullOrWhiteSpace(detail))
				messageBuilder +=
					x => x.AddCustomData(nameof(detail), detail);

			return messageBuilder;
		}

		private static Action<ErrorMessageBuilder> AppendToBuilder(Action<ErrorMessageBuilder> messageBuilder, Guid idHost, HostStatus hostStatus, string? detail)
		{
			messageBuilder += x => x
				.AddCustomData(nameof(idHost), idHost.ToString())
				.AddCustomData(nameof(hostStatus), ((int)hostStatus).ToString());

			if (!string.IsNullOrWhiteSpace(detail))
				messageBuilder +=
					x => x.AddCustomData(nameof(detail), detail);

			return messageBuilder;
		}

		public void LogTrace(
			ITraceInfo traceInfo,
			Guid idHost,
			HostStatus hostStatus,
			Action<LogMessageBuilder> messageBuilder,
			string? detail = null,
			ITransactionContext? transactionContext = null)
		{
			AppendToBuilder(messageBuilder, idHost, hostStatus, detail);
			_logger.LogTraceMessage(traceInfo, messageBuilder);
		}

		public void LogDebug(
			ITraceInfo traceInfo,
			Guid idHost,
			HostStatus hostStatus,
			Action<LogMessageBuilder> messageBuilder,
			string? detail = null,
			ITransactionContext? transactionContext = null)
		{
			AppendToBuilder(messageBuilder, idHost, hostStatus, detail);
			_logger.LogDebugMessage(traceInfo, messageBuilder);
		}

		public void LogInformation(
			ITraceInfo traceInfo,
			Guid idHost,
			HostStatus hostStatus,
			Action<LogMessageBuilder> messageBuilder,
			string? detail = null,
			ITransactionContext? transactionContext = null)
		{
			AppendToBuilder(messageBuilder, idHost, hostStatus, detail);
			_logger.LogInformationMessage(traceInfo, messageBuilder);
		}

		public void LogWarning(
			ITraceInfo traceInfo,
			Guid idHost,
			HostStatus hostStatus,
			Action<LogMessageBuilder> messageBuilder,
			string? detail = null,
			ITransactionContext? transactionContext = null)
		{
			AppendToBuilder(messageBuilder, idHost, hostStatus, detail);
			_logger.LogWarningMessage(traceInfo, messageBuilder);
		}

		public void LogError(
			ITraceInfo traceInfo,
			Guid idHost,
			HostStatus hostStatus,
			Action<ErrorMessageBuilder> messageBuilder,
			string? detail = null,
			ITransactionContext? transactionContext = null)
		{
			AppendToBuilder(messageBuilder, idHost, hostStatus, detail);
			_logger.LogErrorMessage(traceInfo, messageBuilder);
		}

		public void LogCritical(
			ITraceInfo traceInfo,
			Guid idHost,
			HostStatus hostStatus,
			Action<ErrorMessageBuilder> messageBuilder,
			string? detail = null,
			ITransactionContext? transactionContext = null)
		{
			AppendToBuilder(messageBuilder, idHost, hostStatus, detail);
			_logger.LogCriticalMessage(traceInfo, messageBuilder);
		}

		public Task LogTraceAsync(
			ITraceInfo traceInfo,
			Guid idHost,
			HostStatus hostStatus,
			Action<LogMessageBuilder> messageBuilder,
			string? detail = null,
			ITransactionContext? transactionContext = null,
			CancellationToken cancellationToken = default)
		{
			AppendToBuilder(messageBuilder, idHost, hostStatus, detail);
			_logger.LogTraceMessage(traceInfo, messageBuilder);
			return Task.CompletedTask;
		}

		public Task LogDebugAsync(
			ITraceInfo traceInfo,
			Guid idHost,
			HostStatus hostStatus,
			Action<LogMessageBuilder> messageBuilder,
			string? detail = null,
			ITransactionContext? transactionContext = null,
			CancellationToken cancellationToken = default)
		{
			AppendToBuilder(messageBuilder, idHost, hostStatus, detail);
			_logger.LogDebugMessage(traceInfo, messageBuilder);
			return Task.CompletedTask;
		}

		public Task LogInformationAsync(
			ITraceInfo traceInfo,
			Guid idHost,
			HostStatus hostStatus,
			Action<LogMessageBuilder> messageBuilder,
			string? detail = null,
			ITransactionContext? transactionContext = null,
			CancellationToken cancellationToken = default)
		{
			AppendToBuilder(messageBuilder, idHost, hostStatus, detail);
			_logger.LogInformationMessage(traceInfo, messageBuilder);
			return Task.CompletedTask;
		}

		public Task LogWarningAsync(
			ITraceInfo traceInfo,
			Guid idHost,
			HostStatus hostStatus,
			Action<LogMessageBuilder> messageBuilder,
			string? detail = null,
			ITransactionContext? transactionContext = null,
			CancellationToken cancellationToken = default)
		{
			AppendToBuilder(messageBuilder, idHost, hostStatus, detail);
			_logger.LogWarningMessage(traceInfo, messageBuilder);
			return Task.CompletedTask;
		}

		public Task LogErrorAsync(
			ITraceInfo traceInfo,
			Guid idHost,
			HostStatus hostStatus,
			Action<ErrorMessageBuilder> messageBuilder,
			string? detail = null,
			ITransactionContext? transactionContext = null,
			CancellationToken cancellationToken = default)
		{
			AppendToBuilder(messageBuilder, idHost, hostStatus, detail);
			_logger.LogErrorMessage(traceInfo, messageBuilder);
			return Task.CompletedTask;
		}

		public Task LogCriticalAsync(
			ITraceInfo traceInfo,
			Guid idHost,
			HostStatus hostStatus,
			Action<ErrorMessageBuilder> messageBuilder,
			string? detail = null,
			ITransactionContext? transactionContext = null,
			CancellationToken cancellationToken = default)
		{
			AppendToBuilder(messageBuilder, idHost, hostStatus, detail);
			_logger.LogCriticalMessage(traceInfo, messageBuilder);
			return Task.CompletedTask;
		}
	}
}

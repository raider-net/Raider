using Microsoft.Extensions.Logging;
using Raider.Logging;
using Raider.Logging.Extensions;
using Raider.ServiceBus.Components;
using Raider.Trace;
using Raider.Transactions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Raider.ServiceBus.BusLogger
{
	public class BaseComponentLogger : IComponentLogger
	{
		private readonly ILogger _logger;

		public BaseComponentLogger(ILogger<BaseComponentLogger> logger)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		private static Action<LogMessageBuilder> AppendToBuilder(Action<LogMessageBuilder> messageBuilder, Guid idComponent, ComponentStatus componentStatus, string? detail)
		{
			messageBuilder += x => x
				.AddCustomData(nameof(idComponent), idComponent.ToString())
				.AddCustomData(nameof(componentStatus), ((int)componentStatus).ToString());

			if (!string.IsNullOrWhiteSpace(detail))
				messageBuilder +=
					x => x.AddCustomData(nameof(detail), detail);

			return messageBuilder;
		}

		private static Action<ErrorMessageBuilder> AppendToBuilder(Action<ErrorMessageBuilder> messageBuilder, Guid idComponent, ComponentStatus componentStatus, string? detail)
		{
			messageBuilder += x => x
				.AddCustomData(nameof(idComponent), idComponent.ToString())
				.AddCustomData(nameof(componentStatus), ((int)componentStatus).ToString());

			if (!string.IsNullOrWhiteSpace(detail))
				messageBuilder +=
					x => x.AddCustomData(nameof(detail), detail);

			return messageBuilder;
		}

		public Task LogTraceAsync(
			ITraceInfo traceInfo,
			Guid idComponent,
			ComponentStatus componentStatus,
			Action<LogMessageBuilder> messageBuilder,
			string? detail = null,
			ITransactionContext? transactionContext = null,
			CancellationToken cancellationToken = default)
		{
			AppendToBuilder(messageBuilder, idComponent, componentStatus, detail);
			_logger.LogTraceMessage(traceInfo, messageBuilder);
			return Task.CompletedTask;
		}

		public Task LogDebugAsync(
			ITraceInfo traceInfo,
			Guid idComponent,
			ComponentStatus componentStatus,
			Action<LogMessageBuilder> messageBuilder,
			string? detail = null,
			ITransactionContext? transactionContext = null,
			CancellationToken cancellationToken = default)
		{
			AppendToBuilder(messageBuilder, idComponent, componentStatus, detail);
			_logger.LogDebugMessage(traceInfo, messageBuilder);
			return Task.CompletedTask;
		}

		public Task LogInformationAsync(
			ITraceInfo traceInfo,
			Guid idComponent,
			ComponentStatus componentStatus,
			Action<LogMessageBuilder> messageBuilder,
			string? detail = null,
			ITransactionContext? transactionContext = null,
			CancellationToken cancellationToken = default)
		{
			AppendToBuilder(messageBuilder, idComponent, componentStatus, detail);
			_logger.LogInformationMessage(traceInfo, messageBuilder);
			return Task.CompletedTask;
		}

		public Task LogWarningAsync(
			ITraceInfo traceInfo,
			Guid idComponent,
			ComponentStatus componentStatus,
			Action<LogMessageBuilder> messageBuilder,
			string? detail = null,
			ITransactionContext? transactionContext = null,
			CancellationToken cancellationToken = default)
		{
			AppendToBuilder(messageBuilder, idComponent, componentStatus, detail);
			_logger.LogWarningMessage(traceInfo, messageBuilder);
			return Task.CompletedTask;
		}

		public Task LogErrorAsync(
			ITraceInfo traceInfo,
			Guid idComponent,
			ComponentStatus componentStatus,
			Action<ErrorMessageBuilder> messageBuilder,
			string? detail = null,
			ITransactionContext? transactionContext = null,
			CancellationToken cancellationToken = default)
		{
			AppendToBuilder(messageBuilder, idComponent, componentStatus, detail);
			_logger.LogErrorMessage(traceInfo, messageBuilder);
			return Task.CompletedTask;
		}

		public Task LogCriticalAsync(
			ITraceInfo traceInfo,
			Guid idComponent,
			ComponentStatus componentStatus,
			Action<ErrorMessageBuilder> messageBuilder,
			string? detail = null,
			ITransactionContext? transactionContext = null,
			CancellationToken cancellationToken = default)
		{
			AppendToBuilder(messageBuilder, idComponent, componentStatus, detail);
			_logger.LogCriticalMessage(traceInfo, messageBuilder);
			return Task.CompletedTask;
		}
	}
}

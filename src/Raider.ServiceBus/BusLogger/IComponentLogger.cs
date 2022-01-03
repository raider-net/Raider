using Raider.Logging;
using Raider.ServiceBus.Components;
using Raider.Trace;
using Raider.Transactions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Raider.ServiceBus.BusLogger
{
	public interface IComponentLogger
	{
		Task LogTraceAsync(
			ITraceInfo traceInfo,
			Guid idComponent,
			ComponentStatus componentStatus,
			Action<LogMessageBuilder> messageBuilder,
			string? detail = null,
			ITransactionContext? transactionContext = null,
			CancellationToken cancellationToken = default);

		Task LogDebugAsync(
			ITraceInfo traceInfo,
			Guid idComponent,
			ComponentStatus componentStatus,
			Action<LogMessageBuilder> messageBuilder,
			string? detail = null,
			ITransactionContext? transactionContext = null,
			CancellationToken cancellationToken = default);

		Task LogInformationAsync(
			ITraceInfo traceInfo,
			Guid idComponent,
			ComponentStatus componentStatus,
			Action<LogMessageBuilder> messageBuilder,
			string? detail = null,
			ITransactionContext? transactionContext = null,
			CancellationToken cancellationToken = default);

		Task LogWarningAsync(
			ITraceInfo traceInfo,
			Guid idComponent,
			ComponentStatus componentStatus,
			Action<LogMessageBuilder> messageBuilder,
			string? detail = null,
			ITransactionContext? transactionContext = null,
			CancellationToken cancellationToken = default);

		Task LogErrorAsync(
			ITraceInfo traceInfo,
			Guid idComponent,
			ComponentStatus componentStatus,
			Action<ErrorMessageBuilder> messageBuilder,
			string? detail = null,
			ITransactionContext? transactionContext = null,
			CancellationToken cancellationToken = default);

		Task LogCriticalAsync(
			ITraceInfo traceInfo,
			Guid idComponent,
			ComponentStatus componentStatus,
			Action<ErrorMessageBuilder> messageBuilder,
			string? detail = null,
			ITransactionContext? transactionContext = null,
			CancellationToken cancellationToken = default);
	}
}

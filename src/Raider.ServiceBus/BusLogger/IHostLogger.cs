using Raider.Logging;
using Raider.Trace;
using Raider.Transactions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Raider.ServiceBus.BusLogger
{
	public interface IHostLogger
	{
		void LogTrace(
			ITraceInfo traceInfo,
			Guid idHost,
			HostStatus hostStatus,
			Action<LogMessageBuilder> messageBuilder,
			string? detail = null,
			ITransactionContext? transactionContext = null);

		void LogDebug(
			ITraceInfo traceInfo,
			Guid idHost,
			HostStatus hostStatus,
			Action<LogMessageBuilder> messageBuilder,
			string? detail = null,
			ITransactionContext? transactionContext = null);

		void LogInformation(
			ITraceInfo traceInfo,
			Guid idHost,
			HostStatus hostStatus,
			Action<LogMessageBuilder> messageBuilder,
			string? detail = null,
			ITransactionContext? transactionContext = null);

		void LogWarning(
			ITraceInfo traceInfo,
			Guid idHost,
			HostStatus hostStatus,
			Action<LogMessageBuilder> messageBuilder,
			string? detail = null,
			ITransactionContext? transactionContext = null);

		void LogError(
			ITraceInfo traceInfo,
			Guid idHost,
			HostStatus hostStatus,
			Action<ErrorMessageBuilder> messageBuilder,
			string? detail = null,
			ITransactionContext? transactionContext = null);

		void LogCritical(
			ITraceInfo traceInfo,
			Guid idHost,
			HostStatus hostStatus,
			Action<ErrorMessageBuilder> messageBuilder,
			string? detail = null,
			ITransactionContext? transactionContext = null);

		Task LogTraceAsync(
			ITraceInfo traceInfo,
			Guid idHost,
			HostStatus hostStatus,
			Action<LogMessageBuilder> messageBuilder,
			string? detail = null,
			ITransactionContext? transactionContext = null,
			CancellationToken cancellationToken = default);

		Task LogDebugAsync(
			ITraceInfo traceInfo,
			Guid idHost,
			HostStatus hostStatus,
			Action<LogMessageBuilder> messageBuilder,
			string? detail = null,
			ITransactionContext? transactionContext = null,
			CancellationToken cancellationToken = default);

		Task LogInformationAsync(
			ITraceInfo traceInfo,
			Guid idHost,
			HostStatus hostStatus,
			Action<LogMessageBuilder> messageBuilder,
			string? detail = null,
			ITransactionContext? transactionContext = null,
			CancellationToken cancellationToken = default);

		Task LogWarningAsync(
			ITraceInfo traceInfo,
			Guid idHost,
			HostStatus hostStatus,
			Action<LogMessageBuilder> messageBuilder,
			string? detail = null,
			ITransactionContext? transactionContext = null,
			CancellationToken cancellationToken = default);

		Task LogErrorAsync(
			ITraceInfo traceInfo,
			Guid idHost,
			HostStatus hostStatus,
			Action<ErrorMessageBuilder> messageBuilder,
			string? detail = null,
			ITransactionContext? transactionContext = null,
			CancellationToken cancellationToken = default);

		Task LogCriticalAsync(
			ITraceInfo traceInfo,
			Guid idHost,
			HostStatus hostStatus,
			Action<ErrorMessageBuilder> messageBuilder,
			string? detail = null,
			ITransactionContext? transactionContext = null,
			CancellationToken cancellationToken = default);
	}
}

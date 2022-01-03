using Raider.Logging;
using Raider.ServiceBus.Messages;
using Raider.Trace;
using Raider.Transactions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Raider.ServiceBus.BusLogger
{
	public interface IMessageLogger
	{
		Task LogTraceAsync(
			ITraceInfo traceInfo,
			Guid idMessage,
			Guid? idComponent,
			MessageStatus messageStatus,
			Action<LogMessageBuilder> messageBuilder,
			string? detail = null,
			int? retryCount = null,
			DateTime? delayedToUtc = null,
			ITransactionContext? transactionContext = null,
			CancellationToken cancellationToken = default);

		Task LogDebugAsync(
			ITraceInfo traceInfo,
			Guid idMessage,
			Guid? idComponent,
			MessageStatus messageStatus,
			Action<LogMessageBuilder> messageBuilder,
			string? detail = null,
			int? retryCount = null,
			DateTime? delayedToUtc = null,
			ITransactionContext? transactionContext = null,
			CancellationToken cancellationToken = default);

		Task LogInformationAsync(
			ITraceInfo traceInfo,
			Guid idMessage,
			Guid? idComponent,
			MessageStatus messageStatus,
			Action<LogMessageBuilder> messageBuilder,
			string? detail = null,
			int? retryCount = null,
			DateTime? delayedToUtc = null,
			ITransactionContext? transactionContext = null,
			CancellationToken cancellationToken = default);

		Task LogWarningAsync(
			ITraceInfo traceInfo,
			Guid idMessage,
			Guid? idComponent,
			MessageStatus messageStatus,
			Action<LogMessageBuilder> messageBuilder,
			string? detail = null,
			int? retryCount = null,
			DateTime? delayedToUtc = null,
			ITransactionContext? transactionContext = null,
			CancellationToken cancellationToken = default);

		Task LogErrorAsync(
			ITraceInfo traceInfo,
			Guid idMessage,
			Guid? idComponent,
			MessageStatus messageStatus,
			Action<ErrorMessageBuilder> messageBuilder,
			string? detail = null,
			int? retryCount = null,
			DateTime? delayedToUtc = null,
			ITransactionContext? transactionContext = null,
			CancellationToken cancellationToken = default);

		Task LogCriticalAsync(
			ITraceInfo traceInfo,
			Guid idMessage,
			Guid? idComponent,
			MessageStatus messageStatus,
			Action<ErrorMessageBuilder> messageBuilder,
			string? detail = null,
			int? retryCount = null,
			DateTime? delayedToUtc = null,
			ITransactionContext? transactionContext = null,
			CancellationToken cancellationToken = default);
	}
}

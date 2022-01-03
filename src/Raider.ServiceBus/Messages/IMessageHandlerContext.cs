using Raider.Logging;
using Raider.Trace;
using Raider.Transactions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Raider.ServiceBus.Messages
{
	public interface IMessageHandlerContext
	{
		/// <summary>
		/// Current transaction context
		/// </summary>
		ITransactionContext TransactionContext { get; }

		/// <summary>
		/// Parent method scope
		/// </summary>
		ITraceInfo PreviousTraceInfo { get; }

		/// <summary>
		/// Current host id
		/// </summary>
		Guid IdHost { get; }

		/// <summary>
		/// Request message id
		/// </summary>
		Guid IdMessage { get; }

		/// <summary>
		/// Id of the session used for tracing messages
		/// </summary>
		Guid? IdSession { get; }

		/// <summary>
		/// The timespan after which the Send request will be cancelled if no response arrives.
		/// </summary>
		public TimeSpan? Timeout { get; }

		void LogTrace(
			ITraceInfo traceInfo,
			MessageStatus messageStatus,
			Action<LogMessageBuilder> messageBuilder,
			string? detail = null,
			ITransactionContext? transactionContext = null);

		void LogDebug(
			ITraceInfo traceInfo,
			MessageStatus messageStatus,
			Action<LogMessageBuilder> messageBuilder,
			string? detail = null,
			ITransactionContext? transactionContext = null);

		void LogInformation(
			ITraceInfo traceInfo,
			MessageStatus messageStatus,
			Action<LogMessageBuilder> messageBuilder,
			string? detail = null,
			ITransactionContext? transactionContext = null);

		void LogWarning(
			ITraceInfo traceInfo,
			MessageStatus messageStatus,
			Action<LogMessageBuilder> messageBuilder,
			string? detail = null,
			ITransactionContext? transactionContext = null);

		void LogError(
			ITraceInfo traceInfo,
			MessageStatus messageStatus,
			Action<ErrorMessageBuilder> messageBuilder,
			string? detail = null,
			ITransactionContext? transactionContext = null);

		void LogCritical(
			ITraceInfo traceInfo,
			MessageStatus messageStatus,
			Action<ErrorMessageBuilder> messageBuilder,
			string? detail = null,
			ITransactionContext? transactionContext = null);

		Task LogTraceAsync(
			ITraceInfo traceInfo,
			MessageStatus messageStatus,
			Action<LogMessageBuilder> messageBuilder,
			string? detail = null,
			ITransactionContext? transactionContext = null,
			CancellationToken cancellationToken = default);

		Task LogDebugAsync(
			ITraceInfo traceInfo,
			MessageStatus messageStatus,
			Action<LogMessageBuilder> messageBuilder,
			string? detail = null,
			ITransactionContext? transactionContext = null,
			CancellationToken cancellationToken = default);

		Task LogInformationAsync(
			ITraceInfo traceInfo,
			MessageStatus messageStatus,
			Action<LogMessageBuilder> messageBuilder,
			string? detail = null,
			ITransactionContext? transactionContext = null,
			CancellationToken cancellationToken = default);

		Task LogWarningAsync(
			ITraceInfo traceInfo,
			MessageStatus messageStatus,
			Action<LogMessageBuilder> messageBuilder,
			string? detail = null,
			ITransactionContext? transactionContext = null,
			CancellationToken cancellationToken = default);

		Task LogErrorAsync(
			ITraceInfo traceInfo,
			MessageStatus messageStatus,
			Action<ErrorMessageBuilder> messageBuilder,
			string? detail = null,
			ITransactionContext? transactionContext = null,
			CancellationToken cancellationToken = default);

		Task LogCriticalAsync(
			ITraceInfo traceInfo,
			MessageStatus messageStatus,
			Action<ErrorMessageBuilder> messageBuilder,
			string? detail = null,
			ITransactionContext? transactionContext = null,
			CancellationToken cancellationToken = default);
	}
}

using Raider.Logging;
using Raider.ServiceBus.Messages;
using Raider.Trace;
using Raider.Transactions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Raider.ServiceBus.Events
{
	public interface IEventHandlerContext
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
		/// Event id
		/// </summary>
		Guid IdEvent { get; }

		/// <summary>
		/// Id of the session used for tracing events
		/// </summary>
		Guid? IdSession { get; }

		/// <summary>
		/// The timespan after which the Send request will be cancelled if no response arrives.
		/// </summary>
		public TimeSpan? Timeout { get; }

		void LogTrace(
			ITraceInfo traceInfo,
			MessageStatus eventStatus,
			Action<LogMessageBuilder> eventBuilder,
			string? detail = null,
			ITransactionContext? transactionContext = null);

		void LogDebug(
			ITraceInfo traceInfo,
			MessageStatus eventStatus,
			Action<LogMessageBuilder> eventBuilder,
			string? detail = null,
			ITransactionContext? transactionContext = null);

		void LogInformation(
			ITraceInfo traceInfo,
			MessageStatus eventStatus,
			Action<LogMessageBuilder> eventBuilder,
			string? detail = null,
			ITransactionContext? transactionContext = null);

		void LogWarning(
			ITraceInfo traceInfo,
			MessageStatus eventStatus,
			Action<LogMessageBuilder> eventBuilder,
			string? detail = null,
			ITransactionContext? transactionContext = null);

		void LogError(
			ITraceInfo traceInfo,
			MessageStatus eventStatus,
			Action<ErrorMessageBuilder> eventBuilder,
			string? detail = null,
			ITransactionContext? transactionContext = null);

		void LogCritical(
			ITraceInfo traceInfo,
			MessageStatus eventStatus,
			Action<ErrorMessageBuilder> eventBuilder,
			string? detail = null,
			ITransactionContext? transactionContext = null);

		Task LogTraceAsync(
			ITraceInfo traceInfo,
			MessageStatus eventStatus,
			Action<LogMessageBuilder> eventBuilder,
			string? detail = null,
			ITransactionContext? transactionContext = null,
			CancellationToken cancellationToken = default);

		Task LogDebugAsync(
			ITraceInfo traceInfo,
			MessageStatus eventStatus,
			Action<LogMessageBuilder> eventBuilder,
			string? detail = null,
			ITransactionContext? transactionContext = null,
			CancellationToken cancellationToken = default);

		Task LogInformationAsync(
			ITraceInfo traceInfo,
			MessageStatus eventStatus,
			Action<LogMessageBuilder> eventBuilder,
			string? detail = null,
			ITransactionContext? transactionContext = null,
			CancellationToken cancellationToken = default);

		Task LogWarningAsync(
			ITraceInfo traceInfo,
			MessageStatus eventStatus,
			Action<LogMessageBuilder> eventBuilder,
			string? detail = null,
			ITransactionContext? transactionContext = null,
			CancellationToken cancellationToken = default);

		Task LogErrorAsync(
			ITraceInfo traceInfo,
			MessageStatus eventStatus,
			Action<ErrorMessageBuilder> eventBuilder,
			string? detail = null,
			ITransactionContext? transactionContext = null,
			CancellationToken cancellationToken = default);

		Task LogCriticalAsync(
			ITraceInfo traceInfo,
			MessageStatus eventStatus,
			Action<ErrorMessageBuilder> eventBuilder,
			string? detail = null,
			ITransactionContext? transactionContext = null,
			CancellationToken cancellationToken = default);
	}
}

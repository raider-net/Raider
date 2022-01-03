using Raider.Logging;
using Raider.ServiceBus.BusLogger;
using Raider.ServiceBus.Messages;
using Raider.Trace;
using Raider.Transactions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Raider.ServiceBus.Events
{
	public abstract class EventHandlerContext : IEventHandlerContext
	{

		public ITransactionContext TransactionContext { get; internal set; }

		public ITraceInfo PreviousTraceInfo { get; internal set; }

		public Guid IdHost { get; internal set; }

		public Guid IdEvent { get; internal set; }

		public Guid? IdSession { get; internal set; }

		public TimeSpan? Timeout { get; internal set; }

		internal IHandlerMessageLogger HandlerEventLogger { get; set; }

		public virtual void LogCritical(ITraceInfo traceInfo, MessageStatus eventStatus, Action<ErrorMessageBuilder> eventBuilder, string? detail = null, ITransactionContext? transactionContext = null)
			=> HandlerEventLogger.LogCritical(traceInfo, IdEvent, eventStatus, eventBuilder, detail, transactionContext);

		public virtual Task LogCriticalAsync(ITraceInfo traceInfo, MessageStatus eventStatus, Action<ErrorMessageBuilder> eventBuilder, string? detail = null, ITransactionContext? transactionContext = null, CancellationToken cancellationToken = default)
			=> HandlerEventLogger.LogCriticalAsync(traceInfo, IdEvent, eventStatus, eventBuilder, detail, transactionContext, cancellationToken);

		public virtual void LogDebug(ITraceInfo traceInfo, MessageStatus eventStatus, Action<LogMessageBuilder> eventBuilder, string? detail = null, ITransactionContext? transactionContext = null)
			=> HandlerEventLogger.LogDebug(traceInfo, IdEvent, eventStatus, eventBuilder, detail, transactionContext);

		public virtual Task LogDebugAsync(ITraceInfo traceInfo, MessageStatus eventStatus, Action<LogMessageBuilder> eventBuilder, string? detail = null, ITransactionContext? transactionContext = null, CancellationToken cancellationToken = default)
			=> HandlerEventLogger.LogDebugAsync(traceInfo, IdEvent, eventStatus, eventBuilder, detail, transactionContext, cancellationToken);

		public virtual void LogError(ITraceInfo traceInfo, MessageStatus eventStatus, Action<ErrorMessageBuilder> eventBuilder, string? detail = null, ITransactionContext? transactionContext = null)
			=> HandlerEventLogger.LogError(traceInfo, IdEvent, eventStatus, eventBuilder, detail, transactionContext);

		public virtual Task LogErrorAsync(ITraceInfo traceInfo, MessageStatus eventStatus, Action<ErrorMessageBuilder> eventBuilder, string? detail = null, ITransactionContext? transactionContext = null, CancellationToken cancellationToken = default)
			=> HandlerEventLogger.LogErrorAsync(traceInfo, IdEvent, eventStatus, eventBuilder, detail, transactionContext, cancellationToken);

		public virtual void LogInformation(ITraceInfo traceInfo, MessageStatus eventStatus, Action<LogMessageBuilder> eventBuilder, string? detail = null, ITransactionContext? transactionContext = null)
			=> HandlerEventLogger.LogInformation(traceInfo, IdEvent, eventStatus, eventBuilder, detail, transactionContext);

		public virtual Task LogInformationAsync(ITraceInfo traceInfo, MessageStatus eventStatus, Action<LogMessageBuilder> eventBuilder, string? detail = null, ITransactionContext? transactionContext = null, CancellationToken cancellationToken = default)
			=> HandlerEventLogger.LogInformationAsync(traceInfo, IdEvent, eventStatus, eventBuilder, detail, transactionContext, cancellationToken);

		public virtual void LogTrace(ITraceInfo traceInfo, MessageStatus eventStatus, Action<LogMessageBuilder> eventBuilder, string? detail = null, ITransactionContext? transactionContext = null)
			=> HandlerEventLogger.LogTrace(traceInfo, IdEvent, eventStatus, eventBuilder, detail, transactionContext);

		public virtual Task LogTraceAsync(ITraceInfo traceInfo, MessageStatus eventStatus, Action<LogMessageBuilder> eventBuilder, string? detail = null, ITransactionContext? transactionContext = null, CancellationToken cancellationToken = default)
			=> HandlerEventLogger.LogTraceAsync(traceInfo, IdEvent, eventStatus, eventBuilder, detail, transactionContext, cancellationToken);

		public virtual void LogWarning(ITraceInfo traceInfo, MessageStatus eventStatus, Action<LogMessageBuilder> eventBuilder, string? detail = null, ITransactionContext? transactionContext = null)
			=> HandlerEventLogger.LogWarning(traceInfo, IdEvent, eventStatus, eventBuilder, detail, transactionContext);

		public virtual Task LogWarningAsync(ITraceInfo traceInfo, MessageStatus eventStatus, Action<LogMessageBuilder> eventBuilder, string? detail = null, ITransactionContext? transactionContext = null, CancellationToken cancellationToken = default)
			=> HandlerEventLogger.LogWarningAsync(traceInfo, IdEvent, eventStatus, eventBuilder, detail, transactionContext, cancellationToken);
	}
}

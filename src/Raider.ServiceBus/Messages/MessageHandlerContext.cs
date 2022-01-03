using Raider.Logging;
using Raider.ServiceBus.BusLogger;
using Raider.Trace;
using Raider.Transactions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Raider.ServiceBus.Messages
{
	public abstract class MessageHandlerContext : IMessageHandlerContext
	{

		public ITransactionContext TransactionContext { get; internal set; }

		public ITraceInfo PreviousTraceInfo { get; internal set; }

		public Guid IdHost { get; internal set; }

		public Guid IdMessage { get; internal set; }

		public Guid? IdSession { get; internal set; }

		public TimeSpan? Timeout { get; internal set; }

		internal IHandlerMessageLogger HandlerMessageLogger { get; set; }

		public virtual void LogCritical(ITraceInfo traceInfo, MessageStatus messageStatus, Action<ErrorMessageBuilder> messageBuilder, string? detail = null, ITransactionContext? transactionContext = null)
			=> HandlerMessageLogger.LogCritical(traceInfo, IdMessage, messageStatus, messageBuilder, detail, transactionContext);

		public virtual Task LogCriticalAsync(ITraceInfo traceInfo, MessageStatus messageStatus, Action<ErrorMessageBuilder> messageBuilder, string? detail = null, ITransactionContext? transactionContext = null, CancellationToken cancellationToken = default)
			=> HandlerMessageLogger.LogCriticalAsync(traceInfo, IdMessage, messageStatus, messageBuilder, detail, transactionContext, cancellationToken);

		public virtual void LogDebug(ITraceInfo traceInfo, MessageStatus messageStatus, Action<LogMessageBuilder> messageBuilder, string? detail = null, ITransactionContext? transactionContext = null)
			=> HandlerMessageLogger.LogDebug(traceInfo, IdMessage, messageStatus, messageBuilder, detail, transactionContext);

		public virtual Task LogDebugAsync(ITraceInfo traceInfo, MessageStatus messageStatus, Action<LogMessageBuilder> messageBuilder, string? detail = null, ITransactionContext? transactionContext = null, CancellationToken cancellationToken = default)
			=> HandlerMessageLogger.LogDebugAsync(traceInfo, IdMessage, messageStatus, messageBuilder, detail, transactionContext, cancellationToken);

		public virtual void LogError(ITraceInfo traceInfo, MessageStatus messageStatus, Action<ErrorMessageBuilder> messageBuilder, string? detail = null, ITransactionContext? transactionContext = null)
			=> HandlerMessageLogger.LogError(traceInfo, IdMessage, messageStatus, messageBuilder, detail, transactionContext);

		public virtual Task LogErrorAsync(ITraceInfo traceInfo, MessageStatus messageStatus, Action<ErrorMessageBuilder> messageBuilder, string? detail = null, ITransactionContext? transactionContext = null, CancellationToken cancellationToken = default)
			=> HandlerMessageLogger.LogErrorAsync(traceInfo, IdMessage, messageStatus, messageBuilder, detail, transactionContext, cancellationToken);

		public virtual void LogInformation(ITraceInfo traceInfo, MessageStatus messageStatus, Action<LogMessageBuilder> messageBuilder, string? detail = null, ITransactionContext? transactionContext = null)
			=> HandlerMessageLogger.LogInformation(traceInfo, IdMessage, messageStatus, messageBuilder, detail, transactionContext);

		public virtual Task LogInformationAsync(ITraceInfo traceInfo, MessageStatus messageStatus, Action<LogMessageBuilder> messageBuilder, string? detail = null, ITransactionContext? transactionContext = null, CancellationToken cancellationToken = default)
			=> HandlerMessageLogger.LogInformationAsync(traceInfo, IdMessage, messageStatus, messageBuilder, detail, transactionContext, cancellationToken);

		public virtual void LogTrace(ITraceInfo traceInfo, MessageStatus messageStatus, Action<LogMessageBuilder> messageBuilder, string? detail = null, ITransactionContext? transactionContext = null)
			=> HandlerMessageLogger.LogTrace(traceInfo, IdMessage, messageStatus, messageBuilder, detail, transactionContext);

		public virtual Task LogTraceAsync(ITraceInfo traceInfo, MessageStatus messageStatus, Action<LogMessageBuilder> messageBuilder, string? detail = null, ITransactionContext? transactionContext = null, CancellationToken cancellationToken = default)
			=> HandlerMessageLogger.LogTraceAsync(traceInfo, IdMessage, messageStatus, messageBuilder, detail, transactionContext, cancellationToken);

		public virtual void LogWarning(ITraceInfo traceInfo, MessageStatus messageStatus, Action<LogMessageBuilder> messageBuilder, string? detail = null, ITransactionContext? transactionContext = null)
			=> HandlerMessageLogger.LogWarning(traceInfo, IdMessage, messageStatus, messageBuilder, detail, transactionContext);

		public virtual Task LogWarningAsync(ITraceInfo traceInfo, MessageStatus messageStatus, Action<LogMessageBuilder> messageBuilder, string? detail = null, ITransactionContext? transactionContext = null, CancellationToken cancellationToken = default)
			=> HandlerMessageLogger.LogWarningAsync(traceInfo, IdMessage, messageStatus, messageBuilder, detail, transactionContext, cancellationToken);
	}
}

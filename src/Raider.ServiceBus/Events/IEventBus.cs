using Raider.Trace;
using Raider.Transactions;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Raider.ServiceBus.Events
{
	/// <summary>
	/// Bus to work with events.
	/// </summary>
	public interface IEventBus
	{
		/// <summary>
		/// Publish an event.
		/// </summary>
		/// <param name="event">The event</param>
		/// <param name="memberName">Allows you to obtain the method or property name of the caller to the method.</param>
		/// <param name="sourceFilePath">Allows you to obtain the full path of the source file that contains the caller. This is the file path at the time of compile.</param>
		/// <param name="sourceLineNumber">Allows you to obtain the line number in the source file at which the method is called.</param>
		IResult Publish(IEvent @event,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0);

		/// <summary>
		/// Publish an event.
		/// </summary>
		/// <param name="event">The event</param>
		/// <param name="transactionContext">Current transaction context for handler</param>
		/// <param name="traceInfo">Parent trace info</param>
		IResult Publish(IEvent @event, ITransactionContext transactionContext, ITraceInfo traceInfo);

		/// <summary>
		/// Publish an event.
		/// </summary>
		/// <param name="event">The event</param>
		/// <param name="options">Options for event publishing.</param>
		/// <param name="transactionContext">Current transaction context for handler</param>
		/// <param name="traceInfo">Parent trace info</param>
		IResult Publish(IEvent @event, EventOptions options, ITransactionContext transactionContext, ITraceInfo traceInfo);

		/// <summary>
		/// Publish an event.
		/// </summary>
		/// <param name="event">The event</param>
		/// <param name="cancellationToken">Cancellation token to notify if the client no longer is interested in the response.</param>
		/// <param name="memberName">Allows you to obtain the method or property name of the caller to the method.</param>
		/// <param name="sourceFilePath">Allows you to obtain the full path of the source file that contains the caller. This is the file path at the time of compile.</param>
		/// <param name="sourceLineNumber">Allows you to obtain the line number in the source file at which the method is called.</param>
		Task<IResult> PublishAsync(IEvent @event,
			CancellationToken cancellationToken = default,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0);

		/// <summary>
		/// Publish an event.
		/// </summary>
		/// <param name="event">The event</param>
		/// <param name="transactionContext">Current transaction context for handler</param>
		/// <param name="traceInfo">Parent trace info</param>
		/// <param name="cancellationToken">Cancellation token to notify if the client no longer is interested in the response.</param>
		Task<IResult> PublishAsync(IEvent @event, ITransactionContext transactionContext, ITraceInfo traceInfo, CancellationToken cancellationToken = default);

		/// <summary>
		/// Publish an event.
		/// </summary>
		/// <param name="event">The event</param>
		/// <param name="options">Options for event publishing.</param>
		/// <param name="transactionContext">Current transaction context for handler</param>
		/// <param name="traceInfo">Parent trace info</param>
		/// <param name="cancellationToken">Cancellation token to notify if the client no longer is interested in the response.</param>
		Task<IResult> PublishAsync(IEvent @event, EventOptions options, ITransactionContext transactionContext, ITraceInfo traceInfo, CancellationToken cancellationToken = default);
	}
}

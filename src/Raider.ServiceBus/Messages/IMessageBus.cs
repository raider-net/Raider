using Raider.Trace;
using Raider.Transactions;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Raider.ServiceBus.Messages
{
	/// <summary>
	/// Bus to work with request-response messages.
	/// </summary>
	public interface IMessageBus
	{
		/// <summary>
		/// Sends a request message.
		/// </summary>
		/// <param name="message">The request message</param>
		/// <param name="memberName">Allows you to obtain the method or property name of the caller to the method.</param>
		/// <param name="sourceFilePath">Allows you to obtain the full path of the source file that contains the caller. This is the file path at the time of compile.</param>
		/// <param name="sourceLineNumber">Allows you to obtain the line number in the source file at which the method is called.</param>
		IResult Send(Messages.IRequestMessage message,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0);

		/// <summary>
		/// Sends a request message.
		/// </summary>
		/// <param name="message">The request message</param>
		/// <param name="transactionContext">Current transaction context for handler</param>
		/// <param name="traceInfo">Parent trace info</param>
		IResult Send(Messages.IRequestMessage message, ITransactionContext transactionContext, ITraceInfo traceInfo);

		/// <summary>
		/// Sends a request message.
		/// </summary>
		/// <param name="message">The request message</param>
		/// <param name="options">Options for message sending.</param>
		/// <param name="transactionContext">Current transaction context for handler</param>
		/// <param name="traceInfo">Parent trace info</param>
		IResult Send(Messages.IRequestMessage message, Messages.MessageOptions options, ITransactionContext transactionContext, ITraceInfo traceInfo);

		/// <summary>
		/// Sends a request message.
		/// </summary>
		/// <typeparam name="TResponse">The response message type</typeparam>
		/// <param name="message">The request message</param>
		/// <param name="memberName">Allows you to obtain the method or property name of the caller to the method.</param>
		/// <param name="sourceFilePath">Allows you to obtain the full path of the source file that contains the caller. This is the file path at the time of compile.</param>
		/// <param name="sourceLineNumber">Allows you to obtain the line number in the source file at which the method is called.</param>
		/// <returns>The response message.</returns>
		IResult<TResponse> Send<TResponse>(Messages.IRequestMessage<TResponse> message,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0);

		/// <summary>
		/// Sends a request message.
		/// </summary>
		/// <typeparam name="TResponse">The response message type</typeparam>
		/// <param name="message">The request message</param>
		/// <param name="transactionContext">Current transaction context for handler</param>
		/// <param name="traceInfo">Parent trace info</param>
		/// <returns>The response message.</returns>
		IResult<TResponse> Send<TResponse>(Messages.IRequestMessage<TResponse> message, ITransactionContext transactionContext, ITraceInfo traceInfo);

		/// <summary>
		/// Sends a request message.
		/// </summary>
		/// <typeparam name="TResponse">The response message type</typeparam>
		/// <param name="message">The request message</param>
		/// <param name="options">Options for message sending.</param>
		/// <param name="transactionContext">Current transaction context for handler</param>
		/// <param name="traceInfo">Parent trace info</param>
		/// <returns>The response message.</returns>
		IResult<TResponse> Send<TResponse>(Messages.IRequestMessage<TResponse> message, Messages.MessageOptions options, ITransactionContext transactionContext, ITraceInfo traceInfo);

		/// <summary>
		/// Sends a request message.
		/// </summary>
		/// <param name="message">The request message</param>
		/// <param name="cancellationToken">Cancellation token to notify if the client no longer is interested in the response.</param>
		/// <param name="memberName">Allows you to obtain the method or property name of the caller to the method.</param>
		/// <param name="sourceFilePath">Allows you to obtain the full path of the source file that contains the caller. This is the file path at the time of compile.</param>
		/// <param name="sourceLineNumber">Allows you to obtain the line number in the source file at which the method is called.</param>
		Task<IResult> SendAsync(Messages.IRequestMessage message,
			CancellationToken cancellationToken = default,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0);

		/// <summary>
		/// Sends a request message.
		/// </summary>
		/// <param name="message">The request message</param>
		/// <param name="transactionContext">Current transaction context for handler</param>
		/// <param name="traceInfo">Parent trace info</param>
		/// <param name="cancellationToken">Cancellation token to notify if the client no longer is interested in the response.</param>
		Task<IResult> SendAsync(Messages.IRequestMessage message, ITransactionContext transactionContext, ITraceInfo traceInfo, CancellationToken cancellationToken = default);

		/// <summary>
		/// Sends a request message.
		/// </summary>
		/// <param name="message">The request message</param>
		/// <param name="options">Options for message sending.</param>
		/// <param name="transactionContext">Current transaction context for handler</param>
		/// <param name="traceInfo">Parent trace info</param>
		/// <param name="cancellationToken">Cancellation token to notify if the client no longer is interested in the response.</param>
		Task<IResult> SendAsync(Messages.IRequestMessage message, Messages.MessageOptions options, ITransactionContext transactionContext, ITraceInfo traceInfo, CancellationToken cancellationToken = default);

		/// <summary>
		/// Sends a request message.
		/// </summary>
		/// <typeparam name="TResponse">The response message type</typeparam>
		/// <param name="message">The request message</param>
		/// <param name="cancellationToken">Cancellation token to notify if the client no longer is interested in the response.</param>
		/// <param name="memberName">Allows you to obtain the method or property name of the caller to the method.</param>
		/// <param name="sourceFilePath">Allows you to obtain the full path of the source file that contains the caller. This is the file path at the time of compile.</param>
		/// <param name="sourceLineNumber">Allows you to obtain the line number in the source file at which the method is called.</param>
		Task<IResult<TResponse>> SendAsync<TResponse>(Messages.IRequestMessage<TResponse> message,
			CancellationToken cancellationToken = default,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0);

		/// <summary>
		/// Sends a request message.
		/// </summary>
		/// <typeparam name="TResponse">The response message type</typeparam>
		/// <param name="message">The request message</param>
		/// <param name="transactionContext">Current transaction context for handler</param>
		/// <param name="traceInfo">Parent trace info</param>
		/// <param name="cancellationToken">Cancellation token to notify if the client no longer is interested in the response.</param>
		Task<IResult<TResponse>> SendAsync<TResponse>(Messages.IRequestMessage<TResponse> message, ITransactionContext transactionContext, ITraceInfo traceInfo, CancellationToken cancellationToken = default);

		/// <summary>
		/// Sends a request message.
		/// </summary>
		/// <typeparam name="TResponse">The response message type</typeparam>
		/// <param name="message">The request message</param>
		/// <param name="options">Options for message sending.</param>
		/// <param name="transactionContext">Current transaction context for handler</param>
		/// <param name="traceInfo">Parent trace info</param>
		/// <param name="cancellationToken">Cancellation token to notify if the client no longer is interested in the response.</param>
		Task<IResult<TResponse>> SendAsync<TResponse>(Messages.IRequestMessage<TResponse> message, Messages.MessageOptions options, ITransactionContext transactionContext, ITraceInfo traceInfo, CancellationToken cancellationToken = default);
	}
}

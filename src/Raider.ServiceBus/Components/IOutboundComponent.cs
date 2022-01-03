using Raider.Trace;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Raider.ServiceBus.Components
{
	public interface IOutboundComponent : IServiceBusComponent
	{
		int RetryIntervalInSeconds { get; }
		int? FailureTimeoutInSeconds { get; }
		int? MaxRetryCount { get; }


		/// <summary>
		/// Handle a request message.
		/// </summary>
		/// <param name="message">The request message</param>
		/// <param name="options">Options for message sending.</param>
		/// <param name="previousTraceInfo">Parent trace info</param>
		/// <param name="cancellationToken">Cancellation token to notify if the client no longer is interested in the response.</param>
		Task<IResult> OnHandleRequestAsync(IRequestMessage message, MessageOptions options, ITraceInfo previousTraceInfo, CancellationToken cancellationToken = default);

		/// <summary>
		/// Handle a request message.
		/// </summary>
		/// <typeparam name="TResponse">The response message type</typeparam>
		/// <param name="message">The request message</param>
		/// <param name="options">Options for message sending.</param>
		/// <param name="previousTraceInfo">Parent trace info</param>
		/// <param name="cancellationToken">Cancellation token to notify if the client no longer is interested in the response.</param>
		Task<IResult> OnHandleRequestAsync<TResponse>(IRequestMessage<TResponse> message, MessageOptions options, ITraceInfo previousTraceInfo, CancellationToken cancellationToken = default)
			where TResponse : IResponseMessage;

		/// <summary>
		/// Sends a response message asynchronously, when the caller does not wait for the response.
		/// </summary>
		/// <param name="message">The response message</param>
		/// <param name="cancellationToken">Cancellation token to notify if the client no longer is interested in the response.</param>
		/// <param name="memberName">Allows you to obtain the method or property name of the caller to the method.</param>
		/// <param name="sourceFilePath">Allows you to obtain the full path of the source file that contains the caller. This is the file path at the time of compile.</param>
		/// <param name="sourceLineNumber">Allows you to obtain the line number in the source file at which the method is called.</param>
		Task<IResult> SendResponseAsync(IResponseMessage message,
			CancellationToken cancellationToken = default,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0);

		/// <summary>
		/// Sends a response message asynchronously, when the caller does not wait for the response.
		/// </summary>
		/// <param name="message">The response message</param>
		/// <param name="traceInfo">Parent trace info</param>
		/// <param name="cancellationToken">Cancellation token to notify if the client no longer is interested in the response.</param>
		Task<IResult> SendResponseAsync(IResponseMessage message, ITraceInfo traceInfo, CancellationToken cancellationToken = default);
	}
}

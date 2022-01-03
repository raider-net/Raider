using Raider.Trace;
using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Raider.ServiceBus.Components
{
	public interface IInboundComponent : IServiceBusComponent
	{
		/// <summary>
		/// Sends a request message.
		/// </summary>
		/// <param name="message">The request message</param>
		/// <param name="cancellationToken">Cancellation token to notify if the client no longer is interested in the response.</param>
		/// <param name="memberName">Allows you to obtain the method or property name of the caller to the method.</param>
		/// <param name="sourceFilePath">Allows you to obtain the full path of the source file that contains the caller. This is the file path at the time of compile.</param>
		/// <param name="sourceLineNumber">Allows you to obtain the line number in the source file at which the method is called.</param>
		Task<IResult> SendRequestAsync(IRequestMessage message,
			CancellationToken cancellationToken = default,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0);

		/// <summary>
		/// Sends a request message.
		/// </summary>
		/// <param name="message">The request message</param>
		/// <param name="traceInfo">Parent trace info</param>
		/// <param name="cancellationToken">Cancellation token to notify if the client no longer is interested in the response.</param>
		Task<IResult> SendRequestAsync(IRequestMessage message, ITraceInfo traceInfo, CancellationToken cancellationToken = default);

		/// <summary>
		/// Sends a request message.
		/// </summary>
		/// <param name="message">The request message</param>
		/// <param name="options">Options for message sending.</param>
		/// <param name="traceInfo">Parent trace info</param>
		/// <param name="cancellationToken">Cancellation token to notify if the client no longer is interested in the response.</param>
		Task<IResult> SendRequestAsync(IRequestMessage message, MessageOptions options, ITraceInfo traceInfo, CancellationToken cancellationToken = default);

		/// <summary>
		/// Sends a request message.
		/// </summary>
		/// <typeparam name="TResponse">The response message type</typeparam>
		/// <param name="message">The request message</param>
		/// <param name="cancellationToken">Cancellation token to notify if the client no longer is interested in the response.</param>
		/// <param name="memberName">Allows you to obtain the method or property name of the caller to the method.</param>
		/// <param name="sourceFilePath">Allows you to obtain the full path of the source file that contains the caller. This is the file path at the time of compile.</param>
		/// <param name="sourceLineNumber">Allows you to obtain the line number in the source file at which the method is called.</param>
		Task<IResult> SendRequestAsync<TResponse>(IRequestMessage<TResponse> message,
			CancellationToken cancellationToken = default,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
			where TResponse : IResponseMessage;

		/// <summary>
		/// Sends a request message.
		/// </summary>
		/// <typeparam name="TResponse">The response message type</typeparam>
		/// <param name="message">The request message</param>
		/// <param name="traceInfo">Parent trace info</param>
		/// <param name="cancellationToken">Cancellation token to notify if the client no longer is interested in the response.</param>
		Task<IResult> SendRequestAsync<TResponse>(IRequestMessage<TResponse> message, ITraceInfo traceInfo, CancellationToken cancellationToken = default)
			where TResponse : IResponseMessage;

		/// <summary>
		/// Sends a request message.
		/// </summary>
		/// <typeparam name="TResponse">The response message type</typeparam>
		/// <param name="message">The request message</param>
		/// <param name="options">Options for message sending.</param>
		/// <param name="traceInfo">Parent trace info</param>
		/// <param name="cancellationToken">Cancellation token to notify if the client no longer is interested in the response.</param>
		Task<IResult> SendRequestAsync<TResponse>(IRequestMessage<TResponse> message, MessageOptions options, ITraceInfo traceInfo, CancellationToken cancellationToken = default)
			where TResponse : IResponseMessage;

		/// <summary>
		/// Handle a response message asynchronously, that come from <see cref="SendRequestAsync{TResponse}(IRequestMessage{TResponse}, MessageOptions, ITraceInfo, CancellationToken)"/>.
		/// </summary>
		/// <param name="message">The request message</param>
		/// <param name="options">Options for message sending.</param>
		/// <param name="previousTraceInfo">Parent trace info</param>
		/// <param name="cancellationToken">Cancellation token to notify if the client no longer is interested in the response.</param>
		Task<IResult> OnHandleResponseAsync(IResponseMessage message, MessageOptions options, ITraceInfo previousTraceInfo, CancellationToken cancellationToken = default);

		/// <summary>
		/// Sends a request message and synchronously wait for a response.
		/// </summary>
		/// <typeparam name="TResponse">The response message type</typeparam>
		/// <param name="message">The request message</param>
		/// <param name="timeout">Timeout waiting for response</param>
		/// <param name="cancellationToken">Cancellation token to notify if the client no longer is interested in the response.</param>
		/// <param name="memberName">Allows you to obtain the method or property name of the caller to the method.</param>
		/// <param name="sourceFilePath">Allows you to obtain the full path of the source file that contains the caller. This is the file path at the time of compile.</param>
		/// <param name="sourceLineNumber">Allows you to obtain the line number in the source file at which the method is called.</param>
		Task<IResult<TResponse>> SendRequestSynchronouslyAsync<TResponse>(IRequestMessage<TResponse> message,
			TimeSpan timeout,
			CancellationToken cancellationToken = default,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
			where TResponse : IResponseMessage;

		/// <summary>
		/// Sends a request message and synchronously wait for a response.
		/// </summary>
		/// <typeparam name="TResponse">The response message type</typeparam>
		/// <param name="message">The request message</param>
		/// <param name="timeout">Timeout waiting for response</param>
		/// <param name="traceInfo">Parent trace info</param>
		/// <param name="cancellationToken">Cancellation token to notify if the client no longer is interested in the response.</param>
		Task<IResult<TResponse>> SendRequestSynchronouslyAsync<TResponse>(IRequestMessage<TResponse> message, TimeSpan timeout, ITraceInfo traceInfo, CancellationToken cancellationToken = default)
			where TResponse : IResponseMessage;

		/// <summary>
		/// Sends a request message and synchronously wait for a response.
		/// </summary>
		/// <typeparam name="TResponse">The response message type</typeparam>
		/// <param name="message">The request message</param>
		/// <param name="timeout">Timeout waiting for response</param>
		/// <param name="options">Options for message sending.</param>
		/// <param name="traceInfo">Parent trace info</param>
		/// <param name="cancellationToken">Cancellation token to notify if the client no longer is interested in the response.</param>
		Task<IResult<TResponse>> SendRequestSynchronouslyAsync<TResponse>(IRequestMessage<TResponse> message, TimeSpan timeout, MessageOptions options, ITraceInfo traceInfo, CancellationToken cancellationToken = default)
			where TResponse : IResponseMessage;
	}
}

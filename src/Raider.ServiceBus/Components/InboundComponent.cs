using Raider.Trace;
using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Raider.ServiceBus.Components
{
	public abstract class InboundComponent : IInboundComponent, IServiceBusComponent
	{
		/// <inheritdoc/>
		public virtual Task<IResult> OnInit(CancellationToken cancellationToken = default)
			=> Task.FromResult((IResult)new ResultBuilder().Build());

		/// <inheritdoc/>
		public virtual Task<IResult> OnStarted(CancellationToken cancellationToken = default)
			=> Task.FromResult((IResult)new ResultBuilder().Build());

		/// <inheritdoc/>
		public virtual Task<IResult> OnStopping(CancellationToken cancellationToken = default)
			=> Task.FromResult((IResult)new ResultBuilder().Build());

		/// <inheritdoc/>
		public virtual Task<IResult<bool>> OnError(FailureReason failureReason, Exception? exception, CancellationToken cancellationToken = default)
			=> Task.FromResult(new ResultBuilder<bool>().Build());




		/// <inheritdoc/>
		public Task<IResult> SendRequestAsync(IRequestMessage message,
			CancellationToken cancellationToken = default,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
			=> SendRequestAsync(message, new MessageOptions(), TraceInfo.Create(null, null, memberName, sourceFilePath, sourceLineNumber), cancellationToken);

		/// <inheritdoc/>
		public Task<IResult> SendRequestAsync(IRequestMessage message, ITraceInfo traceInfo, CancellationToken cancellationToken = default)
			=> SendRequestAsync(message, new MessageOptions(), traceInfo, cancellationToken);

		/// <inheritdoc/>
		public Task<IResult> SendRequestAsync(IRequestMessage message, MessageOptions options, ITraceInfo traceInfo, CancellationToken cancellationToken = default)
		{
			throw new NotImplementedException();
		}

		/// <inheritdoc/>
		public Task<IResult> SendRequestAsync<TResponse>(IRequestMessage<TResponse> message,
			CancellationToken cancellationToken = default,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
			where TResponse : IResponseMessage
			=> SendRequestAsync(message, new MessageOptions(), TraceInfo.Create(null, null, memberName, sourceFilePath, sourceLineNumber), cancellationToken);

		/// <inheritdoc/>
		public Task<IResult> SendRequestAsync<TResponse>(IRequestMessage<TResponse> message, ITraceInfo traceInfo, CancellationToken cancellationToken = default)
			where TResponse : IResponseMessage
			=> SendRequestAsync(message, new MessageOptions(), traceInfo, cancellationToken);

		/// <inheritdoc/>
		public Task<IResult> SendRequestAsync<TResponse>(IRequestMessage<TResponse> message, MessageOptions options, ITraceInfo traceInfo, CancellationToken cancellationToken = default)
			where TResponse : IResponseMessage
		{
			throw new NotImplementedException();
		}

		public abstract Task<IResult> OnHandleResponseAsync(IResponseMessage message, MessageOptions options, ITraceInfo previousTraceInfo, CancellationToken cancellationToken = default);

		/// <inheritdoc/>
		public Task<IResult<TResponse>> SendRequestSynchronouslyAsync<TResponse>(IRequestMessage<TResponse> message,
			TimeSpan timeout,
			CancellationToken cancellationToken = default,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
			where TResponse : IResponseMessage
			=> SendRequestSynchronouslyAsync(message, timeout, new MessageOptions(), TraceInfo.Create(null, null, memberName, sourceFilePath, sourceLineNumber), cancellationToken);

		/// <inheritdoc/>
		public Task<IResult<TResponse>> SendRequestSynchronouslyAsync<TResponse>(IRequestMessage<TResponse> message, TimeSpan timeout, ITraceInfo traceInfo, CancellationToken cancellationToken = default)
			where TResponse : IResponseMessage
			=> SendRequestSynchronouslyAsync(message, timeout, new MessageOptions(), traceInfo, cancellationToken);

		/// <inheritdoc/>
		public Task<IResult<TResponse>> SendRequestSynchronouslyAsync<TResponse>(IRequestMessage<TResponse> message, TimeSpan timeout, MessageOptions options, ITraceInfo traceInfo, CancellationToken cancellationToken = default)
			where TResponse : IResponseMessage
		{
			throw new NotImplementedException();
		}
	}
}

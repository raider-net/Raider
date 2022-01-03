using Raider.Trace;
using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Raider.ServiceBus.Components
{
	public abstract class OutboundComponent : IOutboundComponent, IServiceBusComponent
	{
		public int RetryIntervalInSeconds { get; private set; }
		public int? FailureTimeoutInSeconds { get; private set; }
		public int? MaxRetryCount { get; private set; }

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
		public abstract Task<IResult> OnHandleRequestAsync(IRequestMessage message, MessageOptions options, ITraceInfo previousTraceInfo, CancellationToken cancellationToken = default);

		/// <inheritdoc/>
		public abstract Task<IResult> OnHandleRequestAsync<TResponse>(IRequestMessage<TResponse> message, MessageOptions options, ITraceInfo previousTraceInfo, CancellationToken cancellationToken = default)
			where TResponse : IResponseMessage;

		/// <inheritdoc/>
		public Task<IResult> SendResponseAsync(IResponseMessage message,
			CancellationToken cancellationToken = default,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
		{
			throw new NotImplementedException();
		}

		/// <inheritdoc/>
		public Task<IResult> SendResponseAsync(IResponseMessage message, ITraceInfo traceInfo, CancellationToken cancellationToken = default)
		{
			throw new NotImplementedException();
		}
	}
}

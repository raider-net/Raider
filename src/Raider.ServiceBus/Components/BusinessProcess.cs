using Raider.Trace;
using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Raider.ServiceBus.Components
{
	public abstract class BusinessProcess<TPrimaryRequestMessage, TPrimaryResponseMessage> : IBusinessProcess<TPrimaryRequestMessage, TPrimaryResponseMessage>, IBaseBusinessProcess, IServiceBusComponent
		where TPrimaryRequestMessage : IRequestMessage<TPrimaryResponseMessage>
		where TPrimaryResponseMessage : IResponseMessage
	{
		public Guid IdBusinessProcess { get; }
		public TPrimaryRequestMessage PrimaryRequestMessage { get; }
		public TPrimaryResponseMessage PrimaryResponseMessage { get; }
		public int MaxInstancesCount { get; }
		public bool IsSingleton => MaxInstancesCount == 1;


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
		public abstract Task<IResult> OnHandlePrimaryRequestAsync(TPrimaryRequestMessage message, MessageOptions options, ITraceInfo previousTraceInfo, CancellationToken cancellationToken = default);

		/// <inheritdoc/>
		public Task<IResult> Reply(TPrimaryResponseMessage message,
			CancellationToken cancellationToken = default,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
			=> Reply(message, TraceInfo.Create(null, null, memberName, sourceFilePath, sourceLineNumber), cancellationToken);

		/// <inheritdoc/>
		public Task<IResult> Reply(TPrimaryResponseMessage message, ITraceInfo traceInfo, CancellationToken cancellationToken = default)
		{
			throw new NotImplementedException();
		}





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

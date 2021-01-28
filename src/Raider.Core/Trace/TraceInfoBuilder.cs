using Raider.Identity;
using Raider.Infrastructure;
using System;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Security.Principal;

namespace Raider.Trace
{
	public interface ITraceInfoBuilder<TBuilder>
		where TBuilder : ITraceInfoBuilder<TBuilder>
	{
		TBuilder Clone(
			ITraceFrame currentTraceFrame,
			ITraceInfo traceInfo);

		ITraceInfo Build();

		TBuilder RuntimeUniqueKey(Guid runtimeUniqueKey, bool force = false);

		//TBuilder TraceFrame(ITraceFrame? traceFrame, bool force = false);

		TBuilder IdUser(int? idUser, bool force = false);

		TBuilder IdUser(ClaimsPrincipal user, bool force = false);

		TBuilder IdUser(IIdentity user, bool force = false);

		TBuilder ExternalCorrelationId(string? externalCorrelationId, bool force = false);

		TBuilder CorrelationId(Guid? correlationId, bool force = false);
	}

	public abstract class TraceInfoBuilderBase<TBuilder> : ITraceInfoBuilder<TBuilder>
		where TBuilder : TraceInfoBuilderBase<TBuilder>
	{
		private class TraceInfo : ITraceInfo
		{
			public Guid RuntimeUniqueKey { get; set; }

			public ITraceFrame TraceFrame { get; set; }

			public int? IdUser { get; set; }

			public string? ExternalCorrelationId { get; set; }

			public Guid? CorrelationId { get; set; }

			public TraceInfo(ITraceFrame traceFrame)
			{
				RuntimeUniqueKey = EnvironmentInfo.RUNTIME_UNIQUE_KEY;
				TraceFrame = traceFrame ?? throw new ArgumentNullException(nameof(traceFrame));
			}
		}

		private readonly TBuilder _builder;
		private TraceInfo _traceInfo;

		protected TraceInfoBuilderBase(ITraceFrame currentTraceFrame, ITraceInfo? previousTraceInfo)
		{
			if (currentTraceFrame == null)
				throw new ArgumentNullException(nameof(currentTraceFrame));

			var traceFrameBuilder = new TraceFrameBuilder(previousTraceInfo?.TraceFrame)
				.CallerMemberName(currentTraceFrame.CallerMemberName)
				.CallerFilePath(currentTraceFrame.CallerFilePath)
				.CallerLineNumber(currentTraceFrame.CallerLineNumber)
				.MethodParameters(currentTraceFrame.MethodParameters);

			((ITraceFrameBuilder<TraceFrameBuilder>)traceFrameBuilder)
				.MethodCallId(currentTraceFrame.MethodCallId);

			if (previousTraceInfo == null)
			{
				_traceInfo = new TraceInfo(traceFrameBuilder.Build());
			}
			else
			{
				_traceInfo = new TraceInfo(traceFrameBuilder.Build())
				{
					RuntimeUniqueKey = previousTraceInfo.RuntimeUniqueKey,
					IdUser = previousTraceInfo.IdUser,
					ExternalCorrelationId = previousTraceInfo.ExternalCorrelationId,
					CorrelationId = previousTraceInfo.CorrelationId
				};
			}

			_builder = (TBuilder)this;
		}

		public virtual TBuilder Clone(
			ITraceFrame currentTraceFrame,
			ITraceInfo traceInfo)
		{
			if (traceInfo == null)
				throw new ArgumentNullException(nameof(traceInfo));

			_traceInfo = new TraceInfo(
				new TraceFrameBuilder(traceInfo.TraceFrame)
					.CallerMemberName(currentTraceFrame.CallerMemberName)
					.CallerFilePath(currentTraceFrame.CallerFilePath)
					.CallerLineNumber(currentTraceFrame.CallerLineNumber)
					.MethodParameters(currentTraceFrame.MethodParameters)
					.Build())
			{
				RuntimeUniqueKey = traceInfo.RuntimeUniqueKey,
				IdUser = traceInfo.IdUser,
				ExternalCorrelationId = traceInfo.ExternalCorrelationId,
				CorrelationId = traceInfo.CorrelationId
			};

			return _builder;
		}

		public ITraceInfo Build()
			=> _traceInfo;

		public TBuilder RuntimeUniqueKey(Guid runtimeUniqueKey, bool force = false)
		{
			if (force || _traceInfo.RuntimeUniqueKey == Guid.Empty)
				_traceInfo.RuntimeUniqueKey = runtimeUniqueKey;

			return _builder;
		}

		//public TBuilder TraceFrame(ITraceFrame? traceFrame, bool force = false)
		//{
		//	if (force || _traceInfo.TraceFrame == null)
		//		_traceInfo.TraceFrame = traceFrame;

		//	return _builder;
		//}

		public TBuilder IdUser(int? idUser, bool force = false)
		{
			if (force || !_traceInfo.IdUser.HasValue)
				_traceInfo.IdUser = idUser;

			return _builder;
		}

		public TBuilder IdUser(ClaimsPrincipal user, bool force = false)
		{
			int? idUser = null;

			if (user is RaiderPrincipal<int> principal)
				idUser = principal.IdentityBase?.UserId;

			return IdUser(idUser, force);
		}

		public TBuilder IdUser(IIdentity identity, bool force = false)
		{
			int? idUser = null;

			if (identity is RaiderIdentity<int> identityBase)
				idUser = identityBase.UserId;

			return IdUser(idUser, force);
		}

		public TBuilder ExternalCorrelationId(string? externalCorrelationId, bool force = false)
		{
			if (force || string.IsNullOrWhiteSpace(_traceInfo.ExternalCorrelationId))
				_traceInfo.ExternalCorrelationId = externalCorrelationId;

			return _builder;
		}

		public TBuilder CorrelationId(Guid? correlationId, bool force = false)
		{
			if (force || !_traceInfo.CorrelationId.HasValue)
				_traceInfo.CorrelationId = correlationId;

			return _builder;
		}
	}

	public sealed class TraceInfoBuilder : TraceInfoBuilderBase<TraceInfoBuilder>
	{
		public TraceInfoBuilder(ITraceFrame currentTraceFrame)
			: this(currentTraceFrame, null)
		{
		}

		public TraceInfoBuilder(ITraceFrame currentTraceFrame, ITraceInfo? previousTraceInfo)
			: base(currentTraceFrame, previousTraceInfo)
		{
		}

		//public static implicit operator TraceInfo?(TraceInfoBuilder builder)
		//{
		//	if (builder == null)
		//		return null;

		//	return builder._traceInfo as TraceInfo;
		//}

		//public static implicit operator TraceInfoBuilder?(TraceInfo traceInfo)
		//{
		//	if (traceInfo == null)
		//		return null;

		//	return new TraceInfoBuilder(traceInfo);
		//}
	}
}

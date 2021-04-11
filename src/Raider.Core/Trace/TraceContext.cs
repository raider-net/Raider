using Raider.Identity;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Security.Principal;

namespace Raider.Trace
{
	public class TraceContext
	{
		private readonly List<KeyValuePair<string, ITraceInfo>> _traces = new();

		private ITraceInfo? _lastTraceInfo;
		private int? _userId;

		public bool Initialized => _lastTraceInfo != null;
		public Guid? CorrelationId => _lastTraceInfo?.CorrelationId;

		private readonly object _lockInit = new();
		public TraceContext Initialize(string key, ITraceInfo traceInfo)
		{
			if (string.IsNullOrWhiteSpace(key))
				throw new ArgumentNullException(nameof(key));

			if (_lastTraceInfo != null)
				throw new NotSupportedException($"{nameof(TraceContext)} already initialized");

			lock (_lockInit)
			{
				if (_lastTraceInfo != null)
					throw new NotSupportedException($"{nameof(TraceContext)} already initialized");

				_lastTraceInfo = traceInfo ?? throw new ArgumentNullException(nameof(traceInfo));
				_traces.Add(new KeyValuePair<string, ITraceInfo>(key, _lastTraceInfo));
			}

			return this;
		}

		private readonly object _lockTrace = new();
		public ITraceInfo AddTraceFrame(string key, ITraceFrame traceFrame)
		{
			if (string.IsNullOrWhiteSpace(key))
				throw new ArgumentNullException(nameof(key));

			if (traceFrame == null)
				throw new ArgumentNullException(nameof(traceFrame));

			if (_lastTraceInfo == null)
				throw new NotSupportedException($"{nameof(TraceContext)} was not initialized");

			lock (_lockTrace)
			{
				_lastTraceInfo = new TraceInfoBuilder(traceFrame, _lastTraceInfo)
					.IdUser(_userId)
					.Build();

				_traces.Add(new KeyValuePair<string, ITraceInfo>(key, _lastTraceInfo));
			}

			return _lastTraceInfo;
		}

		private readonly object _lockUser = new();
		public void SetUser(int userId)
		{
			if (_lastTraceInfo == null)
				throw new NotSupportedException($"{nameof(TraceContext)} was not initialized");

			if (_userId == userId || _lastTraceInfo.IdUser == userId)
				return;

			if (_userId.HasValue || _lastTraceInfo.IdUser.HasValue)
				throw new NotSupportedException($"UserId was already set");

			lock (_lockUser)
			{
				if (_userId == userId || _lastTraceInfo.IdUser == userId)
					return;

				if (_userId.HasValue || _lastTraceInfo.IdUser.HasValue)
					throw new NotSupportedException($"UserId was already set");

				_userId = userId;
			}
		}

		public void SetUser(ClaimsPrincipal principal)
		{
			if (principal == null)
				throw new ArgumentNullException(nameof(principal));

			if (principal is RaiderPrincipal<int> raiderPrincipal && raiderPrincipal.IdentityBase != null)
				SetUser(raiderPrincipal.IdentityBase.UserId);
			else
				throw new NotSupportedException($"{nameof(principal)} must be {nameof(RaiderPrincipal<int>)}");
		}

		public void SetUser(IIdentity identity)
		{
			if (identity == null)
				throw new ArgumentNullException(nameof(identity));

			if (identity is RaiderIdentity<int> identityBase)
				SetUser(identityBase.UserId);
			else
				throw new NotSupportedException($"{nameof(identity)} must be {nameof(RaiderIdentity<int>)}");
		}

		public ITraceInfo Next(
			IEnumerable<MethodParameter>? methodParameters = null,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
			=> Next(new TraceFrameBuilder()
					.CallerMemberName(memberName)
					.CallerFilePath(sourceFilePath)
					.CallerLineNumber(sourceLineNumber == 0 ? (int?)null : sourceLineNumber)
					.MethodParameters(methodParameters)
					.Build());

		public ITraceInfo Next(ITraceFrame traceFrame)
			=> new TraceInfoBuilder(traceFrame, _lastTraceInfo)
				.IdUser(_userId)
				.Build();

		//public bool TryGetTraceInfo(string key, [MaybeNullWhen(false)] out ITraceInfo? traceInfo)
		//	=> _trace.TryGetValue(key, out traceInfo);
	}
}

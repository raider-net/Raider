using Raider.Identity;
using Raider.Localization;
using Raider.Trace;
using Raider.Web;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Raider
{
	public interface IApplicationContext
	{
		ITraceInfo TraceInfo { get; }
		IApplicationResources ApplicationResources { get; }
		IRequestMetadata? RequestMetadata { get; }

		ITraceInfo AddTraceFrame(ITraceFrame traceFrame);

		ITraceInfo AddTraceFrame(ITraceFrame traceFrame, int? idUser);

		ITraceInfo AddTraceFrame(ITraceFrame traceFrame, RaiderPrincipal<int>? principal);

		ITraceInfo Next(
			IEnumerable<MethodParameter>? methodParameters = null,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0);

		ITraceInfo Next(ITraceFrame traceFrame);
	}

	public class ApplicationContext : IApplicationContext
	{
		public ITraceInfo TraceInfo { get; private set; }
		public IApplicationResources ApplicationResources { get; }
		public IRequestMetadata? RequestMetadata { get; }

		public ApplicationContext(ITraceInfo traceInfo, IApplicationResources applicationResources, IRequestMetadata? requestMetadata)
		{
			TraceInfo = traceInfo ?? throw new ArgumentNullException(nameof(traceInfo));
			ApplicationResources = applicationResources ?? throw new ArgumentNullException(nameof(applicationResources));
			RequestMetadata = requestMetadata;
		}

		private readonly object _lockTrace = new();
		public ITraceInfo AddTraceFrame(ITraceFrame traceFrame)
		{
			if (traceFrame == null)
				throw new ArgumentNullException(nameof(traceFrame));

			lock (_lockTrace)
			{
				TraceInfo = new TraceInfoBuilder(traceFrame, TraceInfo)
					.Build();
			}

			return TraceInfo;
		}

		public ITraceInfo AddTraceFrame(ITraceFrame traceFrame, int? idUser)
		{
			if (traceFrame == null)
				throw new ArgumentNullException(nameof(traceFrame));

			lock (_lockTrace)
			{
				TraceInfo = new TraceInfoBuilder(traceFrame, TraceInfo)
					.IdUser(idUser)
					.Build();
			}

			return TraceInfo;
		}

		public ITraceInfo AddTraceFrame(ITraceFrame traceFrame, RaiderPrincipal<int>? principal)
		{
			if (traceFrame == null)
				throw new ArgumentNullException(nameof(traceFrame));

			lock (_lockTrace)
			{
				TraceInfo = new TraceInfoBuilder(traceFrame, TraceInfo)
					.Principal(principal)
					.Build();
			}

			return TraceInfo;
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
			=> new TraceInfoBuilder(traceFrame, TraceInfo)
				.Build();
	}
}

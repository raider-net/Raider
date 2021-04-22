using Raider.Identity;
using Raider.Localization;
using Raider.Trace;
using Raider.Web;
using System;

namespace Raider
{
	public interface IApplicationContext
	{
		ITraceInfo TraceInfo { get; }
		IAuthenticatedPrincipal AuthenticatedPrincipal { get; }
		IApplicationResources ApplicationResources { get; }
		IRequestMetadata? RequestMetadata { get; }
	}

	public class ApplicationContext : IApplicationContext
	{
		public ITraceInfo TraceInfo { get; }
		public IAuthenticatedPrincipal AuthenticatedPrincipal { get; }
		public IApplicationResources ApplicationResources { get; }
		public IRequestMetadata? RequestMetadata { get; set; }

		public ApplicationContext(ITraceInfo traceInfo, IApplicationResources applicationResources)
		{
			TraceInfo = traceInfo ?? throw new ArgumentNullException(nameof(traceInfo));
			ApplicationResources = applicationResources ?? throw new ArgumentNullException(nameof(applicationResources));
			AuthenticatedPrincipal = new AuthenticatedPrincipal();
		}
	}
}

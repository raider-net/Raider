using Raider.Identity;
using Raider.Localization;
using Raider.Trace;
using Raider.Web;
using System;

namespace Raider.Queries.Internal
{
	internal class QueryHandlerContextInternal : IQueryHandlerContext
	{
		public ITraceInfo TraceInfo { get; set; }
		public IApplicationContext ApplicationContext { get; }
		public IAuthenticatedPrincipal AuthenticatedPrincipal => ApplicationContext.AuthenticatedPrincipal;
		public IApplicationResources ApplicationResources => ApplicationContext.ApplicationResources;
		public RequestMetadata? RequestMetadata => ApplicationContext.RequestMetadata;
		public RaiderIdentity<int>? User => ApplicationContext.AuthenticatedPrincipal.User;

		public QueryHandlerContextInternal(ITraceInfo traceInfo, IApplicationContext applicationContext)
		{
			TraceInfo = traceInfo ?? throw new ArgumentNullException(nameof(traceInfo));
			ApplicationContext = applicationContext ?? throw new ArgumentNullException(nameof(applicationContext));
		}
	}
}

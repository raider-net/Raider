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
		public IApplicationResources ApplicationResources => ApplicationContext.ApplicationResources;
		public IRequestMetadata? RequestMetadata => ApplicationContext.RequestMetadata;
		public RaiderIdentity<int>? User => ApplicationContext.TraceInfo.User;

		public QueryHandlerContextInternal(ITraceInfo traceInfo, IApplicationContext applicationContext)
		{
			TraceInfo = traceInfo ?? throw new ArgumentNullException(nameof(traceInfo));
			ApplicationContext = applicationContext ?? throw new ArgumentNullException(nameof(applicationContext));
		}
	}
}

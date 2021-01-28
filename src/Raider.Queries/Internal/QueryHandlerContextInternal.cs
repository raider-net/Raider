using Raider.Identity;
using Raider.Localization;
using Raider.Trace;

namespace Raider.Queries.Internal
{
	internal class QueryHandlerContextInternal : IQueryHandlerContext
	{
		public ITraceInfo? TraceInfo { get; set; }

		public RaiderIdentity<int>? User { get; set; }

		public RaiderPrincipal<int>? Principal { get; set; }

		public IApplicationResources? ApplicationResources { get; set; }
	}
}

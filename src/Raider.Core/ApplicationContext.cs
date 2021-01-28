using Raider.Identity;
using Raider.Localization;
using Raider.Trace;

namespace Raider
{
	public interface IApplicationContext
	{
		ITraceInfo? TraceInfo { get; }
		RaiderIdentity<int>? User { get; }
		RaiderPrincipal<int>? Principal { get; }
		IApplicationResources? ApplicationResources { get; }
	}

	public class ApplicationContext : IApplicationContext
	{
		public ITraceInfo? TraceInfo { get; set; }
		public RaiderIdentity<int>? User { get; set; }
		public RaiderPrincipal<int>? Principal { get; set; }
		public IApplicationResources? ApplicationResources { get; set; }
	}
}

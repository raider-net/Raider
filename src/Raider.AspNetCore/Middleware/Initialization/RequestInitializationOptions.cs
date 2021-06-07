using System;
using System.Threading.Tasks;

namespace Raider.AspNetCore.Middleware.Initialization
{
	public class RequestInitializationOptions
	{
		public const string DefaultHeader = "X-Correlation-ID";

		public string Header { get; set; } = DefaultHeader;

		public bool UseCorrelationIdFromClient { get; set; } = false;

		public bool IncludeInResponse { get; set; } = true;

		public Func<IApplicationContext, Task>? OnRequestInitialized { get; set; }
	}
}

using Raider.Identity;
using Raider.Localization;
using Raider.Trace;
using Raider.Web;
using System;
using System.Threading.Tasks;

namespace Raider.Commands.Internal
{
	internal class CommandHandlerContextInternal : ICommandHandlerContext, IDisposable, IAsyncDisposable
	{
		public ITraceInfo TraceInfo { get; set; }
		public IApplicationContext ApplicationContext { get; }
		public IApplicationResources ApplicationResources => ApplicationContext.ApplicationResources;
		public IRequestMetadata? RequestMetadata => ApplicationContext.RequestMetadata;
		public RaiderIdentity<int>? User => ApplicationContext.TraceInfo.User;

		public bool IsDisposable { get; set; }

		public CommandHandlerContextInternal(ITraceInfo traceInfo, IApplicationContext applicationContext)
		{
			TraceInfo = traceInfo ?? throw new ArgumentNullException(nameof(traceInfo));
			ApplicationContext = applicationContext ?? throw new ArgumentNullException(nameof(applicationContext));
		}

		public ValueTask DisposeAsync()
#if NET5_0_OR_GREATER
			=> ValueTask.CompletedTask;
#else
			=> new ValueTask();
#endif

		public void Dispose()
		{
		}
	}
}

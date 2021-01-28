using Raider.DependencyInjection;
using Raider.Localization;
using Raider.Trace;
using System;

namespace Raider.Commands.Internal
{
	internal abstract class CommandProcessorBase
	{
		protected ServiceFactory ServiceFactory { get; }
		protected IApplicationContext ApplicationContext { get; }
		protected IApplicationResources ApplicationResources { get; }

		public abstract ICommandHandler CreateHandler();

		public abstract void DisposeHandler(ICommandHandler? handler);

		public CommandProcessorBase(ServiceFactory serviceFactory)
		{
			ServiceFactory = serviceFactory ?? throw new ArgumentNullException(nameof(serviceFactory));
			ApplicationContext = ServiceFactory.GetRequiredInstance<IApplicationContext>();
			ApplicationResources = ServiceFactory.GetRequiredInstance<IApplicationResources>();
		}

		protected ICommandHandlerContext CreateCommandHandlerContext(ITraceInfo traceInfo)
		{
			return new CommandHandlerContextInternal
			{
				TraceInfo = traceInfo,
				Principal = ApplicationContext.Principal,
				User = ApplicationContext.User,
				ApplicationResources = ApplicationResources
			};
		}
	}
}

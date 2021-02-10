using Raider.Localization;
using Raider.Trace;
using System;

namespace Raider.Commands.Internal
{
	internal abstract class CommandProcessorBase
	{
		public abstract ICommandHandler CreateHandler(ICommandHandlerFactory handlerFactory);

		public abstract void DisposeHandler(ICommandHandlerFactory handlerFactory, ICommandHandler? handler);

		protected ICommandHandlerContext CreateCommandHandlerContext(ITraceInfo traceInfo, IApplicationContext applicationContext, IApplicationResources applicationResources)
		{
			if (applicationContext == null)
				throw new ArgumentNullException(nameof(applicationContext));
			
			return new CommandHandlerContextInternal
			{
				TraceInfo = traceInfo,
				Principal = applicationContext.Principal,
				User = applicationContext.User,
				ApplicationResources = applicationResources ?? throw new ArgumentNullException(nameof(applicationResources))
			};
		}
	}
}

using Raider.Trace;

namespace Raider.Commands.Internal
{
	internal abstract class CommandProcessorBase
	{
		public abstract ICommandHandler CreateHandler(ICommandHandlerFactory handlerFactory);

		public abstract void DisposeHandler(ICommandHandlerFactory handlerFactory, ICommandHandler? handler);

		protected ICommandHandlerContext CreateCommandHandlerContext(ITraceInfo traceInfo, IApplicationContext applicationContext)
			=> new CommandHandlerContextInternal(traceInfo, applicationContext);
	}
}

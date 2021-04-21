using Raider.Trace;

namespace Raider.Queries.Internal
{
	internal abstract class QueryProcessorBase
	{
		public abstract IQueryHandler CreateHandler(IQueryHandlerFactory handlerFactory);

		public abstract void DisposeHandler(IQueryHandlerFactory handlerFactory, IQueryHandler? handler);

		protected IQueryHandlerContext CreateQueryHandlerContext(ITraceInfo traceInfo, IApplicationContext applicationContext)
			=> new QueryHandlerContextInternal(traceInfo, applicationContext);
	}
}

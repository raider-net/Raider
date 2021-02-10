using Raider.Localization;
using Raider.Trace;
using System;

namespace Raider.Queries.Internal
{
	internal abstract class QueryProcessorBase
	{
		public abstract IQueryHandler CreateHandler(IQueryHandlerFactory handlerFactory);

		public abstract void DisposeHandler(IQueryHandlerFactory handlerFactory, IQueryHandler? handler);

		protected IQueryHandlerContext CreateQueryHandlerContext(ITraceInfo traceInfo, IApplicationContext applicationContext, IApplicationResources applicationResources)
		{
			if (applicationContext == null)
				throw new ArgumentNullException(nameof(applicationContext));

			return new QueryHandlerContextInternal
			{
				TraceInfo = traceInfo,
				Principal = applicationContext.Principal,
				User = applicationContext.User,
				ApplicationResources = applicationResources ?? throw new ArgumentNullException(nameof(applicationResources))
			};
		}
	}
}

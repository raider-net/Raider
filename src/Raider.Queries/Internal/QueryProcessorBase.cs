using Raider.DependencyInjection;
using Raider.Localization;
using Raider.Trace;
using System;

namespace Raider.Queries.Internal
{
	internal abstract class QueryProcessorBase
	{
		protected ServiceFactory ServiceFactory { get; }
		protected IApplicationContext ApplicationContext { get; }
		protected IApplicationResources ApplicationResources { get; }

		public abstract IQueryHandler CreateHandler();

		public abstract void DisposeHandler(IQueryHandler? handler);

		public QueryProcessorBase(ServiceFactory serviceFactory)
		{
			ServiceFactory = serviceFactory ?? throw new ArgumentNullException(nameof(serviceFactory));
			ApplicationContext = ServiceFactory.GetRequiredInstance<IApplicationContext>();
			ApplicationResources = ServiceFactory.GetRequiredInstance<IApplicationResources>();
		}

		protected IQueryHandlerContext CreateQueryHandlerContext(ITraceInfo traceInfo)
		{
			return new QueryHandlerContextInternal
			{
				TraceInfo = traceInfo,
				Principal = ApplicationContext.Principal,
				User = ApplicationContext.User,
				ApplicationResources = ApplicationResources
			};
		}
	}
}

using Microsoft.Extensions.Logging;
using Raider.DependencyInjection;
using Raider.Localization;
using Raider.QueryServices.Queries;
using Raider.Trace;
using System;

namespace Raider.QueryServices.Aspects
{
	public abstract class InterceptorBase<TContext, TBuilder>
		where TContext : QueryHandlerContext
		where TBuilder : QueryHandlerContext.Builder<TContext>
	{
		protected ServiceFactory ServiceFactory { get; }
		protected IApplicationContext ApplicationContext { get; }
		protected IApplicationResources ApplicationResources { get; }

		public InterceptorBase(ServiceFactory serviceFactory)
			: base()
		{
			ServiceFactory = serviceFactory ?? throw new ArgumentNullException(nameof(serviceFactory));
			ApplicationContext = ServiceFactory.GetRequiredInstance<IApplicationContext>();
			ApplicationResources = ServiceFactory.GetRequiredInstance<IApplicationResources>();
		}

		protected TBuilder CreateQueryHandlerContext(ITraceInfo traceInfo, ILogger logger)
		{
			var queryHandlerContextBuilder = ServiceFactory.GetRequiredInstance<TBuilder>();

			queryHandlerContextBuilder
				.TraceInfo(traceInfo)
				.Principal(ApplicationContext.Principal)
				.User(ApplicationContext.User)
				.Logger(logger)
				.ApplicationResources(ApplicationResources);

			return queryHandlerContextBuilder;
		}
	}
}

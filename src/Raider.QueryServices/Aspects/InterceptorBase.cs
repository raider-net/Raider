using Microsoft.Extensions.Logging;
using Raider.DependencyInjection;
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

		public InterceptorBase(ServiceFactory serviceFactory)
			: base()
		{
			ServiceFactory = serviceFactory ?? throw new ArgumentNullException(nameof(serviceFactory));
			ApplicationContext = ServiceFactory.GetRequiredInstance<IApplicationContext>();
		}

		protected TBuilder CreateQueryHandlerContext(ITraceInfo traceInfo, ILogger logger)
		{
			var queryHandlerContextBuilder = ServiceFactory.GetRequiredInstance<TBuilder>();
			var applicationContext = ServiceFactory.GetRequiredInstance<IApplicationContext>();

			queryHandlerContextBuilder
				.TraceInfo(traceInfo)
				.ApplicationContext(applicationContext)
				.Logger(logger);

			return queryHandlerContextBuilder;
		}
	}
}

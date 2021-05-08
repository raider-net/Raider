using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Raider.QueryServices.Queries;
using Raider.Trace;
using System;

namespace Raider.QueryServices.Aspects
{
	public abstract class InterceptorBase<TContext, TBuilder>
		where TContext : QueryHandlerContext
		where TBuilder : QueryHandlerContext.Builder<TContext>
	{
		protected IServiceProvider ServiceProvider { get; }
		protected IApplicationContext ApplicationContext { get; }

		public InterceptorBase(IServiceProvider serviceProvider)
			: base()
		{
			ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
			ApplicationContext = ServiceProvider.GetRequiredService<IApplicationContext>();
		}

		protected TBuilder CreateQueryHandlerContext(ITraceInfo traceInfo, ILogger logger)
		{
			var queryHandlerContextBuilder = ServiceProvider.GetRequiredService<TBuilder>();
			var applicationContext = ServiceProvider.GetRequiredService<IApplicationContext>();

			queryHandlerContextBuilder
				.TraceInfo(traceInfo)
				.ApplicationContext(applicationContext)
				.Logger(logger);

			return queryHandlerContextBuilder;
		}
	}
}

using Microsoft.Extensions.Logging;
using Raider.DependencyInjection;
using Raider.Services.Commands;
using Raider.Trace;
using System;

namespace Raider.Services.Aspects
{
	public abstract class InterceptorBase<TContext, TBuilder>
		where TContext : CommandHandlerContext
		where TBuilder : CommandHandlerContext.Builder<TContext>
	{
		protected ServiceFactory ServiceFactory { get; }
		protected IApplicationContext ApplicationContext { get; }

		public InterceptorBase(ServiceFactory serviceFactory)
			: base()
		{
			ServiceFactory = serviceFactory ?? throw new ArgumentNullException(nameof(serviceFactory));
			ApplicationContext = ServiceFactory.GetRequiredInstance<IApplicationContext>();
		}

		protected TBuilder CreateCommandHandlerContext(ITraceInfo traceInfo, ILogger logger)
		{
			var commandHandlerContextBuilder = ServiceFactory.GetRequiredInstance<TBuilder>();
			var applicationContext = ServiceFactory.GetRequiredInstance<IApplicationContext>();

			commandHandlerContextBuilder
				.TraceInfo(traceInfo)
				.ApplicationContext(applicationContext)
				.Logger(logger);

			return commandHandlerContextBuilder;
		}
	}
}

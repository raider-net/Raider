using Microsoft.Extensions.Logging;
using Raider.DependencyInjection;
using Raider.Localization;
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
		protected IApplicationResources ApplicationResources { get; }

		public InterceptorBase(ServiceFactory serviceFactory)
			: base()
		{
			ServiceFactory = serviceFactory ?? throw new ArgumentNullException(nameof(serviceFactory));
			ApplicationContext = ServiceFactory.GetRequiredInstance<IApplicationContext>();
			ApplicationResources = ServiceFactory.GetRequiredInstance<IApplicationResources>();
		}

		protected TBuilder CreateCommandHandlerContext(ITraceInfo traceInfo, ILogger logger)
		{
			var commandHandlerContextBuilder = ServiceFactory.GetRequiredInstance<TBuilder>();

			commandHandlerContextBuilder
				.TraceInfo(traceInfo)
				.Principal(ApplicationContext.Principal)
				.User(ApplicationContext.User)
				.Logger(logger)
				.ApplicationResources(ApplicationResources);

			return commandHandlerContextBuilder;
		}
	}
}

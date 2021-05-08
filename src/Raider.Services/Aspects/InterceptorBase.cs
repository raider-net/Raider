using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Raider.Services.Commands;
using Raider.Trace;
using System;

namespace Raider.Services.Aspects
{
	public abstract class InterceptorBase<TContext, TBuilder>
		where TContext : CommandHandlerContext
		where TBuilder : CommandHandlerContext.Builder<TContext>
	{
		protected IServiceProvider ServiceProvider { get; }
		protected IApplicationContext ApplicationContext { get; }

		public InterceptorBase(IServiceProvider serviceProvider)
			: base()
		{
			ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
			ApplicationContext = ServiceProvider.GetRequiredService<IApplicationContext>();
		}

		protected TBuilder CreateCommandHandlerContext(ITraceInfo traceInfo, ILogger logger)
		{
			var commandHandlerContextBuilder = ServiceProvider.GetRequiredService<TBuilder>();
			var applicationContext = ServiceProvider.GetRequiredService<IApplicationContext>();

			commandHandlerContextBuilder
				.TraceInfo(traceInfo)
				.ApplicationContext(applicationContext)
				.Logger(logger);

			return commandHandlerContextBuilder;
		}
	}
}

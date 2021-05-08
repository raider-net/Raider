using Microsoft.Extensions.DependencyInjection;
using Raider.Services.Commands;
using System;

namespace Raider.Services
{
	public class SingleServiceBase<THandlerContext, TBuilder, TServiceContext> : ServiceBase<TServiceContext>
			where TServiceContext : ServiceContext, new()
			where THandlerContext : CommandHandlerContext
			where TBuilder : CommandHandlerContext.Builder<THandlerContext>
	{
		public SingleServiceBase(IServiceProvider serviceProvider)
		{
			if (serviceProvider == null)
				throw new ArgumentNullException(nameof(serviceProvider));

			var contextFactory = serviceProvider.GetRequiredService<ContextFactory>();
			ServiceContext = contextFactory.CreateServiceContext<THandlerContext, TBuilder, TServiceContext>(this.GetType());
		}
	}
}

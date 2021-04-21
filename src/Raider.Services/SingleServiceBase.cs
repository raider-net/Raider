using Raider.DependencyInjection;
using Raider.Services.Commands;
using System;

namespace Raider.Services
{
	public class SingleServiceBase<THandlerContext, TBuilder, TServiceContext> : ServiceBase<TServiceContext>
			where TServiceContext : ServiceContext, new()
			where THandlerContext : CommandHandlerContext
			where TBuilder : CommandHandlerContext.Builder<THandlerContext>
	{
		public SingleServiceBase(ServiceFactory serviceFactory)
		{
			if (serviceFactory == null)
				throw new ArgumentNullException(nameof(serviceFactory));

			var contextFactory = serviceFactory.GetRequiredInstance<ContextFactory>();
			ServiceContext = contextFactory.CreateServiceContext<THandlerContext, TBuilder, TServiceContext>(this.GetType());
		}
	}
}

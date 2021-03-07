using Raider.DependencyInjection;
using Raider.Services.Commands;
using System;

namespace Raider.Services
{
	public class SingleServiceBase<THandlerContext, TBuilder> : ServiceBase
			where THandlerContext : CommandHandlerContext
			where TBuilder : CommandHandlerContext.Builder<THandlerContext>
	{
		public SingleServiceBase(ServiceFactory serviceFactory, bool alowAnonymousUser)
		{
			if (serviceFactory == null)
				throw new ArgumentNullException(nameof(serviceFactory));

			var contextFactory = serviceFactory.GetRequiredInstance<ContextFactory>();
			ServiceContext = contextFactory.CreateServiceContext<THandlerContext, TBuilder>(this.GetType(), alowAnonymousUser);
		}
	}
}

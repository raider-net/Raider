using Raider.DependencyInjection;
using Raider.QueryServices.Queries;
using System;

namespace Raider.QueryServices
{
	public class SingleQueryServiceBase<THandlerContext, TBuilder> : QueryServiceBase
			where THandlerContext : QueryHandlerContext
			where TBuilder : QueryHandlerContext.Builder<THandlerContext>
	{
		public SingleQueryServiceBase(ServiceFactory serviceFactory)
		{
			if (serviceFactory == null)
				throw new ArgumentNullException(nameof(serviceFactory));

			var contextFactory = serviceFactory.GetRequiredInstance<ContextFactory>();
			QueryServiceContext = contextFactory.CreateQueryServiceContext<THandlerContext, TBuilder>(this.GetType());
		}
	}
}

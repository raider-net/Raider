using Microsoft.Extensions.DependencyInjection;
using Raider.QueryServices.Queries;
using System;

namespace Raider.QueryServices
{
	public class SingleQueryServiceBase<THandlerContext, TBuilder, TQueryServiceContext> : QueryServiceBase<TQueryServiceContext>
			where TQueryServiceContext : QueryServiceContext, new()
			where THandlerContext : QueryHandlerContext
			where TBuilder : QueryHandlerContext.Builder<THandlerContext>
	{
		public SingleQueryServiceBase(IServiceProvider serviceProvider)
		{
			if (serviceProvider == null)
				throw new ArgumentNullException(nameof(serviceProvider));

			var contextFactory = serviceProvider.GetRequiredService<ContextFactory>();
			QueryServiceContext = contextFactory.CreateQueryServiceContext<THandlerContext, TBuilder, TQueryServiceContext>(this.GetType());
		}
	}
}

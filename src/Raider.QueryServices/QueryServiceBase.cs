using Raider.DependencyInjection;
using Raider.QueryServices.Queries;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Raider.QueryServices
{
	public abstract class QueryServiceBase<TQueryServiceContext>
		where TQueryServiceContext : QueryServiceContext, new()
	{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
		public TQueryServiceContext QueryServiceContext { get; internal set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

		public virtual void Initialize() { }

		public virtual Task InitializeAsync(CancellationToken cancellationToken = default)
			=> Task.CompletedTask;

		protected void SetQueryServiceContext(TQueryServiceContext serviceContext)
		{
			QueryServiceContext = serviceContext ?? throw new ArgumentNullException(nameof(serviceContext));
		}

		protected void SetQueryServiceContext<THandlerContext, TBuilder>(ServiceFactory serviceFactory, Type serviceType)
			where THandlerContext : QueryHandlerContext
			where TBuilder : QueryHandlerContext.Builder<THandlerContext>
		{
			if (serviceFactory == null)
				throw new ArgumentNullException(nameof(serviceFactory));

			if (serviceType == null)
				throw new ArgumentNullException(nameof(serviceType));

			var contextFactory = serviceFactory.GetRequiredInstance<ContextFactory>();
			QueryServiceContext = contextFactory.CreateQueryServiceContext<THandlerContext, TBuilder, TQueryServiceContext>(serviceType);
		}
	}
}

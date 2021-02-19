using Microsoft.EntityFrameworkCore;
using Raider.DependencyInjection;
using Raider.Services.Commands;
using System;
using System.Linq;

namespace Raider.Services
{
	public abstract class QueryableBase<T, TDbContext> : ServiceBase
		where TDbContext : DbContext
	{
		private readonly ServiceFactory _serviceFactory;
		protected TDbContext DbContext { get; private set; }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
		public QueryableBase(ServiceFactory serviceFactory)
		{
			_serviceFactory = serviceFactory ?? throw new ArgumentNullException(nameof(serviceFactory));
		}

		public QueryableBase(ServiceContext serviceContext)
		{
			ServiceContext = serviceContext ?? throw new ArgumentNullException(nameof(serviceContext));
			SetDbContext(ServiceContext.GetOrCreateDbContext<TDbContext>());
		}
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

		protected void SetDbContext(TDbContext dbContext)
		{
			DbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
		}

		protected void SetDbContext<THandlerContext, TBuilder>()
			where THandlerContext : CommandHandlerContext
			where TBuilder : CommandHandlerContext.Builder<THandlerContext>
		{
			var contextFactory = _serviceFactory.GetRequiredInstance<ContextFactory>();
			ServiceContext = contextFactory.CreateServiceContext<THandlerContext, TBuilder>(this.GetType());
			SetDbContext(ServiceContext.GetOrCreateDbContext<TDbContext>());
		}

		public abstract IQueryable<T> Default(Func<IQueryable<T>, IQueryable<T>>? queryableConfigurator = null);

		public abstract IQueryable<T> WithAcl(Func<IQueryable<T>, IQueryable<T>>? queryableConfigurator = null);
	}
}

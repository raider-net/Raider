﻿using Microsoft.Extensions.DependencyInjection;
using Raider.Services.Commands;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Raider.Services
{
	public abstract class ServiceBase<TServiceContext>
		where TServiceContext : ServiceContext, new()
	{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
		public TServiceContext ServiceContext { get; internal set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

		public virtual void Initialize() { }

		public virtual Task InitializeAsync(CancellationToken cancellationToken = default)
			=> Task.CompletedTask;

		protected void SetServiceContext(TServiceContext serviceContext)
		{
			ServiceContext = serviceContext ?? throw new ArgumentNullException(nameof(serviceContext));
		}

		protected void SetServiceContext<THandlerContext, TBuilder>(IServiceProvider serviceProvider, Type serviceType)
			where THandlerContext : CommandHandlerContext
			where TBuilder : CommandHandlerContext.Builder<THandlerContext>
		{
			if (serviceProvider == null)
				throw new ArgumentNullException(nameof(serviceProvider));

			if (serviceType == null)
				throw new ArgumentNullException(nameof(serviceType));

			var contextFactory = serviceProvider.GetRequiredService<ContextFactory>();
			ServiceContext = contextFactory.CreateServiceContext<THandlerContext, TBuilder, TServiceContext>(serviceType);
		}
	}
}

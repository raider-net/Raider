using Microsoft.Extensions.DependencyInjection;
using Raider.QueryServices.Queries;
using System;

namespace Raider.QueryServices.PostgreSql.Extensions
{
	public static class ServiceCollectionExtensions
	{
		public static IServiceCollection AddQueryLogger(this IServiceCollection services, Action<QueryEntryOptions> entryOptions, Action<QueryExitOptions> exitOptions)
		{
			var entryOpt = new QueryEntryOptions();
			entryOptions?.Invoke(entryOpt);
			QueryLogger.SetEntryWriter(new QueryEntryWriter(entryOpt));

			var exitOpt = new QueryExitOptions();
			exitOptions?.Invoke(exitOpt);
			QueryLogger.SetExitWriter(new QueryExitWriter(exitOpt));

			services.AddSingleton<IQueryLogger, QueryLogger>();

			return services;
		}
	}
}

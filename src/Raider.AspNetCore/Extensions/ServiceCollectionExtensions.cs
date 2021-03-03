using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Raider.Trace;
using System;
using System.Linq;

namespace Raider.AspNetCore.Extensions
{
	public static class ServiceCollectionExtensions
	{
		public static IServiceCollection AddRaiderMiddlewareContext(this IServiceCollection services)
		{
			services.AddScoped<TraceContext>();

			return services;
		}
	}
}

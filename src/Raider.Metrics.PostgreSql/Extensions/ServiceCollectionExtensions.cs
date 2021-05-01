using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;

namespace Raider.Metrics.PostgreSql.Extensions
{
	public static class ServiceCollectionExtensions
	{
		public static IServiceCollection AddEventSourcesWriter(this IServiceCollection services,
			DBEventCounterDataWriterOptions options,
			List<EventSourceOptions> eventSourceOptions,
			List<IEventListener>? customListeners = null)
		{
			if (eventSourceOptions == null || eventSourceOptions.Count == 0)
				throw new ArgumentNullException(nameof(eventSourceOptions));

			var writer = new EventCounterDataWriter(options, eventSourceOptions, customListeners);
			services.TryAddSingleton<IEventCounterDataWriter>(writer);

			return services;
		}
	}
}

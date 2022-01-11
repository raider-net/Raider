using Microsoft.Extensions.DependencyInjection;
using Raider.NetHttp;
using System;
using System.Net.Http;

namespace Raider.Extensions
{
	public static class ServiceCollectionExtensions
	{
		public static IServiceCollection AddHttpApiClient<TClient>(this IServiceCollection services,
			Action<HttpApiClientOptions>? configureOptions,
			Action<HttpClient>? configureClient = null,
			Action<IHttpClientBuilder>? configureHttpClientBuilder = null)
			where TClient : HttpApiClient
		{
			var options = new HttpApiClientOptions();
			configureOptions?.Invoke(options);

			var error = options.Validate()?.ToString();
			if (!string.IsNullOrWhiteSpace(error))
				throw new InvalidOperationException(error);

			services.Configure<HttpApiClientOptions>(opt =>
			{
				configureOptions?.Invoke(opt);
			});

			services.AddTransient<LogHandler>();
			services.AddTransient<PolicyHandler>();

			var httpClientBuilder =
				services.AddHttpClient<TClient>(httpClient =>
				{
					httpClient.DefaultRequestHeaders.Clear();

					if (!string.IsNullOrWhiteSpace(options?.BaseAddress))
						httpClient.BaseAddress = new Uri(options.BaseAddress);

					if (!string.IsNullOrWhiteSpace(options?.UserAgent))
						httpClient.DefaultRequestHeaders.Add("User-Agent", $"{options.UserAgent}{(options.Version == null ? "" : $" v{options.Version}")}");

					configureClient?.Invoke(httpClient);
				});

			//na konci, aby to bol najviac outer handler
			httpClientBuilder
				.AddHttpMessageHandler<PolicyHandler>();

			configureHttpClientBuilder?.Invoke(httpClientBuilder);

			//na konci, aby to bol najviac inner handler
			httpClientBuilder
				.AddHttpMessageHandler<LogHandler>();

			if (options.AutomaticDecompression.HasValue
				|| options.Proxy != null
				|| options.UseDefaultCredentials.HasValue
				|| options.Credentials != null)
			{
				httpClientBuilder
					.ConfigurePrimaryHttpMessageHandler(
						serviceProvider =>
						{
							var handler = new System.Net.Http.HttpClientHandler();

							if (options.AutomaticDecompression.HasValue)
								handler.AutomaticDecompression = options.AutomaticDecompression.Value;

							if (options.Proxy != null)
								handler.Proxy = options.Proxy;

							if (options.UseDefaultCredentials.HasValue)
								handler.UseDefaultCredentials = options.UseDefaultCredentials.Value;

							if (options.Credentials != null)
								handler.Credentials = options.Credentials;

							return handler;
						});
			}

			return services;
		}
	}
}

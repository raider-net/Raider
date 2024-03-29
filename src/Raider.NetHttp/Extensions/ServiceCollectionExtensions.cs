﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Raider.NetHttp;
using System;
using System.Net.Http;

namespace Raider.Extensions
{
	public static class ServiceCollectionExtensions
	{
		public static IServiceCollection AddHttpApiClient<TClient, TOptions>(this IServiceCollection services,
			Action<TOptions>? configureOptions,
			Action<HttpClient>? configureClient = null,
			Action<IHttpClientBuilder>? configureHttpClientBuilder = null)
			where TClient : HttpApiClient
			where TOptions : HttpApiClientOptions, new()
		{
			var options = new TOptions();
			configureOptions?.Invoke(options);

			var error = options.Validate()?.ToString();
			if (!string.IsNullOrWhiteSpace(error))
				throw new InvalidOperationException(error);

			services.Configure<TOptions>(opt =>
			{
				configureOptions?.Invoke(opt);
			});

			services.TryAddTransient<LogHandler<TOptions>>();
			services.TryAddTransient<PolicyHandler<TOptions>>();

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
				.AddHttpMessageHandler<PolicyHandler<TOptions>>();

			configureHttpClientBuilder?.Invoke(httpClientBuilder);

			//na konci, aby to bol najviac inner handler
			httpClientBuilder
				.AddHttpMessageHandler<LogHandler<TOptions>>();

			if (options.ApplyToHttpClientHandler)
			{
				httpClientBuilder
					.ConfigurePrimaryHttpMessageHandler(
						serviceProvider =>
						{
							var handler = new System.Net.Http.HttpClientHandler();

							options.ConfigureHttpClientHandler(handler);

							return handler;
						});
			}

			return services;
		}
	}
}

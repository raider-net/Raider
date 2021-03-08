using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Raider.AspNetCore.Authentication;
using Raider.AspNetCore.Middleware.Authentication;
using Raider.AspNetCore.Middleware.Authentication.Authenticate;
using Raider.AspNetCore.Middleware.Authorization;
using Raider.AspNetCore.Middleware.HostNormalizer;
using Raider.AspNetCore.Middleware.Initialization;
using Raider.AspNetCore.Middleware.Tracking;
using Raider.Identity;
using Raider.Trace;
using System;

namespace Raider.AspNetCore.Extensions
{
	public static class ServiceCollectionExtensions
	{
		public static IServiceCollection AddTraceContext(this IServiceCollection services)
			=> services.AddScoped<TraceContext>();

		public static IServiceCollection AddRaiderAuthentication<TAuthMngr>(this IServiceCollection services, Action<AuthenticationOptions>? configureAuthenticationOptions)
			where TAuthMngr : class, IAuthenticationManager
		{
			if (configureAuthenticationOptions == null)
				return services;

			var authenticationBuilder =
				services.AddAuthentication(opt =>
				{
					opt.DefaultAuthenticateScheme = AuthenticationDefaults.AuthenticationScheme;
					opt.DefaultChallengeScheme = AuthenticationDefaults.AuthenticationScheme;
					opt.DefaultForbidScheme = AuthenticationDefaults.AuthenticationScheme;
				});

			authenticationBuilder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IPostConfigureOptions<AuthenticationOptions>, PostConfigureAuthenticationOptions>());
			authenticationBuilder.AddScheme<AuthenticationOptions, RaiderAuthenticationHandler>(
				AuthenticationDefaults.AuthenticationScheme,
				displayName: null,
				configureOptions: configureAuthenticationOptions);

			services.AddScoped<IAuthenticationManager, TAuthMngr>();
			services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();
			services.AddSingleton<IAuthorizationHandler, AuthenticateAuthorizationHandler>();

			services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
			services.AddScoped(sp => (sp.GetRequiredService<IHttpContextAccessor>().HttpContext?.User as RaiderPrincipal<int>) ?? new RaiderPrincipal<int>());

			return services;
		}

		public static IServiceCollection ConfigureRaiderMiddlewares<TAuth>(
			this IServiceCollection services,
			Action<RequestInitializationOptions>? configureRequestInitializationOptions = null,
			Action<ForwardedHeadersOptions>? configureForwardedHeadersOptions = null,
			Action<ExceptionHandlerOptions>? configureExceptionHandlerOptions = null,
			Action<HostNormalizerOptions>? configureHostNormalizerOptions = null,
			Action<RequestTrackingOptions>? configureRequestTracking = null,
			Action<AuthenticationOptions>? configureAuthenticationOptions = null)
			where TAuth : class, IAuthenticationManager
		{
			AddTraceContext(services);

			if (configureRequestInitializationOptions != null)
				services.Configure(configureRequestInitializationOptions);

			if (configureForwardedHeadersOptions != null)
				services.Configure(configureForwardedHeadersOptions);

			if (configureExceptionHandlerOptions != null)
				services.Configure(configureExceptionHandlerOptions);

			if (configureHostNormalizerOptions != null)
				services.Configure(configureHostNormalizerOptions);

			if (configureRequestTracking != null)
				services.Configure(configureRequestTracking);

			if (configureAuthenticationOptions != null)
				AddRaiderAuthentication<TAuth>(services, configureAuthenticationOptions);

			return services;
		}
	}
}

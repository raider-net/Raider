using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Raider.AspNetCore.Authentication;
using Raider.AspNetCore.Middleware.Authentication;
using Raider.AspNetCore.Middleware.Authorization;
using Raider.AspNetCore.Middleware.HostNormalizer;
using Raider.AspNetCore.Middleware.Initialization;
using Raider.AspNetCore.Middleware.Tracking;
using Raider.Extensions;
using Raider.Identity;
using Raider.Localization;
using Raider.Trace;
using System;

namespace Raider.AspNetCore.Extensions
{
	public static class ServiceCollectionExtensions
	{
		public static IServiceCollection AddApplicationContext(this IServiceCollection services,
			bool withQueryList = false,
			bool withCookies = true,
			bool withHeaders = true,
			bool withForm = false)
			=> services.AddScoped<IApplicationContext>(sp =>
			{
				var httpContext = sp.GetRequiredService<IHttpContextAccessor>().HttpContext;

				var traceFrame = TraceFrame.Create();
				ITraceInfo traceInfo;

				if (httpContext == null)
				{
					traceInfo = new TraceInfoBuilder(traceFrame, null)
						.CorrelationId(Guid.NewGuid())
						.ExternalCorrelationId(Guid.NewGuid().ToString("D"))
						.Build();
				}
				else
				{
					traceInfo = new TraceInfoBuilder(traceFrame, null)
						.CorrelationId(Guid.NewGuid())
						.ExternalCorrelationId(httpContext.TraceIdentifier)
						.Principal(httpContext.User)
						.Build();
				}

				var appResources = sp.GetRequiredService<IApplicationResources>();
				var requsetMetadata = httpContext?.Request.ToRequestMetadata(
					withQueryList: withQueryList,
					withCookies: withCookies,
					withHeaders: withHeaders,
					withForm: withForm,
					cookieDataProtectionPurposes: AuthenticationService.GetDataProtectors(httpContext));
				var appCtx = new ApplicationContext(traceInfo, appResources, requsetMetadata);
				return appCtx;
			});

		public static IServiceCollection AddRaiderAuthentication<TAuthMngr, TCookieStore>(this IServiceCollection services, Action<AuthenticationOptions>? configureAuthenticationOptions)
			where TAuthMngr : class, IAuthenticationManager
			where TCookieStore : class, ICookieStore
		{
			services.TryAddSingleton<ICookieStore, TCookieStore>();
			return AddRaiderAuthentication<TAuthMngr>(services, configureAuthenticationOptions, null);
		}

		public static IServiceCollection AddRaiderAuthentication<TAuthMngr>(this IServiceCollection services, Action<AuthenticationOptions>? configureAuthenticationOptions, ICookieStore? cookieStore = null)
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

			services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
			services.AddScoped(sp => (sp.GetRequiredService<IHttpContextAccessor>().HttpContext?.User as RaiderPrincipal<int>) ?? new RaiderPrincipal<int>());

			if (cookieStore != null)
				services.TryAddSingleton(cookieStore);

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
			AddApplicationContext(services);

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
				AddRaiderAuthentication<TAuth>(services, configureAuthenticationOptions, null);

			return services;
		}
	}
}

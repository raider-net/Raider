using Microsoft.AspNetCore.Builder;
using Raider.AspNetCore.Middleware.Exceptions;
using Raider.AspNetCore.Middleware.HostNormalizer;
using Raider.AspNetCore.Middleware.Initialization;
using Raider.AspNetCore.Middleware.Tracking;
using Raider.Logging.Database.PostgreSql;
using System;

namespace Raider.AspNetCore.Extensions
{
	public static class ApplicationBuilderExtensions
	{
		public static IApplicationBuilder UseRaiderAspNetCore(this IApplicationBuilder app)
		{
			if (app == null)
				throw new ArgumentNullException(nameof(app));

			//var loggerFactory = app.ApplicationServices.GetRequiredService<ILoggerFactory>();
			//var logger = loggerFactory.CreateLogger<RequestInitializationMiddleware>();
			//logger.LogEnvironmentInfo();

			DbLogWriter.Instance.WriteEnvironmentInfo();

			app.UseMiddleware<RequestInitializationMiddleware>();

			return app;
		}

		public static IApplicationBuilder UseRaiderExceptionHandler(this IApplicationBuilder app)
		{
			if (app == null)
				throw new ArgumentNullException(nameof(app));

			app.UseMiddleware<ExceptionHandlerMiddleware>();
			return app;
		}

		public static IApplicationBuilder UseRaiderHostNormalizer(this IApplicationBuilder app)
		{
			if (app == null)
				throw new ArgumentNullException(nameof(app));

			app.UseMiddleware<HostNormalizerMiddleware>();
			return app;
		}

		public static IApplicationBuilder UseRaiderRequestTracking(this IApplicationBuilder app)
		{
			if (app == null)
				throw new ArgumentNullException(nameof(app));

			app.UseMiddleware<RequestTrackingMiddleware>();
			return app;
		}

		/// <summary>
		/// 0. UseRaiderAspNetCore
		/// 1. RequestInitializationMiddleware
		/// 2. UseForwardedHeaders
		/// 3. UseStaticFiles
		/// 4. ExceptionHandlerMiddleware
		/// 5. HostNormalizerMiddleware
		/// 6. RequestTrackingMiddleware
		/// 7. UseAuthentication
		/// </summary>
		/// <returns></returns>
		//public static IApplicationBuilder UseRaiderAspNetCore(
		//	this IApplicationBuilder app,
		//	bool useForwardedHeaders,
		//	ForwardedHeadersOptions forwardedHeadersOptions,
		//	bool useStaticFiles,
		//	StaticFileOptions staticFileOptions,
		//	bool useHostNormalizer)
		//{
		//	app.UseRaiderAspNetCore();

		//	if (useForwardedHeaders)
		//	{
		//		if (forwardedHeadersOptions == null)
		//			app.UseForwardedHeaders();
		//		else
		//			app.UseForwardedHeaders(forwardedHeadersOptions);
		//	}

		//	if (useStaticFiles)
		//	{
		//		if (staticFileOptions == null)
		//			app.UseStaticFiles();
		//		else
		//			app.UseStaticFiles(staticFileOptions);
		//	}

		//	app.UseRaiderExceptionHandler();

		//	if (useHostNormalizer)
		//		app.UseRaiderHostNormalizer();

		//	app.UseRaiderRequestTracking();

		//	app.UseAuthentication();

		//	return app;
		//}
	}
}

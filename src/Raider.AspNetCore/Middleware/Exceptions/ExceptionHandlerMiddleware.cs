using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using Raider.AspNetCore.Logging;
using Raider.Localization;
using Raider.Logging.Extensions;
using Raider.Trace;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace Raider.AspNetCore.Middleware.Exceptions
{
	public class ExceptionHandlerMiddleware
	{
		private readonly RequestDelegate _next;
		private readonly ExceptionHandlerOptions _options;
		private readonly ILogger _logger;

		public ExceptionHandlerMiddleware(
			RequestDelegate next,
			IOptions<ExceptionHandlerOptions> options,
			ILogger<ExceptionHandlerMiddleware> logger)
		{
			_next = next ?? throw new ArgumentNullException(nameof(next));
			_options = options?.Value ?? throw new ArgumentNullException(nameof(options));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		public async Task Invoke(HttpContext context)
		{
			var tc = context.RequestServices.GetRequiredService<TraceContext>();
			var traceInfo = tc.AddTraceFrame(nameof(ExceptionHandlerMiddleware), TraceFrame.Create());
			
			string? redirectPath = null;

			try
			{
				await _next(context);

				if (_options.MinimalStatusCodeToRedirect <= context.Response.StatusCode)
				{
					if (Handle(traceInfo, context, null, out redirectPath))
					{
						await InternalRedirectAsync(traceInfo, redirectPath, null, context);
					}
				}
			}
			catch (Exception ex)
				when(Handle(traceInfo, context, ex, out redirectPath))
			{
				await InternalRedirectAsync(traceInfo, redirectPath, ex, context);
			}
		}

		private bool Handle(ITraceInfo traceInfo, HttpContext context, Exception? ex, [MaybeNullWhen(false)] out string? redirectPath)
		{
			var error = _logger.LogErrorMessage(traceInfo, x => x.ExceptionInfo(ex));

			if (_options.OnErrorOccurs != null)
			{
				try
				{
					_options.OnErrorOccurs.Invoke(error, context, false);
				}
				catch (Exception onErroreEx)
				{
					_logger.LogErrorMessage(traceInfo, x => x.ExceptionInfo(onErroreEx).Detail(nameof(_options.OnErrorOccurs)));
				}
			}

			var res = context.RequestServices.GetRequiredService<IApplicationResources>();
			var clientError = context.Response.StatusCode == StatusCodes.Status404NotFound
				? res.DataNotFoundException
				: ((context.Response.StatusCode == StatusCodes.Status401Unauthorized || context.Response.StatusCode == StatusCodes.Status403Forbidden)
					? res.DataForbiddenException
					: res.GlobalExceptionMessage);

			redirectPath = _options.DefaultExceptionPath;
			_options.StatusCodeRedirectPaths?.TryGetValue(context.Response.StatusCode, out redirectPath);
			return !string.IsNullOrWhiteSpace(redirectPath);
		}

		private async Task InternalRedirectAsync(ITraceInfo traceInfo, string redirectPath, Exception? ex, HttpContext context)
		{
			int statusCode = (400 <= context.Response.StatusCode && context.Response.StatusCode < 600)
				? context.Response.StatusCode
				: StatusCodes.Status500InternalServerError;

			PathString originalPath = context.Request.Path;
			context.Request.Path = redirectPath;
			context.Request.ContentLength = null;
			context.Request.ContentType = null;
			context.Request.Method = Microsoft.AspNetCore.Http.HttpMethods.Get;
			try
			{
				context.Response.Clear();
				context.Features.Set(new Microsoft.AspNetCore.Diagnostics.ExceptionHandlerFeature()
				{
					Error = ex,
					Path = originalPath.Value,
				});
				context.Response.StatusCode = statusCode;
				context.Response.OnStarting(ClearCacheHeaders, context.Response);

				_logger
					.LogWarningMessage(traceInfo, x => x
						.LogCode(AspNetLogCode.Req_IntRedir.ToString())
						.InternalMessage("Internal redirect")
						.Detail($"{nameof(InternalRedirectAsync)}{Environment.NewLine}Original path: {originalPath.Value}{Environment.NewLine}New path: {redirectPath}"));

				await _next(context);
			}
			catch (Exception redirectEx)
			{
				var error =
					_logger.LogErrorMessage(traceInfo, x => x
						.LogCode(AspNetLogCode.Req_IntRedir.ToString())
						.ExceptionInfo(redirectEx)
						.Detail($"{nameof(InternalRedirectAsync)}{Environment.NewLine}Original path: {originalPath.Value}{Environment.NewLine}New path: {redirectPath}"));

				if (_options.OnErrorOccurs != null)
				{
					try
					{
						_options.OnErrorOccurs.Invoke(error, context, true);
					}
					catch (Exception onErroreEx)
					{
						_logger.LogErrorMessage(traceInfo, x => x.ExceptionInfo(onErroreEx).Detail(nameof(_options.OnErrorOccurs)));
					}
				}

				throw; //if the redirected path throws exception, throw it to FE
			}
			finally
			{
				context.Request.Path = originalPath;
			}
		}

		private Task ClearCacheHeaders(object state)
		{
			var response = (HttpResponse)state;
			response.Headers[HeaderNames.CacheControl] = "no-cache";
			response.Headers[HeaderNames.Pragma] = "no-cache";
			response.Headers[HeaderNames.Expires] = "-1";
			response.Headers.Remove(HeaderNames.ETag);
			return Task.CompletedTask;
		}
	}
}

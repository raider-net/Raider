using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using Raider.Logging.Extensions;
using Raider.Trace;
using System;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;

namespace Raider.AspNetCore.Middleware.Exceptions
{
	public class ExceptionHandlerMiddleware
	{
		private readonly RequestDelegate _next;
		private readonly ExceptionHandlerOptions _options;
		private readonly ILogger _logger;
		private readonly Func<object, Task> _clearCacheHeadersDelegate;

		public ExceptionHandlerMiddleware(
			RequestDelegate next,
			IOptions<ExceptionHandlerOptions> options,
			ILogger<ExceptionHandlerMiddleware> logger)
		{
			_next = next ?? throw new ArgumentNullException(nameof(next));
			_options = options?.Value ?? throw new ArgumentNullException(nameof(options));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_clearCacheHeadersDelegate = ClearCacheHeaders;
			if (_options.ExceptionHandler == null)
			{
				if (_options.DefaultExceptionPath == null)
					throw new InvalidOperationException("An error occurred when configuring the exception handler middleware. Either the 'DefaultExceptionPath' or the 'ExceptionHandler' property must be set.");

				_options.ExceptionHandler = _next;
			}
		}

		public Task Invoke(HttpContext context)
		{
			ExceptionDispatchInfo? edi = null;
			var appCtx = context.RequestServices.GetRequiredService<IApplicationContext>();
			var traceInfo = appCtx.AddTraceFrame(TraceFrame.Create());
			
			try
			{
				var task = _next(context);
				if (!task.IsCompletedSuccessfully)
				{
					return Awaited(this, traceInfo, context, task);
				}

				if (context.Response.StatusCode != StatusCodes.Status404NotFound)
					return Task.CompletedTask;
			}
			catch (Exception ex)
			{
				edi = ExceptionDispatchInfo.Capture(ex);
			}

			return HandleException(traceInfo, context, edi);

			static async Task Awaited(ExceptionHandlerMiddleware middleware, ITraceInfo traceInfo, HttpContext context, Task task)
			{
				ExceptionDispatchInfo? edi = null;
				try
				{
					await task;
				}
				catch (Exception exception)
				{
					edi = ExceptionDispatchInfo.Capture(exception);
				}

				if (edi != null || context.Response.StatusCode == StatusCodes.Status404NotFound)
					await middleware.HandleException(traceInfo, context, edi);
			}
		}

		private async Task HandleException(ITraceInfo traceInfo, HttpContext context, ExceptionDispatchInfo? edi)
		{
			var statusCode = context.Response.StatusCode;
			var ex = edi?.SourceException;
			var error = _logger.LogErrorMessage(traceInfo, x => x.ExceptionInfo(ex).Detail($"StatusCode = {statusCode}"));

			if (_options.OnErrorOccurs != null)
			{
				try
				{
					_options.OnErrorOccurs.Invoke(error, context);
				}
				catch (Exception onErroreEx)
				{
					_logger.LogErrorMessage(traceInfo, x => x.ExceptionInfo(onErroreEx).Detail(nameof(_options.OnErrorOccurs)));
				}
			}

			if (context.Response.HasStarted)
			{
				_logger.LogErrorMessage(traceInfo, x => x.InternalMessage("The response has already started, the error handler will not be executed."));
				edi?.Throw();
			}

			PathString originalPath = context.Request.Path;
			if (statusCode == StatusCodes.Status404NotFound)
			{
				if (string.IsNullOrWhiteSpace(_options.NotFoundExceptionPath))
				{
					if (!string.IsNullOrWhiteSpace(_options.DefaultExceptionPath))
						context.Request.Path = _options.DefaultExceptionPath;
				}
				else
				{
					context.Request.Path = _options.NotFoundExceptionPath;
				}
			}
			else
			{
				if (!string.IsNullOrWhiteSpace(_options.DefaultExceptionPath))
					context.Request.Path = _options.DefaultExceptionPath;
			}

			try
			{
				ClearHttpContext(context);

				var exceptionHandlerFeature = new ExceptionHandlerFeature()
				{
					Error = ex,
					Path = originalPath.Value,
				};
				context.Features.Set<IExceptionHandlerFeature>(exceptionHandlerFeature);
				context.Features.Set<IExceptionHandlerPathFeature>(exceptionHandlerFeature);
				context.Response.StatusCode = statusCode == StatusCodes.Status404NotFound ? statusCode : StatusCodes.Status500InternalServerError;
				context.Response.OnStarting(_clearCacheHeadersDelegate, context.Response);

				if (_options.ExceptionHandler != null)
					await _options.ExceptionHandler(context);

				return;
			}
			catch (Exception ex2)
			{
				_logger.LogErrorMessage(traceInfo, x => x.ExceptionInfo(ex2).Detail("An exception was thrown attempting to execute the error handler."));
			}
			finally
			{
				context.Request.Path = originalPath;
			}

			edi?.Throw(); // Re-throw the original if we couldn't handle it
		}

		private static void ClearHttpContext(HttpContext context)
		{
			context.Response.Clear();

			context.SetEndpoint(endpoint: null);
			var routeValuesFeature = context.Features.Get<IRouteValuesFeature>();
			routeValuesFeature?.RouteValues?.Clear();
		}

		private static Task ClearCacheHeaders(object state)
		{
			var headers = ((HttpResponse)state).Headers;
			headers[HeaderNames.CacheControl] = "no-cache,no-store";
			headers[HeaderNames.Pragma] = "no-cache";
			headers[HeaderNames.Expires] = "-1";
			headers.Remove(HeaderNames.ETag);
			return Task.CompletedTask;
		}
	}
}

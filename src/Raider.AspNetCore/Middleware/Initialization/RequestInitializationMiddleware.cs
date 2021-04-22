using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Raider.Extensions;
using Raider.Trace;
using System;
using System.Threading.Tasks;

namespace Raider.AspNetCore.Middleware.Initialization
{
	public class RequestInitializationMiddleware
	{
		private readonly RequestDelegate _next;
		private readonly RequestInitializationOptions _options;
		private readonly ILogger _logger;

		public RequestInitializationMiddleware(
			RequestDelegate next,
			IOptions<RequestInitializationOptions> options,
			ILogger<RequestInitializationMiddleware> logger)
		{
			_next = next ?? throw new ArgumentNullException(nameof(next));
			_options = options?.Value ?? throw new ArgumentNullException(nameof(options));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		public async Task Invoke(HttpContext context)
		{
			if (_options.UseCorrelationIdFromClient
				&& context.Request.Headers.TryGetValue(_options.Header, out StringValues externalCorrelationId))
			{
				context.TraceIdentifier = externalCorrelationId;
			}

			var appCtx = context.RequestServices.GetRequiredService<IApplicationContext>();
			appCtx.AddTraceFrame(TraceFrame.Create());

			if (_options.IncludeInResponse)
			{
				context.Response.OnStarting(() =>
				{
					context
						.Response
						.Headers
						.AddUniqueKey(_options.Header, new[] { context.TraceIdentifier });
					return Task.CompletedTask;
				});
			}

			await _next(context);
		}
	}
}

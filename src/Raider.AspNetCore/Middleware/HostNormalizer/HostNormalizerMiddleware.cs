using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Raider.Extensions;
using Raider.Logging.Extensions;
using Raider.Trace;
using System;
using System.Threading.Tasks;

namespace Raider.AspNetCore.Middleware.HostNormalizer
{
	public class HostNormalizerMiddleware
	{
		private readonly RequestDelegate _next;
		private readonly HostNormalizerOptions _options;
		private readonly ILogger _logger;

		public HostNormalizerMiddleware(
			RequestDelegate next,
			IOptions<HostNormalizerOptions> options,
			ILogger<HostNormalizerMiddleware> logger)
		{
			_next = next ?? throw new ArgumentNullException(nameof(next));
			_options = options?.Value ?? throw new ArgumentNullException(nameof(options));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		public async Task Invoke(HttpContext context)
		{
			var ac = context.RequestServices.GetRequiredService<IApplicationContext>();
			var traceInfo = ac.AddTraceFrame(TraceFrame.Create());

			try
			{
				var request = context.Request;

				string? host = null;
				string? protocol = null;

				if (string.IsNullOrWhiteSpace(_options.Host))
				{
					var forwardedHost = request.Headers["X-Forwarded-Host"];
					if (!string.IsNullOrWhiteSpace(forwardedHost))
						host = forwardedHost;
				}
				else
					host = _options.Host;

				if (string.IsNullOrWhiteSpace(_options.Protocol))
				{
					var forwardedProtocol = request.Headers["X-Forwarded-Proto"];
					if (!string.IsNullOrWhiteSpace(forwardedProtocol))
						protocol = forwardedProtocol;
				}
				else
					protocol = _options.Protocol;

				if (!string.IsNullOrWhiteSpace(host))
					request.Host = new HostString(host);

				if (!string.IsNullOrWhiteSpace(protocol))
					request.Scheme = protocol;

				if (!string.IsNullOrWhiteSpace(_options.VirtualPath))
					request.PathBase = $"/{_options.VirtualPath.TrimPrefix("/")}";
			}
			catch (Exception ex)
			{
				_logger.LogErrorMessage(traceInfo, x => x.ExceptionInfo(ex));
			}

			await _next(context);
		}
	}
}

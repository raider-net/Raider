using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Raider.AspNetCore.Logging;
using Raider.AspNetCore.Logging.Dto;
using Raider.Diagnostics;
using Raider.Extensions;
using Raider.Logging.Extensions;
using Raider.Trace;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Raider.AspNetCore.Middleware.Tracking
{
	public class RequestTrackingMiddleware
	{
		private readonly RequestDelegate _next;
		private readonly RequestTrackingOptions _options;
		private readonly ILogger _logger;

		public RequestTrackingMiddleware(
			RequestDelegate next,
			IOptions<RequestTrackingOptions> options,
			ILogger<RequestTrackingMiddleware> logger)
		{
			_next = next ?? throw new ArgumentNullException(nameof(next));
			_options = options?.Value ?? throw new ArgumentNullException(nameof(options));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		public async Task Invoke(HttpContext context)
		{
			var isWebSocketRequest = context.WebSockets.IsWebSocketRequest;

			long? startTicks = null;

			if (!isWebSocketRequest)
			{
				startTicks = StaticWatch.CurrentTicks;
			}

			var tc = context.RequestServices.GetRequiredService<TraceContext>();
			var traceInfo = tc.AddTraceFrame(nameof(RequestTrackingMiddleware), TraceFrame.Create());

			if (_options.LogRequest)
			{
				try
				{
					bool canLog = _options.CanLogRequest(context.Request, out bool bodyAsString);

					var request =
						await Request.Create(
							context.Request,
							traceInfo.CorrelationId ?? Guid.Empty,
							traceInfo.ExternalCorrelationId,
							_options.LogRequestHeaders,
							canLog && _options.LogRequestForm,
							canLog && _options.LogRequestBody && bodyAsString,
							canLog && _options.LogRequestBody && !bodyAsString,
							canLog && _options.LogRequestFiles);

					AspNetLogWriter.Instance.WriteRequest(request);
				}
				catch (Exception ex)
				{
					_logger.LogErrorMessage(traceInfo, x => x.ExceptionInfo(ex));
				}
			}

			if (_options.LogResponse)
			{
				if (_options.LogResponseBody)
				{
					var originalResponseBody = context.Response.Body;
					using (var responseBodyStream = new MemoryStream())
					{
						int statusCode = 500;
						string? body = null;
						byte[]? bodyByteArray = null;
						string? error = null;

						try
						{
							context.Response.Body = responseBodyStream;
							await _next(context);
							statusCode = context.Response.StatusCode;
						}
						catch (Exception ex)
						{
							error = ex.ToStringTrace();
							_logger.LogErrorMessage(traceInfo, x => x.ExceptionInfo(ex));
							throw;
						}
						finally
						{
							decimal? elapsedMilliseconds = null;

							if (startTicks.HasValue)
							{
								long endTicks = StaticWatch.CurrentTicks;
								elapsedMilliseconds = StaticWatch.ElapsedMilliseconds(startTicks.Value, endTicks);
							}

							try
							{
								responseBodyStream.Seek(0, SeekOrigin.Begin);

								bool canLog = _options.CanLogResponse(context.Request.Path, context.Response, out bool bodyAsString);

								if (canLog && _options.LogResponseBody)
								{
									if (bodyAsString)
										body = new StreamReader(responseBodyStream).ReadToEnd();
									else
										bodyByteArray = responseBodyStream.ToArray();

									responseBodyStream.Seek(0, SeekOrigin.Begin);
								}

								await responseBodyStream.CopyToAsync(originalResponseBody);
							}
							catch (Exception ex)
							{
								if (string.IsNullOrWhiteSpace(error))
								{
									error = ex.ToStringTrace();
								}
								else
								{
									var sb = new StringBuilder();
									sb.AppendLine(error);
									sb.AppendLine();
									sb.AppendLine("--- STREAM READ ERROR ---");
									sb.AppendLine();
									sb.AppendLine(ex.ToStringTrace());
									error = sb.ToString();
								}

								_logger.LogErrorMessage(traceInfo, x => x.ExceptionInfo(ex));
							}

							context.Response.Body = originalResponseBody;

							var response =
								Response.Create(
									context.Response,
									traceInfo.CorrelationId ?? Guid.Empty,
									traceInfo.ExternalCorrelationId,
									statusCode,
									body,
									bodyByteArray,
									error,
									elapsedMilliseconds,
									_options.LogResponseHeaders);

							AspNetLogWriter.Instance.WriteResponse(response);
						}
					}
				}
				else //_options.LogResponseBody == false
				{
					try
					{
						await _next(context);
						LogResponse(context, context.Response.StatusCode, traceInfo, startTicks, null);
					}
					catch (Exception ex)
						when (LogResponse(context, 500, traceInfo, startTicks, ex.ToStringTrace()))
					{ } //never catch or rethrow ... LogResponse always returns FALSE.
				}
			}
			else //_options.LogResponse == false
			{
				await _next(context);
			}
		}

		private bool LogResponse(HttpContext context, int statusCode, ITraceInfo traceInfo, long? startTicks, string? error)
		{
			decimal? elapsedMilliseconds = null;

			if (startTicks.HasValue)
			{
				long endTicks = StaticWatch.CurrentTicks;
				elapsedMilliseconds = StaticWatch.ElapsedMilliseconds(startTicks.Value, endTicks);
			}

			var response =
				Response.Create(
					context.Response,
					traceInfo.CorrelationId ?? Guid.Empty,
					traceInfo.ExternalCorrelationId,
					statusCode,
					null,
					null,
					error,
					elapsedMilliseconds,
					_options.LogResponseHeaders);

			AspNetLogWriter.Instance.WriteResponse(response);

			return false;
		}
	}
}

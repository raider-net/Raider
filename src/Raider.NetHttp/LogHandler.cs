using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Raider.Logging.Extensions;
using Raider.NetHttp.Http;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Raider.NetHttp
{
	/// <inheritdoc />
	internal class LogHandler<TOptions> : DelegatingHandler
		where TOptions : HttpApiClientOptions
	{
		private readonly TOptions _options;
		private readonly ILogger _errorLogger;

		public LogHandler(IOptions<TOptions> options, ILogger<LogHandler<TOptions>> errorLogger)
		{
			_options = options?.Value ?? throw new ArgumentNullException(nameof(options));
			_errorLogger = errorLogger ?? throw new ArgumentNullException(nameof(errorLogger));
		}

		protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			var uri = request.RequestUri?.ToString();
			var correlationId = Guid.NewGuid();
			Stopwatch? sw = null;

			var logger = GetLogger(uri);
			if (logger != null)
			{
				if (logger.OnBeforeRequestSendAsStringAsync != null)
				{
					var requestDto = await RequestDtoMapper.Map(request, null, null, null, true, false, false, cancellationToken).ConfigureAwait(false);
					string? body = null;
					if (request.Content != null)
						body = await request.Content.ReadAsStringAsync(
#if NET5_0
						cancellationToken
#endif
						).ConfigureAwait(false);

					try
					{
						await logger.OnBeforeRequestSendAsStringAsync.Invoke(requestDto, body, correlationId, cancellationToken).ConfigureAwait(false);
					}
					catch (Exception ex)
					{
						_errorLogger.LogErrorMessage(x => x.ExceptionInfo(ex).Detail($"{nameof(LogHandler<TOptions>)}.{nameof(SendAsync)} - {nameof(logger.OnBeforeRequestSendAsStringAsync)}"));
					}
				}

				if (logger.OnBeforeRequestSendAsByteArrayAsync != null)
				{
					var requestDto = await RequestDtoMapper.Map(request, null, null, null, true, false, false, cancellationToken).ConfigureAwait(false);
					byte[]? body = null;
					if (request.Content != null)
						body = await request.Content.ReadAsByteArrayAsync(
#if NET5_0
						cancellationToken
#endif
						).ConfigureAwait(false);

					try
					{
						await logger.OnBeforeRequestSendAsByteArrayAsync.Invoke(requestDto, body, correlationId, cancellationToken).ConfigureAwait(false);
					}
					catch (Exception ex)
					{
						_errorLogger.LogErrorMessage(x => x.ExceptionInfo(ex).Detail($"{nameof(LogHandler<TOptions>)}.{nameof(SendAsync)} - {nameof(logger.OnBeforeRequestSendAsByteArrayAsync)}"));
					}
				}

				if (logger.OnBeforeRequestSendAsStreamAsync != null)
				{
					var requestDto = await RequestDtoMapper.Map(request, null, null, null, true, false, false, cancellationToken).ConfigureAwait(false);
					Stream? body = null;
					if (request.Content != null)
						body = await request.Content.ReadAsStreamAsync(
#if NET5_0
						cancellationToken
#endif
						).ConfigureAwait(false);

					try
					{
						await logger.OnBeforeRequestSendAsStreamAsync.Invoke(requestDto, body, correlationId, cancellationToken).ConfigureAwait(false);
					}
					catch (Exception ex)
					{
						_errorLogger.LogErrorMessage(x => x.ExceptionInfo(ex).Detail($"{nameof(LogHandler<TOptions>)}.{nameof(SendAsync)} - {nameof(logger.OnBeforeRequestSendAsStreamAsync)}"));
					}
				}

				sw = Stopwatch.StartNew();
			}

			var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);

			if (logger != null)
			{
				sw?.Stop();

				if (logger.OnAfterResponseReceivedAsStringAsync != null)
				{
					var responseDto = await ResponseDtoMapper.Map(response, null, null, sw?.ElapsedMilliseconds, true, false, false, cancellationToken).ConfigureAwait(false);
					string? body = null;
					if (response.Content != null)
						body = await response.Content.ReadAsStringAsync(
#if NET5_0
						cancellationToken
#endif
						).ConfigureAwait(false);

					try
					{
						await logger.OnAfterResponseReceivedAsStringAsync.Invoke(responseDto, body, correlationId, cancellationToken).ConfigureAwait(false);
					}
					catch (Exception ex)
					{
						_errorLogger.LogErrorMessage(x => x.ExceptionInfo(ex).Detail($"{nameof(LogHandler<TOptions>)}.{nameof(SendAsync)} - {nameof(logger.OnAfterResponseReceivedAsStringAsync)}"));
					}
				}

				if (logger.OnAfterResponseReceivedAsByteArrayAsync != null)
				{
					var responseDto = await ResponseDtoMapper.Map(response, null, null, sw?.ElapsedMilliseconds, true, false, false, cancellationToken).ConfigureAwait(false);
					byte[]? body = null;
					if (response.Content != null)
						body = await response.Content.ReadAsByteArrayAsync(
#if NET5_0
						cancellationToken
#endif
						).ConfigureAwait(false);

					try
					{
						await logger.OnAfterResponseReceivedAsByteArrayAsync.Invoke(responseDto, body, correlationId, cancellationToken).ConfigureAwait(false);
					}
					catch (Exception ex)
					{
						_errorLogger.LogErrorMessage(x => x.ExceptionInfo(ex).Detail($"{nameof(LogHandler<TOptions>)}.{nameof(SendAsync)} - {nameof(logger.OnAfterResponseReceivedAsByteArrayAsync)}"));
					}
				}

				if (logger.OnAfterResponseReceivedAsStreamAsync != null)
				{
					var responseDto = await ResponseDtoMapper.Map(response, null, null, sw?.ElapsedMilliseconds, true, false, false, cancellationToken).ConfigureAwait(false);
					Stream? body = null;
					if (response.Content != null)
						body = await response.Content.ReadAsStreamAsync(
#if NET5_0
						cancellationToken
#endif
						).ConfigureAwait(false);

					try
					{
						await logger.OnAfterResponseReceivedAsStreamAsync.Invoke(responseDto, body, correlationId, cancellationToken).ConfigureAwait(false);
					}
					catch (Exception ex)
					{
						_errorLogger.LogErrorMessage(x => x.ExceptionInfo(ex).Detail($"{nameof(LogHandler<TOptions>)}.{nameof(SendAsync)} - {nameof(logger.OnAfterResponseReceivedAsStreamAsync)}"));
					}
				}
			}

			return response;
		}

		private IRequestResponseLogger? GetLogger(string? uri)
		{
			if (string.IsNullOrWhiteSpace(uri))
				return null;

			if (_options.LogDisabledUris != null && _options.LogDisabledUris.Any(x => uri.StartsWith(x)))
				return null;

			if (_options.UriLoggers == null || _options.UriLoggers.Count == 0)
				return null;

			var key = _options.UriLoggers.Keys.FirstOrDefault(x => uri.StartsWith(x));
			if (!string.IsNullOrWhiteSpace(key) && _options.UriLoggers.TryGetValue(key, out var logger))
					return logger;

			if (_options.UriLoggers.TryGetValue("*", out var defaultLogger))
				return defaultLogger;

			return null;
		}
	}
}

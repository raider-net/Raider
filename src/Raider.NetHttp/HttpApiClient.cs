﻿using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Raider.Extensions;
using Raider.Logging;
using Raider.Logging.Extensions;
using Raider.NetHttp.Http;
using Raider.Trace;
using System;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Raider.NetHttp
{
	public abstract class HttpApiClient
	{
		private readonly HttpClient _client;

		protected HttpApiClientOptions Options { get; }
		protected ILogger Logger { get; }

		public HttpApiClient(HttpClient client, IOptions<HttpApiClientOptions> options, ILogger<HttpApiClient> logger)
		{
			_client = client ?? throw new ArgumentNullException(nameof(client));
			Options = options?.Value ?? throw new ArgumentNullException(nameof(options));
			Logger = logger ?? throw new ArgumentNullException(nameof(logger));

			var error = Options.Validate()?.ToString();
			if (!string.IsNullOrWhiteSpace(error))
				throw new InvalidOperationException(error);
		}

		public Task<IHttpApiClientResponse> SendAsync(Action<RequestBuilder> configureRequest, CancellationToken cancellationToken = default)
		{
			var builder = new RequestBuilder();
			configureRequest.Invoke(builder);

			return SendAsync(builder.Build(), false, cancellationToken);
		}

		public Task<IHttpApiClientResponse> SendAsync(Action<RequestBuilder> configureRequest, bool? continueOnCapturedContext, CancellationToken cancellationToken = default)
		{
			var builder = new RequestBuilder();
			configureRequest.Invoke(builder);

			return SendAsync(builder.Build(), continueOnCapturedContext, cancellationToken);
		}

		public Task<IHttpApiClientResponse> SendAsync(IHttpApiClientRequest request, CancellationToken cancellationToken = default)
		{
			return SendAsync(request, false, cancellationToken);
		}

		public async Task<IHttpApiClientResponse> SendAsync(IHttpApiClientRequest request, bool? continueOnCapturedContext, CancellationToken cancellationToken = default)
		{
			if (request == null)
				throw new ArgumentNullException(nameof(request));

			if (string.IsNullOrWhiteSpace(request.BaseAddress))
				request.BaseAddress = Options.BaseAddress;

			var response = new Raider.NetHttp.Http.Internal.HttpApiClientResponse(request);

			CancellationTokenSource? requestTimeoutCancellationTokenSource = null;
			CancellationTokenSource? linkedCancellationTokenSource = null;
			var usedCancellationToken = cancellationToken;
			try
			{
				if (request.RequestTimeout.HasValue)
				{
					requestTimeoutCancellationTokenSource = new CancellationTokenSource(request.RequestTimeout.Value);
					if (cancellationToken != default)
					{
						linkedCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(requestTimeoutCancellationTokenSource.Token, cancellationToken);
						usedCancellationToken = linkedCancellationTokenSource.Token;
					}
					else
					{
						usedCancellationToken = requestTimeoutCancellationTokenSource.Token;
					}
				}

				using var httpRequestMessage = request.ToHttpRequestMessage();

				if (continueOnCapturedContext.HasValue)
				{
					var httpResponseMessageTask =
						_client
							.SendAsync(httpRequestMessage, System.Net.Http.HttpCompletionOption.ResponseHeadersRead, usedCancellationToken)
							.ConfigureAwait(continueOnCapturedContext: continueOnCapturedContext.Value);

					response.HttpResponseMessage = await httpResponseMessageTask;
				}
				else
				{
					var httpResponseMessageTask =
						_client
							.SendAsync(httpRequestMessage, System.Net.Http.HttpCompletionOption.ResponseHeadersRead, usedCancellationToken);

					response.HttpResponseMessage = await httpResponseMessageTask;
				}
			}
			catch (TaskCanceledException)
			{
				if (requestTimeoutCancellationTokenSource != null && requestTimeoutCancellationTokenSource.IsCancellationRequested)
					response.RequestTimedOut = true;
				else
					response.OperationCanceled = true;
			}
			catch (TimeoutException)
			{
				response.RequestTimedOut = true;
			}
			catch (OperationCanceledException)
			{
				response.OperationCanceled = true;
			}
			catch (Exception ex)
			{
				response.Exception = ex;
			}
			finally
			{
				linkedCancellationTokenSource?.Dispose();
				requestTimeoutCancellationTokenSource?.Dispose();
			}

			return response;
		}

		protected virtual ErrorMessageBuilder? CreateErrorMessage(
			IHttpApiClientRequest? request,
			IHttpApiClientResponse? response,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
		{
			if (request == null && response == null)
				return null;

			var builder = new ErrorMessageBuilder(TraceInfo.Create(null, null, memberName, sourceFilePath, sourceLineNumber));
			var sb = new StringBuilder();

			if (request != null)
				sb.AppendLine($"URI = {request.GetRequestUri()}");

			if (response != null)
			{
				if (request == null && response.Request != null)
					sb.AppendLine($"URI = {response.Request.GetRequestUri()}");

				sb.AppendLine($"{nameof(response.StatusCode)} = {response.StatusCode}");

				if (response.OperationCanceled.HasValue)
					sb.AppendLine($"{nameof(response.OperationCanceled)} = {response.OperationCanceled}");

				if (response.RequestTimedOut.HasValue)
					sb.AppendLine($"{nameof(response.RequestTimedOut)} = {response.RequestTimedOut}");

				if (response.Exception != null)
					builder.ExceptionInfo(response.Exception);
			}

			builder.Detail(sb.ToString());

			return builder;
		}

		protected virtual ErrorMessageBuilder? LogError(
			IHttpApiClientRequest? request,
			IHttpApiClientResponse? response,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
		{
			if (request == null && response == null)
				return null;

			var errorMessageBuilder = CreateErrorMessage(request, response, memberName, sourceFilePath, sourceLineNumber);

			if (errorMessageBuilder == null)
				return null;

			Logger.LogErrorMessage(errorMessageBuilder.Build());

			return errorMessageBuilder;
		}

		protected virtual StringBuilder LogErrorToStringBuilder(
			IHttpApiClientRequest? request,
			IHttpApiClientResponse? response,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
		{
			var sb = new StringBuilder();

			if (request != null)
				sb.AppendLine($"URI = {request.GetRequestUri()}");

			if (response != null)
			{
				if (request == null && response.Request != null)
					sb.AppendLine($"URI = {response.Request.GetRequestUri()}");

				sb.AppendLine($"{nameof(response.StatusCode)} = {response.StatusCode}");

				if (response.OperationCanceled.HasValue)
					sb.AppendLine($"{nameof(response.OperationCanceled)} = {response.OperationCanceled}");

				if (response.RequestTimedOut.HasValue)
					sb.AppendLine($"{nameof(response.RequestTimedOut)} = {response.RequestTimedOut}");

				if (response.Exception != null)
					sb.AppendLine($"Exception: {response.Exception.ToStringTrace()}");
			}
			else
			{
				sb.AppendLine("NO RESPONSE");
			}

			return sb;
		}
	}
}

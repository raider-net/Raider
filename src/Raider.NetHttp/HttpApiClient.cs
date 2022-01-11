using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Raider.NetHttp.Http;
using System;
using System.Net.Http;
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

			return SendAsync(builder, false, cancellationToken);
		}

		public Task<IHttpApiClientResponse> SendAsync(Action<RequestBuilder> configureRequest, bool? continueOnCapturedContext, CancellationToken cancellationToken = default)
		{
			var builder = new RequestBuilder();
			configureRequest.Invoke(builder);

			return SendAsync(builder, continueOnCapturedContext, cancellationToken);
		}

		public Task<IHttpApiClientResponse> SendAsync(IHttpApiClientRequest request, CancellationToken cancellationToken = default)
		{
			var builder = new RequestBuilder(request);
			return SendAsync(builder, false, cancellationToken);
		}

		public Task<IHttpApiClientResponse> SendAsync(IHttpApiClientRequest request, bool? continueOnCapturedContext, CancellationToken cancellationToken = default)
		{
			var builder = new RequestBuilder(request);
			return SendAsync(builder, continueOnCapturedContext, cancellationToken);
		}

		public async Task<IHttpApiClientResponse> SendAsync(RequestBuilder builder, bool? continueOnCapturedContext, CancellationToken cancellationToken = default)
		{
			if (builder == null)
				throw new ArgumentNullException(nameof(builder));
			
			builder.BaseAddress(Options.BaseAddress, false);

			var request = builder.Build();
			var response = new Raider.NetHttp.Http.Internal.HttpApiClientResponse(request);

			try
			{
				using var httpRequestMessage = request.ToHttpRequestMessage();

				if (continueOnCapturedContext.HasValue)
				{
					var httpResponseMessageTask =
						_client
							.SendAsync(httpRequestMessage, System.Net.Http.HttpCompletionOption.ResponseHeadersRead, cancellationToken)
							.ConfigureAwait(continueOnCapturedContext: continueOnCapturedContext.Value);

					response.Response = await httpResponseMessageTask;
				}
				else
				{
					var httpResponseMessageTask =
						_client
							.SendAsync(httpRequestMessage, System.Net.Http.HttpCompletionOption.ResponseHeadersRead, cancellationToken);

					response.Response = await httpResponseMessageTask;
				}
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

			return response;
		}
	}
}

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Raider.Extensions;
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
		private readonly HttpApiClientOptions _options;
		protected abstract ILogger<HttpApiClient> Logger { get; }

		public HttpApiClient(HttpClient client, IOptions<HttpApiClientOptions> options)
		{
			_client = client ?? throw new ArgumentNullException(nameof(client));
			_options = options?.Value ?? throw new ArgumentNullException(nameof(options));
		}

		protected async Task<T> SendAsync<T>(HttpApiClientRequest request, bool? continueOnCapturedContext, CancellationToken cancellationToken = default)
			where T : HttpApiClientResponse, new()
		{
			if (request == null)
				throw new ArgumentNullException(nameof(request));

			var response = new T
			{
				Request = request
			};

			try
			{
				if (string.IsNullOrWhiteSpace(request.BaseAddress))
					request.SetBaseAddress(_options.BaseAddress);

				using var httpRequestMessage = request.ToHttpRequestMessage();

				if (continueOnCapturedContext.HasValue)
				{
					var httpResponseMessageTask =
						_client
							.SendAsync(httpRequestMessage, System.Net.Http.HttpCompletionOption.ResponseHeadersRead, cancellationToken)
							.ConfigureAwait(continueOnCapturedContext: continueOnCapturedContext.Value);

					using (var httpResponseMessage = await httpResponseMessageTask)
					{
						await response.FromHttpResponseMessageAsync(httpResponseMessage, cancellationToken);
					}
				}
				else
				{
					var httpResponseMessageTask =
						_client
							.SendAsync(httpRequestMessage, System.Net.Http.HttpCompletionOption.ResponseHeadersRead, cancellationToken);

					using (var httpResponseMessage = await httpResponseMessageTask)
					{
						await response.FromHttpResponseMessageAsync(httpResponseMessage, cancellationToken);
					}
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
				response.Exception = ex.ToStringTrace();
			}

			try
			{
				if (!response.IsOK)
				{
					var exception = response.GetException();
					Logger.LogError(exception); //TODO: Log ErrorMessage instead
				}
			}
			catch { }

			return response;
		}




		public async Task<HttpApiClientResponse> SendAsync(HttpApiClientRequest request, CancellationToken cancellationToken = default)
		{
			try
			{
				var response = await SendAsync<HttpApiClientResponse>(request, null, cancellationToken);
				return response;
			}
			catch (Exception ex)
			{
				Logger.LogError(ex, "Error"); //TODO: Log ErrorMessage instead
			}

			return new HttpApiClientResponse { Request = request };
		}


		public async Task<HttpApiClientResponse<T>> SendAsync<T>(HttpApiClientRequest request, CancellationToken cancellationToken = default)
		{
			try
			{
				var response = await SendAsync<HttpApiClientResponse<T>>(request, null, cancellationToken);
				return response;
			}
			catch (Exception ex)
			{
				Logger.LogError(ex, "Error"); //TODO: Log ErrorMessage instead
			}

			return new HttpApiClientResponse<T> { Request = request };
		}
	}
}

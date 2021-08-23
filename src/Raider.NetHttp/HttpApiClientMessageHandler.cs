using Raider.Policy;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Raider.NetHttp
{
	/// <inheritdoc />
	internal class HttpApiClientMessageHandler : DelegatingHandler
	{
		private readonly IAsyncPolicy<HttpResponseMessage>? _policy;

		public HttpApiClientMessageHandler() { }

		public HttpApiClientMessageHandler(IAsyncPolicy<HttpResponseMessage> policy)
		{
			_policy = policy;
		}

		protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			HttpResponseMessage response;
			
			if (_policy == null)
			{
				response = await base.SendAsync(request, cancellationToken);
			}
			else
			{
				response = await _policy.ExecuteAsync(ct => SendInternalAsync(request, ct), cancellationToken);
			}

			return response;
		}

		protected virtual Task<HttpResponseMessage> SendInternalAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			if (request == null)
				throw new ArgumentNullException(nameof(request));

			return base.SendAsync(request, cancellationToken);
		}
	}
}

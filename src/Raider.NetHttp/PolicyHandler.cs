﻿using Microsoft.Extensions.Options;
using Raider.Policy;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Raider.NetHttp
{
	/// <inheritdoc />
	internal class PolicyHandler : DelegatingHandler
	{
		private readonly HttpApiClientOptions _options;

		public PolicyHandler(IOptions<HttpApiClientOptions> options)
		{
			_options = options?.Value ?? throw new ArgumentNullException(nameof(options));
		}

		protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			HttpResponseMessage response;

			var uri = request.RequestUri?.ToString();
			var policy = GetPolicy(uri);
			if (policy == null)
			{
				response = await base.SendAsync(request, cancellationToken);
			}
			else
			{
				response = await policy.ExecuteAsync(ct => SendInternalAsync(request, ct), cancellationToken);
			}

			return response;
		}

		protected virtual Task<HttpResponseMessage> SendInternalAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			if (request == null)
				throw new ArgumentNullException(nameof(request));

			return base.SendAsync(request, cancellationToken);
		}

		private IAsyncPolicy<HttpResponseMessage>? GetPolicy(string? uri)
		{
			if (string.IsNullOrWhiteSpace(uri))
				return null;

			if (_options.UriPolicies == null || _options.UriPolicies.Count == 0)
				return null;

			var key = _options.UriPolicies.Keys.FirstOrDefault(x => uri.StartsWith(x));
			if (!string.IsNullOrWhiteSpace(key) && _options.UriPolicies.TryGetValue(key, out var policy))
				return policy;

			if (_options.UriPolicies.TryGetValue("*", out var defaultPolicy))
				return defaultPolicy;

			return null;
		}
	}
}
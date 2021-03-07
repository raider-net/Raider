using Microsoft.AspNetCore.Http;
using System;

namespace Raider.Extensions
{
	public static class HttpRequestExtensions
	{
		public static Uri GetUri(this HttpRequest request)
		{
			if (request == null)
				return null;

			//string absoluteUri = $"{request.Scheme}://{request.Host}{request.Path}{request.QueryString}";
			//Uri uri = new Uri(absoluteUri);
			//return uri;

			var hostComponents = request.Host.ToUriComponent().Split(':');

			UriBuilder uriBuilder = new UriBuilder
			{
				Scheme = request.Scheme,
				Host = hostComponents[0],
				Path = string.Concat(request.PathBase.ToUriComponent(), request.Path.ToUriComponent()), //Request.PathBase reprezentuje VIRTUAL PATH = VirtualPath
				Query = request.QueryString.ToUriComponent()
			};

			if (hostComponents.Length == 2)
			{
				uriBuilder.Port = Convert.ToInt32(hostComponents[1]);
			}

			return uriBuilder.Uri;
		}
	}
}

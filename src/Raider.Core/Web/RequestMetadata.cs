using System;
using System.Collections.Generic;

namespace Raider.Web
{
	public class RequestMetadata : IRequestMetadata
	{
		public List<KeyValuePair<string, List<string>>>? Query { get; set; }
		public string? ContentType { get; set; }
		public long? ContentLength { get; set; }
		public List<KeyValuePair<string, string>>? Cookies { get; set; }
		public Dictionary<string, List<string>>? Headers { get; set; }
		public string? Protocol { get; set; }
		public Dictionary<string, object?>? RouteValues { get; set; }
		public string? Path { get; set; }
		public string? PathBase { get; set; }
		public string? Host { get; set; }
		public string? RemoteIp { get; set; }
		public int? Port { get; set; }
		public Uri? Uri { get; set; }
		public string? Scheme { get; set; }
		public string? Method { get; set; }
		public List<KeyValuePair<string, List<string>>>? Form { get; set; }
		public int? FilesCount { get; set; }
		public Dictionary<string, Func<string, string>>? CookieUnprotectors { get; set; } //Dictionary<cookieName, Unprotector>

		IReadOnlyList<KeyValuePair<string, List<string>>>? IRequestMetadata.Query => Query;
		IReadOnlyList<KeyValuePair<string, string>>? IRequestMetadata.Cookies => Cookies;
		IReadOnlyDictionary<string, List<string>>? IRequestMetadata.Headers => Headers;
		IReadOnlyDictionary<string, object?>? IRequestMetadata.RouteValues => RouteValues;
		IReadOnlyList<KeyValuePair<string, List<string>>>? IRequestMetadata.Form => Form;
		IReadOnlyDictionary<string, Func<string, string>>? IRequestMetadata.CookieUnprotectors => CookieUnprotectors;

		public string UnprotectCookie(string key, string value)
		{
			if (CookieUnprotectors == null || string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(value))
				return value;

			if (CookieUnprotectors.TryGetValue(key, out Func<string, string>? cookieUnprotector))
			{
				if (cookieUnprotector == null)
					return value;

				return cookieUnprotector.Invoke(value);
			}

			return value;
		}
	}
}

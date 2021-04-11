using System;
using System.Collections.Generic;

namespace Raider.Web
{
	public class RequestMetadata
	{
		public List<KeyValuePair<string, List<string>>>? Query { get; set; }
		public string? ContentType { get; set; }
		public long? ContentLength { get; set; }
		public List<KeyValuePair<string, string>>? Cookies { get; set; }
		public IDictionary<string, List<string>>? Headers { get; set; }
		public string? Protocol { get; set; }
		public IDictionary<string, object?>? RouteValues { get; set; }
		public string? Path { get; set; }
		public string? PathBase { get; set; }
		public string? Host { get; set; }
		public int? Port { get; set; }
		public Uri? Uri { get; set; }
		public string? Scheme { get; set; }
		public string? Method { get; set; }
		public List<KeyValuePair<string, List<string>>>? Form { get; set; }
		public int? FilesCount { get; set; }
		public Dictionary<string, Func<string, string>>? CookieUnprotectors { get; set; } //Dictionary<cookieName, Unprotector>

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

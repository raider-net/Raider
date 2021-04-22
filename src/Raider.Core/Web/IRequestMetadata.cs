using System;
using System.Collections.Generic;

namespace Raider.Web
{
	public interface IRequestMetadata
	{
		IReadOnlyList<KeyValuePair<string, List<string>>>? Query { get; }
		string? ContentType { get; }
		long? ContentLength { get; }
		IReadOnlyList<KeyValuePair<string, string>>? Cookies { get; }
		IReadOnlyDictionary<string, List<string>>? Headers { get; }
		string? Protocol { get; }
		IReadOnlyDictionary<string, object?>? RouteValues { get; }
		string? Path { get; }
		string? PathBase { get; }
		string? Host { get; }
		string? RemoteIp { get; }
		int? Port { get; }
		Uri? Uri { get; }
		string? Scheme { get; }
		string? Method { get; }
		IReadOnlyList<KeyValuePair<string, List<string>>>? Form { get; }
		int? FilesCount { get; }
		IReadOnlyDictionary<string, Func<string, string>>? CookieUnprotectors { get; } //Dictionary<cookieName, Unprotector>

		string UnprotectCookie(string key, string value)
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

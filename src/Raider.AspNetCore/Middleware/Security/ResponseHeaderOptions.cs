using System;
using Raider.Extensions;
using System.Collections.Generic;
using static Raider.AspNetCore.Middleware.Security.ResponseHeaderOptions;
using Microsoft.AspNetCore.Http;

namespace Raider.AspNetCore.Middleware.Security
{
	public interface IResponseHeaderOptions
	{
		bool Remove { get; }
		string Key { get; }
		string? Value { get; }
		Protocol ApplyToProtocol { get; }
		IReadOnlyList<string>? ApplyOnlyToContentTypes { get; }
	}

	public class ResponseHeaderOptions : IResponseHeaderOptions
	{
		public enum Protocol
		{
			http = 1,
			https = 2,
			httpAndHttps = http & https
		}

		public bool Remove { get; }
		public string Key { get; set; }
		public string? Value { get; set; }
		public Protocol ApplyToProtocol { get; set; }
		public List<string>? ApplyOnlyToContentTypes { get; set; }
		IReadOnlyList<string>? IResponseHeaderOptions.ApplyOnlyToContentTypes => ApplyOnlyToContentTypes;

		public ResponseHeaderOptions(string key, string? value, Protocol applyToProtocol = Protocol.httpAndHttps)
		{
			Key = string.IsNullOrWhiteSpace(key)
				? throw new ArgumentNullException(nameof(key))
				: key;
			Value = value;
			ApplyToProtocol = applyToProtocol;
			Remove = string.IsNullOrWhiteSpace(value);
		}

		public ResponseHeaderOptions ApplyToContentType(string contentType)
		{
			if (string.IsNullOrWhiteSpace(contentType))
				throw new ArgumentNullException(nameof(contentType));

			if (ApplyOnlyToContentTypes == null)
				ApplyOnlyToContentTypes = new List<string>();

			ApplyOnlyToContentTypes.AddUniqueItem(contentType);
			return this;
		}

		internal static void Apply(HttpContext context, IResponseHeaderOptions options)
		{
			var request = context.Request;

			var headers = context.Response.Headers;

			if (request.IsHttps)
			{
				if (options.ApplyToProtocol == Protocol.https || options.ApplyToProtocol == Protocol.httpAndHttps)
				{
					if (options.Remove)
					{
						if (options.ApplyOnlyToContentTypes == null || options.ApplyOnlyToContentTypes.Count == 0)
						{
							headers.Remove(options.Key);
						}
						else
						{
							if (string.IsNullOrWhiteSpace(context.Response.ContentType))
							{
								headers.Remove(options.Key);
							}
							else
							{
								foreach (var contentType in options.ApplyOnlyToContentTypes)
								{
									if (context.Response.ContentType.StartsWith(contentType))
									{
										headers.Remove(options.Key);
										break;
									}
								}
							}
						}
					}
					else
					{
						if (options.ApplyOnlyToContentTypes == null || options.ApplyOnlyToContentTypes.Count == 0)
						{
							headers[options.Key] = options.Value;
						}
						else
						{
							if (string.IsNullOrWhiteSpace(context.Response.ContentType))
							{
								headers[options.Key] = options.Value;
							}
							else
							{
								foreach (var contentType in options.ApplyOnlyToContentTypes)
								{
									if (context.Response.ContentType.StartsWith(contentType))
									{
										headers[options.Key] = options.Value;
										break;
									}
								}
							}
						}
					}
				}
			}
			else
			{
				if (options.ApplyToProtocol == Protocol.http || options.ApplyToProtocol == Protocol.httpAndHttps)
				{
					if (options.Remove)
					{
						if (options.ApplyOnlyToContentTypes == null || options.ApplyOnlyToContentTypes.Count == 0)
						{
							headers.Remove(options.Key);
						}
						else
						{
							if (string.IsNullOrWhiteSpace(context.Response.ContentType))
							{
								headers.Remove(options.Key);
							}
							else
							{
								foreach (var contentType in options.ApplyOnlyToContentTypes)
								{
									if (context.Response.ContentType.StartsWith(contentType))
									{
										headers.Remove(options.Key);
										break;
									}
								}
							}
						}
					}
					else
					{
						if (options.ApplyOnlyToContentTypes == null || options.ApplyOnlyToContentTypes.Count == 0)
						{
							headers[options.Key] = options.Value;
						}
						else
						{
							if (string.IsNullOrWhiteSpace(context.Response.ContentType))
							{
								headers[options.Key] = options.Value;
							}
							else
							{
								foreach (var contentType in options.ApplyOnlyToContentTypes)
								{
									if (context.Response.ContentType.StartsWith(contentType))
									{
										headers[options.Key] = options.Value;
										break;
									}
								}
							}
						}
					}
				}
			}
		}

		public static IResponseHeaderOptions RemoveSerever { get; } =
			new ResponseHeaderOptions("Server", null, Protocol.httpAndHttps);

		public static IResponseHeaderOptions ReferrerPolicy { get; } =
			new ResponseHeaderOptions("referrer-policy", "strict-origin-when-cross-origin", Protocol.httpAndHttps)
			.ApplyToContentType("text/html");

		public static IResponseHeaderOptions XContentTypeOptions { get; } =
			new ResponseHeaderOptions("x-content-type-options", "nosniff", Protocol.httpAndHttps);

		public static IResponseHeaderOptions XFrameOptions { get; } =
			new ResponseHeaderOptions("x-frame-options", "DENY", Protocol.httpAndHttps)
				.ApplyToContentType("text/html");

		public static IResponseHeaderOptions XPermittedCrossDomainPolicies { get; } =
			new ResponseHeaderOptions("X-Permitted-Cross-Domain-Policies", "none", Protocol.httpAndHttps);

		public static IResponseHeaderOptions XXssProtection { get; } =
			new ResponseHeaderOptions("x-xss-protection", "1; mode=block", Protocol.httpAndHttps)
			.ApplyToContentType("text/html");

		public static IResponseHeaderOptions ExpectCT { get; } =
			new ResponseHeaderOptions("Expect-CT", "max-age=0, enforce, report-uri=\"https://example.report-uri.com/r/d/ct/enforce\"", Protocol.httpAndHttps);

		public static IResponseHeaderOptions FeaturePolicy { get; } =
			new ResponseHeaderOptions(
				"Feature-Policy",
				"accelerometer 'none';" +
					"ambient-light-sensor 'none';" +
					"autoplay 'none';" +
					"battery 'none';" +
					"camera 'none';" +
					"display-capture 'none';" +
					"document-domain 'none';" +
					"encrypted-media 'none';" +
					"execution-while-not-rendered 'none';" +
					"execution-while-out-of-viewport 'none';" +
					"gyroscope 'none';" +
					"magnetometer 'none';" +
					"microphone 'none';" +
					"midi 'none';" +
					"navigation-override 'none';" +
					"payment 'none';" +
					"picture-in-picture 'none';" +
					"publickey-credentials-get 'none';" +
					"sync-xhr 'none';" +
					"usb 'none';" +
					"wake-lock 'none';" +
					"xr-spatial-tracking 'none';",
				Protocol.httpAndHttps)
				.ApplyToContentType("text/html");

		public static IResponseHeaderOptions ContentSecurityPolicy { get; } =
			new ResponseHeaderOptions(
				"Content-Security-Policy",
				"base-uri 'none';" +
					"block-all-mixed-content;" +
					"child-src 'none';" +
					"connect-src 'none';" +
					"default-src 'none';" +
					"font-src 'none';" +
					"form-action 'none';" + //self
					"frame-ancestors 'none';" +
					"frame-src 'none';" +
					"img-src 'none';" +
					"manifest-src 'none';" +
					"media-src 'none';" +
					"object-src 'none';" +
					"sandbox;" +
					"script-src 'none';" +
					"script-src-attr 'none';" +
					"script-src-elem 'none';" +
					"style-src 'none';" +
					"style-src-attr 'none';" +
					"style-src-elem 'none';" +
					"upgrade-insecure-requests;" +
					"worker-src 'none';",
				Protocol.httpAndHttps)
				.ApplyToContentType("text/html");
	}
}

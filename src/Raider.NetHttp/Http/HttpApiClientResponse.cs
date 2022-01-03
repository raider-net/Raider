using Raider.Net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Raider.NetHttp.Http
{
	public class HttpApiClientResponse
	{
		public HttpApiClientRequest? Request { get; set; }
		public int? StatusCode { get; set; }
		public List<KeyValuePair<string, IEnumerable<string>>> Headers { get; set; }
		public List<KeyValuePair<string, IEnumerable<string>>> ContentHeaders { get; set; }
		public Stream? ContentStream { get; set; }
		public byte[]? ContentByteArray { get; set; }
		public string? Content { get; set; }
		public bool? RequestTimedOut { get; set; }
		public bool? OperationCanceled { get; set; }
		public string? Exception { get; set; }



		private bool LogUnknownResponseContentTypes { get; set; } = true;
		private bool LogAllResponseContentTypes { get; set; } = true;
		public List<string>? LoggedResponseAsByteArrayContentTypes { get; set; }
		public List<string>? LoggedResponseAsStringContentTypes { get; set; }
		public List<string>? NotLoggedResponsePaths { get; set; }






#if NETSTANDARD2_0 || NETSTANDARD2_1
		[Newtonsoft.Json.JsonIgnore]
#elif NET5_0
		[System.Text.Json.Serialization.JsonIgnore]
#endif
		public bool StatusCodeIsOK =>
		StatusCode.HasValue
			&& StatusCode.Value < 400;


#if NETSTANDARD2_0 || NETSTANDARD2_1
		[Newtonsoft.Json.JsonIgnore]
#elif NET5_0
		[System.Text.Json.Serialization.JsonIgnore]
#endif
		public bool IsOK =>
			StatusCodeIsOK
			&& string.IsNullOrWhiteSpace(Exception)
			&& OperationCanceled != true
			&& RequestTimedOut != true;

		public HttpApiClientResponse()
		{
			Headers = new List<KeyValuePair<string, IEnumerable<string>>>();
			ContentHeaders = new List<KeyValuePair<string, IEnumerable<string>>>();
			LoggedResponseAsStringContentTypes = new List<string>
			{
				"application/x-www-form-urlencoded",
				"application/ecmascript",
				"application/javascript",
				"application/json",
				"application/problem+json",
				"application/jsonml+json",
				"application/plain",
				"application/vnd.dvb.service",
				"application/xhtml+xml",
				"application/x-javascript",
				"application/xml",
				"text/ecmascript",
				"text/html",
				"text/javascript",
				"text/plain",
				"text/xml"
			};
		}

		public bool CanLogResponse(string? path, string? contentType, out bool bodyAsString)
			=> IsResponseContentTypeAllowed(contentType, out bodyAsString)
				&& IsResponsePathAllowed(path);

		private bool IsResponseContentTypeAllowed(string? contentType, out bool bodyAsString)
		{
			if (string.IsNullOrWhiteSpace(contentType))
			{
				bodyAsString = false;
				return LogUnknownResponseContentTypes;
			}
			else
			{
				if (0 < LoggedResponseAsStringContentTypes?.Count)
				{
					if (LoggedResponseAsStringContentTypes
							.Any(x => -1 < x.IndexOf(contentType, StringComparison.OrdinalIgnoreCase)
									|| -1 < contentType.IndexOf(x, StringComparison.OrdinalIgnoreCase)))
					{
						bodyAsString = true;
						return true;
					}

					if (0 < LoggedResponseAsByteArrayContentTypes?.Count)
					{
						bodyAsString = false;
						return LogAllResponseContentTypes
							|| LoggedResponseAsByteArrayContentTypes
								.Any(x => -1 < x.IndexOf(contentType, StringComparison.OrdinalIgnoreCase)
										|| -1 < contentType.IndexOf(x, StringComparison.OrdinalIgnoreCase));
					}
					else
					{
						bodyAsString = false;
						return LogAllResponseContentTypes;
					}
				}
				else
				{
					if (0 < LoggedResponseAsByteArrayContentTypes?.Count)
					{
						bodyAsString = false;
						return LogAllResponseContentTypes
							|| LoggedResponseAsByteArrayContentTypes
								.Any(x => -1 < x.IndexOf(contentType, StringComparison.OrdinalIgnoreCase)
										|| -1 < contentType.IndexOf(x, StringComparison.OrdinalIgnoreCase));
					}
					else
					{
						bodyAsString = false;
						return LogAllResponseContentTypes;
					}
				}
			}
		}

		private bool IsResponsePathAllowed(string? path)
		{
			if (string.IsNullOrWhiteSpace(path))
			{
				return true;
			}
			else
			{
				if (0 < NotLoggedResponsePaths?.Count)
				{
					return NotLoggedResponsePaths.Any(x => path.StartsWith(x, StringComparison.OrdinalIgnoreCase));
				}
				else
				{
					return true;
				}
			}
		}

		//public virtual T GetContent<T>()
		//{
		//	if (string.IsNullOrWhiteSpace(Content)) //TODO CACHE!!!!
		//		return default;

		//	return JsonConvert.DeserializeObject<T>(Content);
		//}

		internal async Task FromHttpResponseMessageAsync(
			System.Net.Http.HttpResponseMessage httpResponseMessage,
			CancellationToken cancellationToken = default)
		{
			if (httpResponseMessage == null)
				throw new ArgumentNullException(nameof(httpResponseMessage));

			StatusCode = (int)httpResponseMessage.StatusCode;

			if (httpResponseMessage.Headers != null)
				foreach (var header in httpResponseMessage.Headers)
					Headers.Add(header);

			if (httpResponseMessage.Content.Headers != null)
				foreach (var header in httpResponseMessage.Content.Headers)
					ContentHeaders.Add(header);

			await ReadHttpContentAsync(httpResponseMessage.Content, cancellationToken);
		}

		protected virtual Task ReadHttpContentAsync(
			System.Net.Http.HttpContent httpContent,
			CancellationToken cancellationToken = default)
		{
			return Task.CompletedTask;
		}

		public string? GetException()
		{
			if (IsOK)
				return null;

			var uri = Request?.GetRequestUri()?.ToString();

			string text;
			if (string.IsNullOrWhiteSpace(Exception))
			{
				if (RequestTimedOut == true)
					text = $"The request {uri} timed out.";
				else if (OperationCanceled == true)
					text = $"Operation {uri} was canceled by client CancellationToken.";
				else
					text = $"Uri: {uri} Content:{Environment.NewLine}{Content}";
			}
			else
			{
				if (string.IsNullOrWhiteSpace(Content))
				{
					text = $"Uri: {uri} Exception:{Environment.NewLine}{Exception}";
				}
				else
				{
					text = $"Uri: {uri} Exception:{Environment.NewLine}{Exception}{Environment.NewLine}{Environment.NewLine}Content:{Environment.NewLine}{Content}";
				}
			}

			if (StatusCode.HasValue)
				return $"StatusCode:{StatusCode.Value}: {text}";
			else
				return text;
		}

		public static async Task<T?> DeserializeFromJsonStreamAsync<T>(
			Stream utf8JsonStream,
			CancellationToken cancellationToken = default)
		{
			if (utf8JsonStream == null)
				throw new ArgumentNullException(nameof(utf8JsonStream));

#if NETSTANDARD2_0 || NETSTANDARD2_1
			using var streamReader = new StreamReader(utf8JsonStream, new System.Text.UTF8Encoding(false));
			using var jsonTextReader = new Newtonsoft.Json.JsonTextReader(streamReader);
			var jToken = await Newtonsoft.Json.Linq.JToken.LoadAsync(jsonTextReader);
			return jToken.ToObject<T>();
#elif NET5_0
			var jsonOptions = new System.Text.Json.JsonSerializerOptions
			{
				PropertyNameCaseInsensitive = true
			};
			var result = await System.Text.Json.JsonSerializer.DeserializeAsync<T>(utf8JsonStream, jsonOptions, cancellationToken);
			return result;
#endif
		}

		public override string ToString()
		{
#if NETSTANDARD2_0 || NETSTANDARD2_1
			return Newtonsoft.Json.JsonConvert.SerializeObject(this, Newtonsoft.Json.Formatting.Indented);
#elif NET5_0
			return System.Text.Json.JsonSerializer.Serialize(this, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
#endif
		}
	}

	public class HttpApiClientResponse<T> : HttpApiClientResponse
	{
		public T? ContentData { get; private set; }

		public HttpApiClientResponse()
			: base()
		{ }

		protected override async Task ReadHttpContentAsync(
			System.Net.Http.HttpContent httpContent,
			CancellationToken cancellationToken = default)
		{
#if NETSTANDARD2_0 || NETSTANDARD2_1
				var stream = await httpContent.ReadAsStreamAsync();
#elif NET5_0
			var stream = await httpContent.ReadAsStreamAsync(cancellationToken);
#endif
			MemoryStream? ms = null;

			var contentTypeKvp = ContentHeaders.FirstOrDefault(x => string.Equals("Content-Type", x.Key, StringComparison.OrdinalIgnoreCase));

			string? contentType = null;
			if (string.Equals("Content-Type", contentTypeKvp.Key, StringComparison.OrdinalIgnoreCase))
			{
				contentType = contentTypeKvp.Value.FirstOrDefault();

				if (contentType?.StartsWith(MimeTypes.json, StringComparison.OrdinalIgnoreCase) ?? false)
				{
					try
					{
						ms = new MemoryStream();
#if NETSTANDARD2_0 || NETSTANDARD2_1
					await stream.CopyToAsync(ms);
#elif NET5_0
						await stream.CopyToAsync(ms, cancellationToken);
#endif
						ms.Seek(0, SeekOrigin.Begin);

						ContentData = await DeserializeFromJsonStreamAsync<T>(ms, cancellationToken);
					}
					catch { } //TODO: LOG
				}
			}

			if (CanLogResponse(Request?.RelativePath, contentType, out bool bodyAsString))
			{
				if (bodyAsString)
				{
					try
					{
						if (ms == null)
						{
							ms = new MemoryStream();
#if NETSTANDARD2_0 || NETSTANDARD2_1
					await stream.CopyToAsync(ms);
#elif NET5_0
							await stream.CopyToAsync(ms, cancellationToken);
#endif
						}

						ms.Seek(0, SeekOrigin.Begin);
						using var reader = new StreamReader(ms, true);
						Content = reader.ReadToEnd();
					}
					catch { }
				}
				else
				{
					try
					{
						if (ms == null)
						{
							ms = new MemoryStream();
#if NETSTANDARD2_0 || NETSTANDARD2_1
					await stream.CopyToAsync(ms);
#elif NET5_0
							await stream.CopyToAsync(ms, cancellationToken);
#endif
						}

						ms.Seek(0, SeekOrigin.Begin);
						ContentByteArray = ms.ToArray();
					}
					catch { }
				}
			}
			else
			{
				if (ms != null)
				{
					ms.Seek(0, SeekOrigin.Begin);
					ContentStream = ms;
				}
				else
				{
					ContentStream = stream;
				}
			}
		}
	}
}

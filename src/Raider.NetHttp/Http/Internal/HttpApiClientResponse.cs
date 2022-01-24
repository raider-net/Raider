#if NET5_0
using Raider.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Raider.NetHttp.Http.Internal
{
	internal class HttpApiClientResponse : IHttpApiClientResponse, IDisposable
	{
		private bool disposedValue;

		public IHttpApiClientRequest Request { get; }
		public HttpResponseMessage? Response { get; set; }

		public int? StatusCode => (int?)Response?.StatusCode;

		public bool? RequestTimedOut { get; set; }

		public bool? OperationCanceled { get; set; }

		[System.Text.Json.Serialization.JsonIgnore]
		public Exception? Exception { get; set; }

		public string? ExceptionText => Exception?.ToStringTrace();

		[System.Text.Json.Serialization.JsonIgnore]
		public bool StatusCodeIsOK =>
		StatusCode.HasValue
			&& StatusCode.Value < 400;

		[System.Text.Json.Serialization.JsonIgnore]
		public bool IsOK =>
			StatusCodeIsOK
			&& Exception == null
			&& OperationCanceled != true
			&& RequestTimedOut != true;

		public HttpApiClientResponse(IHttpApiClientRequest request)
		{
			Request = request ?? throw new ArgumentNullException(nameof(request));
		}

		public Task CopyContentToAsync(Stream stream, CancellationToken cancellationToken)
		{
			if (Response?.Content != null)
			{
				Response.Content.CopyToAsync(stream, cancellationToken);
			}

			return Task.CompletedTask;
		}

		public List<KeyValuePair<string, IEnumerable<string>>>? GetAllHeaders()
		{
			var responseHeaders = GetResponseHeaders();

			if (responseHeaders == null)
			{
				return GetContentHeaders();
			}
			else
			{
				var contentHeader = GetContentHeaders();
				if (contentHeader != null)
					responseHeaders.AddRange(contentHeader);

				return responseHeaders;
			}
		}

		public List<KeyValuePair<string, IEnumerable<string>>>? GetResponseHeaders()
			=> Response?.Headers?.ToList();

		public List<KeyValuePair<string, IEnumerable<string>>>? GetContentHeaders()
			=> Response?.Content.Headers?.ToList();

		public Task<Stream?> ReadContentAsStreamAsync(CancellationToken cancellationToken)
			=> Response?.Content == null
				? Task.FromResult((Stream?)null)
				: Response.Content.ReadAsStreamAsync(cancellationToken) as Task<Stream?>;

		public Task<byte[]?> ReadContentAsByteArrayAsync(CancellationToken cancellationToken)
			=> Response?.Content == null
				? Task.FromResult((byte[]?)null)
				: Response.Content.ReadAsByteArrayAsync(cancellationToken) as Task<byte[]?>;

		public Task<string?> ReadContentAsStringAsync(CancellationToken cancellationToken)
			=> Response?.Content == null
				? Task.FromResult((string?)null)
				: Response.Content.ReadAsStringAsync(cancellationToken) as Task<string?>;

		public async Task<T?> ReadJsonContentAsAsync<T>(
			System.Text.Json.JsonSerializerOptions? jsonSerializerOptions = null, 
			CancellationToken cancellationToken = default)
		{
			if (Response == null)
				return default;

			using var stream = await Response.Content.ReadAsStreamAsync(cancellationToken);
			var result = await System.Text.Json.JsonSerializer.DeserializeAsync<T>(stream, jsonSerializerOptions, cancellationToken);
			return result;
		}

		public override string ToString()
		{
			return System.Text.Json.JsonSerializer.Serialize(this, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing && Response != null)
				{
					Response.Dispose();
				}

				disposedValue = true;
			}
		}

		public void Dispose()
		{
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}
	}
}
#endif

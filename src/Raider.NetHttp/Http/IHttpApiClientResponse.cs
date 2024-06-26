﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Raider.NetHttp.Http
{
	public interface IHttpApiClientResponse : IDisposable
	{
		IHttpApiClientRequest Request { get; }
		HttpResponseMessage? HttpResponseMessage { get; }
		int? StatusCode { get; }
		bool? RequestTimedOut { get; }
		bool? OperationCanceled { get; }

#if NETSTANDARD2_0 || NETSTANDARD2_1
		[Newtonsoft.Json.JsonIgnore]
#elif NET5_0_OR_GREATER
		[System.Text.Json.Serialization.JsonIgnore]
#endif
		Exception? Exception { get; }
		string? ExceptionText { get; }

#if NETSTANDARD2_0 || NETSTANDARD2_1
		[Newtonsoft.Json.JsonIgnore]
#elif NET5_0_OR_GREATER
		[System.Text.Json.Serialization.JsonIgnore]
#endif
		bool StatusCodeIsOK { get; }

#if NETSTANDARD2_0 || NETSTANDARD2_1
		[Newtonsoft.Json.JsonIgnore]
#elif NET5_0_OR_GREATER
		[System.Text.Json.Serialization.JsonIgnore]
#endif
		bool IsOK { get; }

		List<KeyValuePair<string, IEnumerable<string>>>? GetAllHeaders();
		List<KeyValuePair<string, IEnumerable<string>>>? GetResponseHeaders();
		List<KeyValuePair<string, IEnumerable<string>>>? GetContentHeaders();

		Task CopyContentToAsync(Stream stream, CancellationToken cancellationToken);
		Task<Stream?> ReadContentAsStreamAsync(CancellationToken cancellationToken);
		Task<byte[]?> ReadContentAsByteArrayAsync(CancellationToken cancellationToken);
		Task<string?> ReadContentAsStringAsync(CancellationToken cancellationToken);
#if NETSTANDARD2_0 || NETSTANDARD2_1
		Task<T?> ReadJsonContentAsAsync<T>(Newtonsoft.Json.JsonSerializerSettings? jsonSerializerOptions = null, CancellationToken cancellationToken = default);
#elif NET5_0_OR_GREATER
		Task<T?> ReadJsonContentAsAsync<T>(System.Text.Json.JsonSerializerOptions? jsonSerializerOptions = null, CancellationToken cancellationToken = default);
#endif
	}
}

using Raider.Extensions;
using Raider.Web.Logging;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Raider.NetHttp.Http
{
	public static class ResponseDtoMapper
	{
		public static async Task<ResponseDto> Map(
			HttpResponseMessage httpResponse,
			Guid? correlationId,
			string? externalCorrelationId,
			long? elapsedMilliseconds,
			bool logResponseHeaders,
			bool logResponseBodyAsString,
			bool logResponseBodyAsByteArray,
			CancellationToken cancellationToken)
		{
			if (httpResponse == null)
				throw new ArgumentNullException(nameof(httpResponse));

			var response = new ResponseDto
			{
				CorrelationId = correlationId,
				ExternalCorrelationId = externalCorrelationId,
				StatusCode = (int)httpResponse.StatusCode,
				ElapsedMilliseconds = elapsedMilliseconds,
			};

			if (logResponseHeaders)
			{
				try
				{
					if (httpResponse.Headers != null)
					{
						var headers = httpResponse.Headers.ToDictionary(x => x.Key, x => x.Value);

						if (httpResponse.Content?.Headers != null)
						{
							var contentHeaders = httpResponse.Content.Headers.ToDictionary(x => x.Key, x => x.Value);
							headers.AddOrReplaceRange(contentHeaders);
						}

#if NETSTANDARD2_0 || NETSTANDARD2_1
						response.Headers = Newtonsoft.Json.JsonConvert.SerializeObject(headers);
#elif NET5_0
						response.Headers = System.Text.Json.JsonSerializer.Serialize(headers);
#endif
					}
				}
				catch { }
			}

			if (logResponseBodyAsString)
			{
				if (httpResponse.Content != null)
					response.Body = await httpResponse.Content.ReadAsStringAsync(
#if NET5_0
						cancellationToken
#endif
						).ConfigureAwait(false);

				if (string.IsNullOrWhiteSpace(response.Body))
					response.Body = null;
			}

			if (logResponseBodyAsByteArray)
			{
				if (httpResponse.Content != null)
					response.BodyByteArray = await httpResponse.Content.ReadAsByteArrayAsync(
#if NET5_0
						cancellationToken
#endif
						).ConfigureAwait(false);

				if (response.BodyByteArray != null && response.BodyByteArray.Length == 0)
					response.BodyByteArray = null;
			}

			return response;
		}
	}
}

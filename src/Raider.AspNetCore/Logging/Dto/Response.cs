using Microsoft.AspNetCore.Http;
using Raider.Infrastructure;
using System;
using System.Collections.Generic;

namespace Raider.AspNetCore.Logging.Dto
{
	public class Response : Serializer.IDictionaryObject
	{
		public Guid RuntimeUniqueKey { get; set; }
		public DateTimeOffset Created { get; set; }
		public Guid? CorrelationId { get; set; }
		public string? ExternalCorrelationId { get; set; }
		public int? StatusCode { get; set; }
		public string? Headers { get; set; }
		public string? Body { get; set; }
		public byte[]? BodyByteArray { get; set; }
		public string? Error { get; set; }
		public decimal? ElapsedMilliseconds { get; set; }

		public Response()
		{
			RuntimeUniqueKey = EnvironmentInfo.RUNTIME_UNIQUE_KEY;
			Created = DateTimeOffset.Now;
		}

		public static Response Create(
			HttpResponse httpResponse,
			Guid correlationId,
			string? externalCorrelationId,
			int? statusCode,
			string? body,
			byte[]? bodyByteArray,
			string? error,
			decimal? elapsedMilliseconds,
			bool logRequestHeaders)
		{
			if (httpResponse == null)
				throw new ArgumentNullException(nameof(httpResponse));

			var request = new Response
			{
				CorrelationId = correlationId,
				ExternalCorrelationId = externalCorrelationId,
				StatusCode = statusCode,
				Body = string.IsNullOrWhiteSpace(body)
					? null
					: body,
				BodyByteArray = (bodyByteArray != null && bodyByteArray.Length == 0)
					? null
					: bodyByteArray,
				Error = error,
				ElapsedMilliseconds = elapsedMilliseconds,
			};

			if (logRequestHeaders)
			{
				try
				{
					if (httpResponse.Headers != null)
					{
						var headers = new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>(httpResponse.Headers);
						request.Headers = System.Text.Json.JsonSerializer.Serialize(headers);
					}
				}
				catch { }
			}

			return request;
		}

		public IDictionary<string, object?> ToDictionary()
		{
			var dict = new Dictionary<string, object?>
			{
				{ nameof(RuntimeUniqueKey), RuntimeUniqueKey },
				{ nameof(Created), Created },
			};

			if (CorrelationId.HasValue)
				dict.Add(nameof(CorrelationId), CorrelationId);

			if (!string.IsNullOrWhiteSpace(ExternalCorrelationId))
				dict.Add(nameof(ExternalCorrelationId), ExternalCorrelationId);

			if (StatusCode.HasValue)
				dict.Add(nameof(StatusCode), StatusCode);

			if (!string.IsNullOrWhiteSpace(Headers))
				dict.Add(nameof(Headers), Headers);

			if (!string.IsNullOrWhiteSpace(Body))
				dict.Add(nameof(Body), Body);

			if (BodyByteArray != null)
				dict.Add(nameof(BodyByteArray), BodyByteArray);

			if (!string.IsNullOrWhiteSpace(Error))
				dict.Add(nameof(Error), Error);

			if (ElapsedMilliseconds.HasValue)
				dict.Add(nameof(ElapsedMilliseconds), ElapsedMilliseconds);

			return dict;
		}
	}
}

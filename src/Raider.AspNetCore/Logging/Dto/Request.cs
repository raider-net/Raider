using Microsoft.AspNetCore.Http;
using Raider.AspNetCore.Middleware.Tracking;
using Raider.Infrastructure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Raider.AspNetCore.Logging.Dto
{
	internal class Request : Serializer.IDictionaryObject
	{
		public Guid RuntimeUniqueKey { get; set; }
		public DateTimeOffset Created { get; set; }
		public Guid? CorrelationId { get; set; }
		public string? ExternalCorrelationId { get; set; }
		public string? Protocol { get; set; }
		public string? Scheme { get; set; }
		public string? Host { get; set; }
		public string? Method { get; set; }
		public string? Path { get; set; }
		public string? QueryString { get; set; }
		public string? Headers { get; set; }
		public string? Body { get; set; }
		public byte[]? BodyByteArray { get; set; }
		public string? Form { get; set; }
		public string? Files { get; set; }

		public Request()
		{
			RuntimeUniqueKey = EnvironmentInfo.RUNTIME_UNIQUE_KEY;
			Created = DateTimeOffset.Now;
		}

		public static async Task<Request> Create(
			HttpRequest httpRequest,
			Guid correlationId,
			string? externalCorrelationId,
			bool logRequestHeaders,
			bool logRequestForm,
			bool logRequestBodyAsString,
			bool logRequestBodyAsByteArray,
			bool logRequestFiles)
		{
			if (httpRequest == null)
				throw new ArgumentNullException(nameof(httpRequest));

			var request = new Request
			{
				CorrelationId = correlationId,
				ExternalCorrelationId = externalCorrelationId
			};

			try { request.Protocol = httpRequest.Protocol; } catch { }
			try { request.Scheme = httpRequest.Scheme; } catch { }
			try { request.Host = httpRequest.Host.ToString(); } catch { }
			try { request.Method = httpRequest.Method; } catch { }
			try { request.Path = httpRequest.Path; } catch { }
			try { request.QueryString = httpRequest.QueryString.ToString(); } catch { }

			if (logRequestHeaders)
			{
				try
				{
					if (httpRequest.Headers != null)
					{
						var headers = new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>(httpRequest.Headers);
						request.Headers = System.Text.Json.JsonSerializer.Serialize(headers);
					}
				}
				catch { }
			}

			if (logRequestForm)
			{
				try
				{
					if (httpRequest.HasFormContentType && httpRequest.Form != null)
					{
						var form = new List<KeyValuePair<string, Microsoft.Extensions.Primitives.StringValues>>(httpRequest.Form);
						request.Form = System.Text.Json.JsonSerializer.Serialize(form);
					}
				}
				catch { }
			}

			if (logRequestBodyAsString)
			{
				var originalRequestBody = httpRequest.Body;
				try
				{
					using (var requestBodyStream = new MemoryStream())
					{
						await httpRequest.Body.CopyToAsync(requestBodyStream);
						requestBodyStream.Seek(0, SeekOrigin.Begin);
						request.Body = new StreamReader(requestBodyStream /* TODO , encoding*/).ReadToEnd();
					}
					originalRequestBody.Seek(0, SeekOrigin.Begin);
				}
				finally
				{
					httpRequest.Body = originalRequestBody;
				}
			}

			if (logRequestBodyAsByteArray)
			{
				var originalRequestBody = httpRequest.Body;
				try
				{
					using (var requestBodyStream = new MemoryStream())
					{
						await httpRequest.Body.CopyToAsync(requestBodyStream);
						requestBodyStream.Seek(0, SeekOrigin.Begin);
						request.BodyByteArray = requestBodyStream.ToArray();
					}
					originalRequestBody.Seek(0, SeekOrigin.Begin);
				}
				finally
				{
					httpRequest.Body = originalRequestBody;
				}
			}

			if (logRequestFiles)
			{
				//TODO
			}

			return request;
		}

		public IReadOnlyDictionary<string, object?> ToDictionary()
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

			if (!string.IsNullOrWhiteSpace(Protocol))
				dict.Add(nameof(Protocol), Protocol);

			if (!string.IsNullOrWhiteSpace(Scheme))
				dict.Add(nameof(Scheme), Scheme);

			if (!string.IsNullOrWhiteSpace(Host))
				dict.Add(nameof(Host), Host);

			if (!string.IsNullOrWhiteSpace(Method))
				dict.Add(nameof(Method), Method);

			if (!string.IsNullOrWhiteSpace(Path))
				dict.Add(nameof(Path), Path);

			if (!string.IsNullOrWhiteSpace(QueryString))
				dict.Add(nameof(QueryString), QueryString);

			if (!string.IsNullOrWhiteSpace(Headers))
				dict.Add(nameof(Headers), Headers);

			if (!string.IsNullOrWhiteSpace(Body))
				dict.Add(nameof(Body), Body);

			if (BodyByteArray != null)
				dict.Add(nameof(BodyByteArray), BodyByteArray);

			if (!string.IsNullOrWhiteSpace(Form))
				dict.Add(nameof(Form), Form);

			if (!string.IsNullOrWhiteSpace(Files))
				dict.Add(nameof(Files), Files);

			return dict;
		}
	}
}

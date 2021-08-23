using Raider.Extensions;
using Raider.NetHttp.Http.Headers;
using Raider.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace Raider.NetHttp.Http
{
	public class HttpApiClientRequest
	{
		public const string MultipartFormData = "form-data";

		public string? BaseAddress { get; set; }
		public string? RelativePath { get; set; }
		public string? QueryString { get; set; }
		public string? HttpMethod { get; set; }
		public bool ClearDefaultHeaders { get; set; }
		public RequestHeaders Headers { get; }
		public string? MultipartSubType { get; set; }
		public string? MultipartBoundary { get; set; }
		public List<KeyValuePair<string, string>>? FormData { get; set; }
		public List<StringContent>? StringContents { get; set; }
		public List<StreamContent>? StreamContents { get; set; }
		public List<ByteArrayContent>? ByteArrayContents { get; set; }

		public HttpApiClientRequest()
		{
			Headers = new RequestHeaders();
		}

		public HttpApiClientRequest SetBaseAddress(string? baseAddress)
		{
			BaseAddress = baseAddress;
			return this;
		}

		public HttpApiClientRequest SetRelativePath(string path)
		{
			RelativePath = string.IsNullOrWhiteSpace(path)
				? ""
				: path.TrimPrefix("/");
			return this;
		}

		public HttpApiClientRequest SetQueryString(string queryString)
		{
			QueryString = string.IsNullOrWhiteSpace(queryString)
				? null
				: $"?{queryString.TrimPrefix("?")}";
			return this;
		}

		public HttpApiClientRequest SetQueryString(Dictionary<string, string> queryString)
		{
			if (queryString == null || queryString.Count == 0)
				QueryString = null;
			else
				QueryString = $"?{string.Join("&", queryString.Select(kvp => $"{System.Net.WebUtility.UrlEncode(kvp.Key)}={System.Net.WebUtility.UrlEncode(kvp.Value)}"))}";

			return this;
		}

		public HttpApiClientRequest AddQueryString(string key, string value)
		{
			if (string.IsNullOrWhiteSpace(key))
				return this;

			if (string.IsNullOrWhiteSpace(QueryString))
			{
				SetQueryString(new Dictionary<string, string> { { key, value } });
			}
			else
			{
				QueryString = $"{QueryString}&{key}={value}";
			}

			return this;
		}

		public HttpApiClientRequest SetMethod(string httpMethod)
		{
			if (string.IsNullOrWhiteSpace(httpMethod))
				throw new ArgumentNullException(nameof(httpMethod));

			HttpMethod = httpMethod;
			return this;
		}

		public HttpApiClientRequest AddFormData(List<KeyValuePair<string, string>> formData)
		{
			if (formData == null || formData.Count == 0)
				return this;

			if (FormData == null)
				FormData = new List<KeyValuePair<string, string>>();

			FormData.AddRange(formData);

			return this;
		}

		public HttpApiClientRequest AddStringContent(StringContent stringContent)
		{
			if (stringContent == null)
				throw new ArgumentNullException(nameof(stringContent));

			if (StringContents == null)
				StringContents = new List<StringContent>();

			StringContents.Add(stringContent);
			return this;
		}

		public HttpApiClientRequest AddStreamContent(StreamContent streamContent)
		{
			if (streamContent == null)
				throw new ArgumentNullException(nameof(streamContent));

			if (StreamContents == null)
				StreamContents = new List<StreamContent>();

			StreamContents.Add(streamContent);
			return this;
		}

		public HttpApiClientRequest AddByteArrayContent(ByteArrayContent byteArrayContent)
		{
			if (byteArrayContent == null)
				throw new ArgumentNullException(nameof(byteArrayContent));

			if (ByteArrayContents == null)
				ByteArrayContents = new List<ByteArrayContent>();

			ByteArrayContents.Add(byteArrayContent);
			return this;
		}

		public HttpApiClientRequest SetMultipart(string multipartSubType, string multipartBoundary)
		{
			if (string.IsNullOrWhiteSpace(multipartSubType))
				throw new ArgumentNullException(nameof(multipartSubType));

			MultipartSubType = multipartSubType;
			MultipartBoundary = multipartBoundary;
			return this;
		}

		public string? GetRequestUri()
			=> UriHelper.Combine(
				string.IsNullOrWhiteSpace(BaseAddress)
					? "/"
					: BaseAddress,
				$"{RelativePath}{QueryString}");

		public HttpRequestMessage ToHttpRequestMessage()
		{
			if (HttpMethod == null)
				throw new InvalidOperationException($"{HttpMethod} == null");

			var path = GetRequestUri();
			var httpRequestMessage = new HttpRequestMessage(new System.Net.Http.HttpMethod(HttpMethod), path);

			if (ClearDefaultHeaders)
				httpRequestMessage.Headers.Clear();

			Headers.SetHttpRequestHeaders(httpRequestMessage.Headers);

			var content = ToHttpContent();
			if (content != null)
				httpRequestMessage.Content = content;

			return httpRequestMessage;
		}

		private HttpContent? ToHttpContent()
		{
			var contentsCount =
				(FormData?.Count ?? 0)
				+ (StringContents?.Count ?? 0)
				+ (StreamContents?.Count ?? 0)
				+ (ByteArrayContents?.Count ?? 0);

			if (contentsCount == 0)
				return null;

			if (1 < contentsCount)
			{
				if (string.IsNullOrWhiteSpace(MultipartSubType))
					throw new InvalidOperationException($"{nameof(MultipartSubType)} == null");

				MultipartContent multipartContent;
				if (string.Equals(MultipartFormData, MultipartSubType, StringComparison.OrdinalIgnoreCase))
				{
					var multipartFormDataContent = string.IsNullOrWhiteSpace(MultipartBoundary)
						? new MultipartFormDataContent()
						: new MultipartFormDataContent(MultipartBoundary);

					if (FormData != null)
						foreach (var kvp in FormData)
							multipartFormDataContent.Add(new System.Net.Http.StringContent(kvp.Value), kvp.Key);

					if (StringContents != null)
						foreach (var stringContent in StringContents)
						{
							if (string.IsNullOrWhiteSpace(stringContent.HttpContentNameFormDataMultipartPurpose))
								multipartFormDataContent.Add(stringContent.ToStringContent());
							else
								multipartFormDataContent.Add(stringContent.ToStringContent(), stringContent.HttpContentNameFormDataMultipartPurpose);
						}

					if (StreamContents != null)
						foreach (var streamContent in StreamContents)
						{
							if (string.IsNullOrWhiteSpace(streamContent.HttpContentNameFormDataMultipartPurposeMultipartPurpose))
								multipartFormDataContent.Add(streamContent.ToStreamContent());
							else if (string.IsNullOrWhiteSpace(streamContent.HttpContentFileNameFormDataMultipartPurposeMultipartPurpose))
								multipartFormDataContent.Add(streamContent.ToStreamContent(), streamContent.HttpContentNameFormDataMultipartPurposeMultipartPurpose);
							else
								multipartFormDataContent.Add(streamContent.ToStreamContent(), streamContent.HttpContentNameFormDataMultipartPurposeMultipartPurpose, streamContent.HttpContentFileNameFormDataMultipartPurposeMultipartPurpose);
						}

					if (ByteArrayContents != null)
						foreach (var byteArrayContent in ByteArrayContents)
						{
							if (string.IsNullOrWhiteSpace(byteArrayContent.HttpContentNameFormDataMultipartPurposeMultipartPurpose))
								multipartFormDataContent.Add(byteArrayContent.ToByteArrayContent());
							else if (string.IsNullOrWhiteSpace(byteArrayContent.HttpContentFileNameFormDataMultipartPurposeMultipartPurpose))
								multipartFormDataContent.Add(byteArrayContent.ToByteArrayContent(), byteArrayContent.HttpContentNameFormDataMultipartPurposeMultipartPurpose);
							else
								multipartFormDataContent.Add(byteArrayContent.ToByteArrayContent(), byteArrayContent.HttpContentNameFormDataMultipartPurposeMultipartPurpose, byteArrayContent.HttpContentFileNameFormDataMultipartPurposeMultipartPurpose);
						}

					multipartContent = multipartFormDataContent;
				}
				else
				{
					multipartContent = string.IsNullOrWhiteSpace(MultipartBoundary)
						? new MultipartContent(MultipartSubType)
						: new MultipartContent(MultipartSubType, MultipartBoundary);

					if (FormData != null)
						foreach (var kvp in FormData)
							throw new InvalidOperationException($"{nameof(FormData)} must be send as {nameof(MultipartFormDataContent)}. {nameof(MultipartSubType)} must be equal to {MultipartFormData}");

					if (StringContents != null)
						foreach (var stringContent in StringContents)
							multipartContent.Add(stringContent.ToStringContent());

					if (StreamContents != null)
						foreach (var streamContent in StreamContents)
							multipartContent.Add(streamContent.ToStreamContent());

					if (ByteArrayContents != null)
						foreach (var byteArrayContent in ByteArrayContents)
							multipartContent.Add(byteArrayContent.ToByteArrayContent());
				}

				return multipartContent;
			}
			else
			{
				if (FormData != null && 0 < FormData.Count)
					throw new InvalidOperationException($"{nameof(FormData)} must be send as {nameof(MultipartFormDataContent)}");

				if (StringContents != null)
					foreach (var stringContent in StringContents)
						return stringContent.ToStringContent();

				if (StreamContents != null)
					foreach (var streamContent in StreamContents)
						return streamContent.ToStreamContent();

				if (ByteArrayContents != null)
					foreach (var byteArrayContent in ByteArrayContents)
						return byteArrayContent.ToByteArrayContent();
			}

			return null;
		}
	}
}

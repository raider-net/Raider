using Raider.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace Raider.NetHttp.Http
{
	public interface IRequestBuilder<TBuilder, TObject>
		where TBuilder : IRequestBuilder<TBuilder, TObject>
		where TObject : IHttpApiClientRequest
	{
		TBuilder Object(TObject request);

		TObject Build();

		HttpRequestMessage ToHttpRequestMessage();

		TBuilder BaseAddress(string? baseAddress, bool force = true);

		TBuilder RelativePath(string path, bool force = true);

		TBuilder QueryString(string queryString, bool force = true);

		TBuilder QueryString(Dictionary<string, string> queryString, bool force = true);

		TBuilder AddQueryString(string key, string value);

		TBuilder Method(string httpMethod, bool force = true);

		TBuilder Method(HttpMethod httpMethod, bool force = true);

		TBuilder AddFormData(List<KeyValuePair<string, string>> formData, bool force = true);

		TBuilder AddStringContent(StringContent stringContent, bool force = true);

		TBuilder AddStreamContent(StreamContent streamContent, bool force = true);

		TBuilder AddByteArrayContent(ByteArrayContent byteArrayContent, bool force = true);

		TBuilder Multipart(string multipartSubType, string multipartBoundary);
	}

	public abstract class RequestBuilderBase<TBuilder, TObject> : IRequestBuilder<TBuilder, TObject>
		where TBuilder : RequestBuilderBase<TBuilder, TObject>
		where TObject : IHttpApiClientRequest
	{
		protected readonly TBuilder _builder;
		protected TObject _request;

		protected RequestBuilderBase(TObject request)
		{
			_request = request;
			_builder = (TBuilder)this;
		}

		public virtual TBuilder Object(TObject request)
		{
			_request = request;
			return _builder;
		}

		public TObject Build()
			=> _request;

		public HttpRequestMessage ToHttpRequestMessage()
			=> _request.ToHttpRequestMessage();

		public TBuilder BaseAddress(string? baseAddress, bool force = true)
		{
			if (force || string.IsNullOrWhiteSpace(_request.BaseAddress))
				_request.BaseAddress = baseAddress;

			return _builder;
		}

		public TBuilder RelativePath(string path, bool force = true)
		{
			if (force || string.IsNullOrWhiteSpace(_request.RelativePath))
				_request.RelativePath = string.IsNullOrWhiteSpace(path)
					? ""
					: path.TrimPrefix("/");

			return _builder;
		}

		public TBuilder QueryString(string queryString, bool force = true)
		{
			if (force || string.IsNullOrWhiteSpace(_request.QueryString))
				_request.QueryString = string.IsNullOrWhiteSpace(queryString)
					? null
					: $"?{queryString.TrimPrefix("?")}";

			return _builder;
		}

		public TBuilder QueryString(Dictionary<string, string> queryString, bool force = true)
		{
			if (force || string.IsNullOrWhiteSpace(_request.QueryString))
			{
				if (queryString == null || queryString.Count == 0)
					_request.QueryString = null;
				else
					_request.QueryString = $"?{string.Join("&", queryString.Select(kvp => $"{System.Net.WebUtility.UrlEncode(kvp.Key)}={System.Net.WebUtility.UrlEncode(kvp.Value)}"))}";
			}

			return _builder;
		}

		public TBuilder AddQueryString(string key, string value)
		{
			if (string.IsNullOrWhiteSpace(key))
				return _builder;

			if (string.IsNullOrWhiteSpace(_request.QueryString))
			{
				QueryString(new Dictionary<string, string> { { key, value } });
			}
			else
			{
				_request.QueryString = $"{_request.QueryString}&{key}={value}";
			}

			return _builder;
		}

		public TBuilder Method(string httpMethod, bool force = true)
		{
			if (force || string.IsNullOrWhiteSpace(_request.HttpMethod))
			{
				if (string.IsNullOrWhiteSpace(httpMethod))
					throw new ArgumentNullException(nameof(httpMethod));

				_request.HttpMethod = httpMethod;
			}

			return _builder;
		}

		public TBuilder Method(HttpMethod httpMethod, bool force = true)
		{
			if (force || string.IsNullOrWhiteSpace(_request.HttpMethod))
				_request.HttpMethod = httpMethod.ToString();

			return _builder;
		}

		public TBuilder AddFormData(List<KeyValuePair<string, string>> formData, bool force = true)
		{
			if (force || _request.FormData == null)
			{
				if (formData == null || formData.Count == 0)
					return _builder;

				if (_request.FormData == null)
					_request.FormData = new List<KeyValuePair<string, string>>();

				_request.FormData.AddRange(formData);
			}

			return _builder;
		}

		public TBuilder AddStringContent(StringContent stringContent, bool force = true)
		{
			if (force || _request.StringContents == null)
			{
				if (stringContent == null)
					throw new ArgumentNullException(nameof(stringContent));

				if (_request.StringContents == null)
					_request.StringContents = new List<StringContent>();

				_request.StringContents.Add(stringContent);
			}

			return _builder;
		}

		public TBuilder AddStreamContent(StreamContent streamContent, bool force = true)
		{
			if (force || _request.StreamContents == null)
			{
				if (streamContent == null)
					throw new ArgumentNullException(nameof(streamContent));

				if (_request.StreamContents == null)
					_request.StreamContents = new List<StreamContent>();

				_request.StreamContents.Add(streamContent);
			}

			return _builder;
		}

		public TBuilder AddByteArrayContent(ByteArrayContent byteArrayContent, bool force = true)
		{
			if (force || _request.ByteArrayContents == null)
			{
				if (byteArrayContent == null)
					throw new ArgumentNullException(nameof(byteArrayContent));

				if (_request.ByteArrayContents == null)
					_request.ByteArrayContents = new List<ByteArrayContent>();

				_request.ByteArrayContents.Add(byteArrayContent);
			}

			return _builder;
		}

		public TBuilder Multipart(string multipartSubType, string multipartBoundary)
		{
			if (string.IsNullOrWhiteSpace(multipartSubType))
				throw new ArgumentNullException(nameof(multipartSubType));

			_request.MultipartSubType = multipartSubType;
			_request.MultipartBoundary = multipartBoundary;
			return _builder;
		}
	}

	public class RequestBuilder : RequestBuilderBase<RequestBuilder, IHttpApiClientRequest>
	{
		public RequestBuilder()
			: this(new HttpApiClientRequest())
		{
		}

		public RequestBuilder(IHttpApiClientRequest request)
			: base(request)
		{
		}

		public static implicit operator HttpApiClientRequest?(RequestBuilder builder)
		{
			if (builder == null)
				return null;

			return builder._request as HttpApiClientRequest;
		}

		public static implicit operator RequestBuilder?(HttpApiClientRequest request)
		{
			if (request == null)
				return null;

			return new RequestBuilder(request);
		}
	}
}

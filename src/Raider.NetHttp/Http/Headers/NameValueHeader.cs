using System;
using System.Net.Http.Headers;

namespace Raider.NetHttp.Http.Headers
{
	public class NameValueHeader
	{
		public string? Name { get; set; }
		public string? Value { get; set; }

		public NameValueHeaderValue ToNameValueHeaderValue()
		{
			if (string.IsNullOrWhiteSpace(Name))
				throw new InvalidOperationException($"{nameof(Name)} == null");

			return string.IsNullOrWhiteSpace(Value)
				? new NameValueHeaderValue(Name)
				: new NameValueHeaderValue(Name, Value);
		}
	}
}

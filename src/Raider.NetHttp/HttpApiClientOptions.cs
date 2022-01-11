using Raider.Policy;
using Raider.Security.Cryptography;
using Raider.Text;
using Raider.Validation;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Raider.NetHttp
{
	public class HttpApiClientOptions : IValidable
	{
		public string? BaseAddress { get; set; }
		public string? UserAgent { get; set; } = nameof(HttpApiClient);
		public Version? Version { get; set; }
		public DecompressionMethods? AutomaticDecompression { get; set; } = DecompressionMethods.GZip;
		public IWebProxy? Proxy { get; set; }
		public bool? UseDefaultCredentials { get; set; }
		public ICredentials? Credentials { get; set; }
		public Dictionary<string, IAsyncPolicy<HttpResponseMessage>>? UriPolicies { get; set; } //Dictionary<Uri, IAsyncPolicy<HttpResponseMessage>> OR ----- Wildcard ----- Dictionary<*, IAsyncPolicy<HttpResponseMessage>>
		public List<string>? LogDisabledUris { get; set; }
		public Dictionary<string, IRequestResponseLogger>? UriLoggers { get; set; } //Dictionary<Uri, IRequestResponseLogger> OR ----- Wildcard ----- Dictionary<*, IRequestResponseLogger>

		//Func<object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors, bool)>
		public Func<object, X509Certificate, X509Chain, SslPolicyErrors, bool>? RemoteCertificateValidationCallback { get; set; }
			= DefaultServerCertificateValidation.ServerCertificateValidation;

		public StringBuilder? Validate(string? propertyPrefix = null, StringBuilder? parentErrorBuffer = null, Dictionary<string, object>? validationContext = null)
		{
			if (string.IsNullOrWhiteSpace(BaseAddress))
			{
				if (parentErrorBuffer == null)
					parentErrorBuffer = new StringBuilder();

				parentErrorBuffer.AppendLine($"{StringHelper.ConcatIfNotNullOrEmpty(propertyPrefix, ".", nameof(BaseAddress))} == null");
			}

			return parentErrorBuffer;
		}
	}
}

using Raider.Security.Cryptography;
using System;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace Raider.NetHttp
{
	public class HttpApiClientOptions
	{
		public string? BaseAddress { get; set; }
		public string? UserAgent { get; set; } = nameof(HttpApiClient);
		public Version? Version { get; set; }
		public DecompressionMethods? AutomaticDecompression { get; set; } = DecompressionMethods.GZip;
		public IWebProxy? Proxy { get; set; }
		public bool? UseDefaultCredentials { get; set; }
		public ICredentials? Credentials { get; set; }

		//Func<object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors, bool)>
		public Func<object, X509Certificate, X509Chain, SslPolicyErrors, bool>? RemoteCertificateValidationCallback { get; set; }
			= DefaultServerCertificateValidation.ServerCertificateValidation;
	}
}

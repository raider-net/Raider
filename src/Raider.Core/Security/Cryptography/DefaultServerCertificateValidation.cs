using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace Raider.Security.Cryptography
{
	public static class DefaultServerCertificateValidation
	{
		public static bool ServerCertificateValidation(
			object s,
			X509Certificate certificate,
			X509Chain chain,
			SslPolicyErrors sslPolicyErrors)
			=> true;
	}
}

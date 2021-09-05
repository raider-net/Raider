using System.DirectoryServices.Protocols;
using System.Text;

namespace Raider
{
	public class LdapConnectorConfig
	{
		public string Server { get; set; }
		public int Port { get; set; }
		public AuthType AuthType { get; set; } = AuthType.Kerberos;
		public string DomainName { get; set; }
		public string UserName { get; set; }
		public string Password { get; set; }
		public bool SecureSocketLayer { get; set; }

		public string? Validate()
		{
			var sb = new StringBuilder();

			if (string.IsNullOrWhiteSpace(Server))
				sb.AppendLine($"{nameof(Server)} == null");

			if (Port <= 0)
				sb.AppendLine($"{nameof(Port)} <= 0");

			if (string.IsNullOrWhiteSpace(DomainName))
				sb.AppendLine($"{nameof(DomainName)} == null");

			if (string.IsNullOrWhiteSpace(UserName))
				sb.AppendLine($"{nameof(UserName)} == null");

			if (string.IsNullOrWhiteSpace(Password))
				sb.AppendLine($"{nameof(Password)} == null");

			var error = sb.ToString();
			return string.IsNullOrWhiteSpace(error)
				? null
				: error;
		}
	}
}

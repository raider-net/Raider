﻿using System;
using System.DirectoryServices.Protocols;
using System.Text;

namespace Raider.Ldap
{
	public class LdapConnectorConfig
	{
		public string Server { get; set; }
		public int Port { get; set; }
		public AuthType AuthType { get; set; } = AuthType.Kerberos;
		public string? DomainName { get; set; }
		public string? UserName { get; set; }
		public string? Password { get; set; }
		public bool SecureSocketLayer { get; set; }
		public int? TimeoutInSeconds { get; set; }
		public int? ProtocolVersion { get; set; }

		public string? Validate()
		{
			var sb = new StringBuilder();

			if (string.IsNullOrWhiteSpace(Server))
				sb.AppendLine($"{nameof(Server)} == null");

			if (Port <= 0)
				sb.AppendLine($"{nameof(Port)} <= 0");

			if (TimeoutInSeconds <= 0)
				sb.AppendLine($"{nameof(TimeoutInSeconds)} <= 0");

			var error = sb.ToString();
			return string.IsNullOrWhiteSpace(error)
				? null
				: error;
		}
	}
}

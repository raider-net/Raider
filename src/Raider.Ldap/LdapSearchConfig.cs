using System.Collections.Generic;
using System.DirectoryServices.Protocols;

namespace Raider.Ldap
{
	public class LdapSearchConfig
	{
		public string? DistinguishedName { get; set; }
		public string? LdapFilter { get; set; }
		public SearchScope SearchScope { get; set; } = SearchScope.Subtree;
		public List<string>? Attributes { get; set; }
	}
}

using System;
using System.Collections.Generic;

namespace Raider.Ldap
{
	public class LdapAttributeValues
	{
		public string Name { get; }
		public List<LdapValue> Values { get; }

		public LdapAttributeValues(string name)
		{
			if (string.IsNullOrWhiteSpace(name))
				throw new ArgumentNullException(nameof(name));

			Name = name;
			Values = new List<LdapValue>();
		}

		public override string ToString()
			=> 1 < Values.Count
				? $"{Name}: (Count={Values.Count}): [{string.Join(Environment.NewLine, Values)}]"
				: (Values.Count == 0 ? $"{Name}: " : $"{Name}: {Values[0]}");
	}
}

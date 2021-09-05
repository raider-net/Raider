using System;
using System.Collections.Generic;

namespace Raider
{
	public class LdapResultValue
	{
		public string Name { get; }
		public List<LdapValue> Values { get; }

		public LdapResultValue(string name)
		{
			if (string.IsNullOrWhiteSpace(name))
				throw new ArgumentNullException(nameof(name));

			Name = name;
			Values = new List<LdapValue>();
		}

		public override string ToString()
			=> 1 < Values.Count
				? $"{Name}: (Count={Values.Count}): [{string.Join(Environment.NewLine, Values)}]"
				: $"{Name}: {string.Join(Environment.NewLine, Values)}";
	}
}

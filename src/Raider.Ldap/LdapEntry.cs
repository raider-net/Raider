using System;
using System.Collections.Generic;

namespace Raider.Ldap
{
	public class LdapEntry
	{
		public List<LdapAttributeValues> Attributes { get; }

		public LdapEntry()
		{
			Attributes = new List<LdapAttributeValues>();
		}

		public LdapEntry(List<LdapAttributeValues> attributes)
		{
			Attributes = attributes ?? throw new ArgumentNullException(nameof(attributes));
		}

		public void AddAttribute(LdapAttributeValues attribute)
		{
			if (attribute == null)
				return;

			Attributes.Add(attribute);
		}

		public void AddAttributes(IEnumerable<LdapAttributeValues> attributes)
		{
			if (attributes == null)
				return;

			Attributes.AddRange(attributes);
		}

		public override string ToString()
			=> string.Join(Environment.NewLine, Attributes);
	}
}

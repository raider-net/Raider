using System;
using System.Collections.Generic;

namespace Raider.Ldap
{
	public class LdapSearchResult
	{
		public List<LdapEntry> Entries { get; }
		public List<string> Errors { get; }

		public LdapSearchResult()
		{
			Entries = new List<LdapEntry>();
			Errors = new List<string>();
		}

		public LdapSearchResult(List<LdapEntry> entries)
		{
			Entries = entries ?? throw new ArgumentNullException(nameof(entries));
			Errors = new List<string>();
		}

		public void AddEntry(LdapEntry entry)
		{
			if (entry == null)
				return;

			Entries.Add(entry);
		}

		public void AddEntries(IEnumerable<LdapEntry> entries)
		{
			if (entries == null)
				return;

			Entries.AddRange(entries);
		}

		public override string ToString()
			=> $"{nameof(Entries)} count=[{Entries.Count}]";
	}
}

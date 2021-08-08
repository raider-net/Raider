using System;

namespace Raider.Web
{
	public enum BrowserType
	{
		Generic,
		IE,
		Chrome,
		Firefox,
		Edge,
		Safari,
		Opera
	}

	public class BrowserInfo
	{
		public BrowserType Type { get; set; } = BrowserType.Generic;
		public Version? Version { get; set; }

		public override string ToString()
		{
			return $"{Type} {Version}";
		}

		public BrowserInfo Clone()
		{
			var clone = new BrowserInfo
			{
				Type = Type,
				Version = Version == null
					? null
					: new Version(
						Version.Major < 0 ? 0 : Version.Major,
						Version.Minor < 0 ? 0 : Version.Minor,
						Version.Build < 0 ? 0 : Version.Build,
						Version.Revision < 0 ? 0 : Version.Revision)
			};
			return clone;
		}
	}
}

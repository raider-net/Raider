using System;
using System.Security.Principal;

namespace Raider.Extensions
{
	public static class WindowsIdentityExtensions
	{
		public static string GetLogonNameWithoutDomain(this WindowsIdentity windowsIdentity)
		{
			if (windowsIdentity == null)
				throw new ArgumentNullException(nameof(windowsIdentity));

			if (string.IsNullOrWhiteSpace(windowsIdentity.Name))
				return windowsIdentity.Name;

			return windowsIdentity.Name.SubstringSafe(windowsIdentity.Name.IndexOf("\\") + 1);
		}
	}
}

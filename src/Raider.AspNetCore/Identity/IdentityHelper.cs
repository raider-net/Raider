using Microsoft.AspNetCore.Http;
using System;
using System.Security.Claims;
using System.Security.Principal;

namespace Raider.AspNetCore.Identity
{
	public static class IdentityHelper
	{
		public static bool IsWindowsAuthentication(HttpContext context)
		{
			return IsWindowsAuthentication(context?.User);
		}

		public static bool IsWindowsAuthentication(ClaimsPrincipal? claimsPrincipal)
		{
			return (claimsPrincipal is WindowsPrincipal)
					&& UsesWindowsAuthentication(claimsPrincipal?.Identity);
		}

		//https://docs.microsoft.com/en-us/dotnet/standard/security/principal-and-identity-objects
		public static bool UsesWindowsAuthentication(IIdentity? identity) //WindowsIdentity : ClaimsIdentity : IIdentity
		{
			return identity is WindowsIdentity;
		}

		public static void AddClaim(ClaimsPrincipal claimsPrincipal, Claim claim)
		{
			if (claimsPrincipal == null)
				throw new ArgumentNullException(nameof(claimsPrincipal));

			if (claimsPrincipal.Identity == null)
				throw new ArgumentNullException(nameof(claimsPrincipal.Identity));

			if (claim == null)
				throw new ArgumentNullException(nameof(claim));

			AddClaim(claimsPrincipal.Identity, claim);
		}

		public static void AddClaim(IIdentity identity, Claim claim)
		{
			if (identity == null)
				throw new ArgumentNullException(nameof(identity));

			var claimsIdentity = identity as ClaimsIdentity;

			if (claimsIdentity == null)
				throw new ArgumentException("identity IS NOT ClaimsIdentity", nameof(identity));

			claimsIdentity.AddClaim(claim);
		}

		public static bool HasClaim(IIdentity identity, string claimType)
		{
			if (identity == null)
				throw new ArgumentNullException(nameof(identity));

			var claimsIdentity = identity as ClaimsIdentity;
			return claimsIdentity != null && claimsIdentity.HasClaim(c => c.Type?.Equals(claimType, StringComparison.OrdinalIgnoreCase) ?? false);
		}

		public static bool HasClaim(IIdentity identity, string claimType, string userName)
		{
			if (identity == null)
				throw new ArgumentNullException(nameof(identity));

			var claimsIdentity = identity as ClaimsIdentity;
			return claimsIdentity != null && claimsIdentity.HasClaim(claimType, userName);
		}
	}
}

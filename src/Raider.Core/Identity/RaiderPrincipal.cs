using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Claims;
using System.Security.Principal;

namespace Raider.Identity
{
	public class RaiderPrincipal : ClaimsPrincipal
	{
		public RaiderIdentity? IdentityBase => Identity as RaiderIdentity;

		public RaiderPrincipal()
			: base()
		{
		}

		public RaiderPrincipal(IEnumerable<ClaimsIdentity> identities)
			: base(identities)
		{
		}

		public RaiderPrincipal(BinaryReader reader)
			: base(reader)
		{
		}

		public RaiderPrincipal(IIdentity identity)
			: base(identity)
		{
		}

		public RaiderPrincipal(IPrincipal principal)
			: base(principal)
		{
		}

		public override bool IsInRole(string role)
		{
			return IdentityBase != null && IdentityBase.IsInRole(role);
		}

		public bool IsInRole(int role)
		{
			return IdentityBase != null && IdentityBase.IsInRole(role);
		}

		public bool IsInAllRoles(params string[] roles)
		{
			return IdentityBase != null && IdentityBase.IsInAllRoles(roles);
		}

		public bool IsInAllRoles(params int[] roles)
		{
			return IdentityBase != null && IdentityBase.IsInAllRoles(roles);
		}

		public bool IsInAnyRole(params string[] roles)
		{
			return IdentityBase != null && IdentityBase.IsInAnyRole(roles);
		}

		public bool IsInAnyRole(params int[] roles)
		{
			return IdentityBase != null && IdentityBase.IsInAnyRole(roles);
		}

		public bool HasActivity(string activity)
		{
			return IdentityBase != null && IdentityBase.HasActivity(activity);
		}

		public bool HasAllActivities(params string[] activities)
		{
			return IdentityBase != null && IdentityBase.HasAllActivities(activities);
		}

		public bool HasAnyActivity(params string[] activities)
		{
			return IdentityBase != null && IdentityBase.HasAnyActivity(activities);
		}

		public bool HasActivityClaim(string activity)
		{
			return IdentityBase != null && IdentityBase.HasActivityClaim(activity);
		}

		public bool HasAllActivityClaims(params string[] activities)
		{
			return IdentityBase != null && IdentityBase.HasAllActivityClaims(activities);
		}

		public bool HasAnyActivityClaim(params string[] activities)
		{
			return IdentityBase != null && IdentityBase.HasAnyActivityClaim(activities);
		}

		public void AddClaim(Claim claim)
		{
			if (IdentityBase == null)
				throw new InvalidOperationException($"{nameof(IdentityBase)} == null");

			IdentityBase.AddClaim(claim);
		}

		public void AddClaims(IEnumerable<Claim> claims)
		{
			if (IdentityBase == null)
				throw new InvalidOperationException($"{nameof(IdentityBase)} == null");

			IdentityBase.AddClaims(claims);
		}

		public Claim? FindFirstClaim(string type, string value)
		{
			return IdentityBase?.FindFirstClaim(type, value);
		}

		public bool HasClaim(string type)
		{
			return IdentityBase != null && IdentityBase.HasClaim(type);
		}

		public IEnumerable<Claim?>? FindAllRaiderClaims(string type)
		{
			return IdentityBase?.FindAllRaiderClaims(type);
		}

		public virtual IEnumerable<Claim>? FindAllRaiderClaims(Predicate<Claim> match)
		{
			return IdentityBase?.FindAllRaiderClaims(match);
		}

		public virtual Claim? FindFirstRaiderClaim(Predicate<Claim> match)
		{
			return IdentityBase?.FindFirstRaiderClaim(match);
		}

		public virtual Claim? FindFirstRaiderClaim(string type)
		{
			return IdentityBase?.FindFirstRaiderClaim(type);
		}

		public virtual Claim? FindFirstRaiderClaim(string type, string value)
		{
			return IdentityBase?.FindFirstRaiderClaim(type, value);
		}

		public virtual bool HasRaiderClaim(string type)
		{
			return IdentityBase != null && IdentityBase.HasRaiderClaim(type);
		}

		public virtual bool HasRaiderClaim(string type, string value)
		{
			return IdentityBase != null && IdentityBase.HasRaiderClaim(type, value);
		}

		public virtual bool HasRaiderClaim(Predicate<Claim> match)
		{
			return IdentityBase != null && IdentityBase.HasRaiderClaim(match);
		}
	}

	public class RaiderPrincipal<TIdentity> : RaiderPrincipal
		where TIdentity : struct
	{
		public new RaiderIdentity<TIdentity>? IdentityBase => Identity as RaiderIdentity<TIdentity>;

		public RaiderPrincipal()
			: base()
		{
		}

		public RaiderPrincipal(IEnumerable<ClaimsIdentity> identities)
			: base(identities)
		{
		}

		public RaiderPrincipal(BinaryReader reader)
			: base(reader)
		{
		}

		public RaiderPrincipal(IIdentity identity)
			: base(identity)
		{
		}

		public RaiderPrincipal(IPrincipal principal)
			: base(principal)
		{
		}
	}
}

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

		public bool HasPermission(string permission)
		{
			return IdentityBase != null && IdentityBase.HasPermission(permission);
		}

		public bool HasPermission(int permission)
		{
			return IdentityBase != null && IdentityBase.HasPermission(permission);
		}

		public bool HasAllPermissions(params string[] permissions)
		{
			return IdentityBase != null && IdentityBase.HasAllPermissions(permissions);
		}

		public bool HasAllPermissions(params int[] permissions)
		{
			return IdentityBase != null && IdentityBase.HasAllPermissions(permissions);
		}

		public bool HasAnyPermission(params string[] permissions)
		{
			return IdentityBase != null && IdentityBase.HasAnyPermission(permissions);
		}

		public bool HasAnyPermission(params int[] permissions)
		{
			return IdentityBase != null && IdentityBase.HasAnyPermission(permissions);
		}

		public bool HasPermissionClaim(string permission)
		{
			return IdentityBase != null && IdentityBase.HasPermissionClaim(permission);
		}

		public bool HasAllPermissionClaims(params string[] permissions)
		{
			return IdentityBase != null && IdentityBase.HasAllPermissionClaims(permissions);
		}

		public bool HasAnyPermissionClaim(params string[] permissions)
		{
			return IdentityBase != null && IdentityBase.HasAnyPermissionClaim(permissions);
		}

		public bool HasPermissionClaim(int permission)
		{
			return IdentityBase != null && IdentityBase.HasPermissionClaim(permission);
		}

		public bool HasAllPermissionClaims(params int[] permissions)
		{
			return IdentityBase != null && IdentityBase.HasAllPermissionClaims(permissions);
		}

		public bool HasAnyPermissionClaim(params int[] permissions)
		{
			return IdentityBase != null && IdentityBase.HasAnyPermissionClaim(permissions);
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

		public static RaiderPrincipal<int>? Create(string authenticationSchemeType, AuthenticatedUser authenticatedUser)
		{
			if (string.IsNullOrWhiteSpace(authenticationSchemeType))
				throw new ArgumentNullException(nameof(authenticationSchemeType));

			if (authenticatedUser == null)
				throw new ArgumentNullException(nameof(authenticatedUser));

			var claimsIdentity = new ClaimsIdentity(authenticationSchemeType);
			claimsIdentity.AddClaim(new Claim(ClaimTypes.Name, authenticatedUser.Login));
			return Create(claimsIdentity, authenticatedUser, true, true);
		}

		public static RaiderPrincipal<int>? Create(
			IIdentity? identity,
			AuthenticatedUser? authenticatedUser,
			bool rolesToClams,
			bool permissionsToClaims)
		{
			if (identity == null || authenticatedUser == null)
				return null;

			var raiderIdentity = new RaiderIdentity<int>(
				identity,
				authenticatedUser.UserId,
				authenticatedUser.Login,
				authenticatedUser.DisplayName,
				authenticatedUser.UserData,
				authenticatedUser.Roles,
				authenticatedUser.RoleIds,
				authenticatedUser.Permissions,
				authenticatedUser.PermissionIds,
				rolesToClams,
				permissionsToClaims);

			var raiderPrincipal = new RaiderPrincipal<int>(raiderIdentity);
			return raiderPrincipal;
		}
	}
}

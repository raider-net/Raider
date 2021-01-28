﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;

namespace Raider.Identity
{
	public class RaiderIdentity : ClaimsIdentity
	{
		public const string ISSUER = "https://claims.raider.sk";
		public const string LOGIN_CLAIM_NAME = "login";
		public const string DISPLAYNAME_CLAIM_NAME = "displayName";
		public const string USER_ID_CLAIM_NAME = "userId";
		public const string ROLE_CLAIM_NAME = "role";
		public const string ROLE_ID_CLAIM_NAME = "roleId";
		public const string ACTIVITY_CLAIM_NAME = "activity";

		private readonly IReadOnlyCollection<string> EMPTY_ROLES = new List<string>();
		private readonly IReadOnlyCollection<int> EMPTY_ROLE_IDS = new List<int>();
		private readonly IReadOnlyCollection<string> EMPTY_ACTIVITIES = new List<string>();

		public string UserId { get; }

		public string Login { get; }

		public string DisplayName { get; }

		public object? UserData { get; }

		public IReadOnlyCollection<string> Roles { get; }

		public IReadOnlyCollection<int> RoleIds { get; }

		public IReadOnlyCollection<string> Activities { get; }

		//public WindowsIdentity WindowsIdentity { get; }

		//public RaiderIdentity(string name)
		//	: base(new GenericIdentity(name))
		//{
		//	Roles = EMPTY_ROLES;
		//	Activities = EMPTY_ACTIVITIES;
		//}

		//public RaiderIdentity(string name, string authenticationType)
		//	: base(new GenericIdentity(name, authenticationType))
		//{
		//	Roles = EMPTY_ROLES;
		//	Activities = EMPTY_ACTIVITIES;
		//}

		public RaiderIdentity(
			string name,
			string authenticationType,
			string userId,
			string login,
			string displayName,
			object? userData,
			List<string> roles,
			List<int> roleIds,
			List<string> activities,
			bool rolesToClams,
			bool activitiesToClaims)
			: base(new GenericIdentity(
				string.IsNullOrWhiteSpace(name)
					? throw new ArgumentNullException(name)
					: name,
				string.IsNullOrWhiteSpace(authenticationType)
					? throw new ArgumentNullException(authenticationType)
					: authenticationType))
		{
			UserId = string.IsNullOrWhiteSpace(userId)
				? throw new ArgumentNullException(userId)
				: userId;
			Login = string.IsNullOrWhiteSpace(login)
				? throw new ArgumentNullException(login)
				: login;
			DisplayName = string.IsNullOrWhiteSpace(displayName)
				? throw new ArgumentNullException(displayName)
				: displayName;
			UserData = userData;
			Roles = roles?.AsReadOnly() ?? EMPTY_ROLES;
			RoleIds = roleIds?.AsReadOnly() ?? EMPTY_ROLE_IDS;
			Activities = activities?.AsReadOnly() ?? EMPTY_ACTIVITIES;
			AddImplicitClaims(rolesToClams, activitiesToClaims);
		}

		public RaiderIdentity(
			IIdentity identity,
			string userId,
			string login,
			string displayName,
			object? userData,
			List<string> roles,
			List<int> roleIds,
			List<string> activities,
			bool rolesToClams,
			bool activitiesToClaims)
			: base(identity, (identity as ClaimsIdentity)?.Claims)
		{
			//if (identity is WindowsIdentity winIdentity)
			//	WindowsIdentity = winIdentity;

			UserId = string.IsNullOrWhiteSpace(userId)
				? throw new ArgumentNullException(userId)
				: userId;
			Login = string.IsNullOrWhiteSpace(login)
				? throw new ArgumentNullException(login)
				: login;
			DisplayName = string.IsNullOrWhiteSpace(displayName)
				? throw new ArgumentNullException(displayName)
				: displayName;
			UserData = userData;
			Roles = roles?.AsReadOnly() ?? EMPTY_ROLES;
			RoleIds = roleIds?.AsReadOnly() ?? EMPTY_ROLE_IDS;
			Activities = activities?.AsReadOnly() ?? EMPTY_ACTIVITIES;
			AddImplicitClaims(rolesToClams, activitiesToClaims);
		}

		private void AddImplicitClaims(bool rolesToClams, bool activitiesToClaims)
		{
			AddClaim(new Claim(LOGIN_CLAIM_NAME, Login));
			AddClaim(new Claim(DISPLAYNAME_CLAIM_NAME, DisplayName));
			AddClaim(new Claim(USER_ID_CLAIM_NAME, UserId));

			if (rolesToClams)
			{
				AddClaims(ROLE_CLAIM_NAME, Roles);
				AddClaims(ROLE_ID_CLAIM_NAME, RoleIds);
			}
			if (activitiesToClaims)
				AddClaims(ACTIVITY_CLAIM_NAME, Activities);
		}

		public bool IsInRole(string role)
		{
			if (string.IsNullOrWhiteSpace(role))
				throw new ArgumentNullException(nameof(role));

			return Roles.Contains(role, StringComparer.InvariantCultureIgnoreCase);
		}

		public bool IsInRole(int role)
		{
			return RoleIds.Contains(role);
		}

		public bool IsInAllRoles(params string[] roles)
		{
			if (roles == null)
				throw new ArgumentNullException(nameof(roles));

			return roles.All(r => Roles.Contains(r, StringComparer.InvariantCultureIgnoreCase));
		}

		public bool IsInAllRoles(params int[] roles)
		{
			if (roles == null)
				throw new ArgumentNullException(nameof(roles));

			return roles.All(r => RoleIds.Contains(r));
		}

		public bool IsInAnyRole(params string[] roles)
		{
			if (roles == null)
				throw new ArgumentNullException(nameof(roles));

			return roles.Any(r => Roles.Contains(r, StringComparer.InvariantCultureIgnoreCase));
		}

		public bool IsInAnyRole(params int[] roles)
		{
			if (roles == null)
				throw new ArgumentNullException(nameof(roles));

			return roles.Any(r => RoleIds.Contains(r));
		}

		public bool HasActivity(string activity)
		{
			if (string.IsNullOrWhiteSpace(activity))
				throw new ArgumentNullException(nameof(activity));

			return Activities.Contains(activity, StringComparer.InvariantCultureIgnoreCase);
		}

		public bool HasAllActivities(params string[] activities)
		{
			if (activities == null)
				throw new ArgumentNullException(nameof(activities));

			return activities.All(r => Activities.Contains(r, StringComparer.InvariantCultureIgnoreCase));
		}

		public bool HasAnyActivity(params string[] activities)
		{
			if (activities == null)
				throw new ArgumentNullException(nameof(activities));

			return activities.Any(r => Activities.Contains(r, StringComparer.InvariantCultureIgnoreCase));
		}

		public bool HasActivityClaim(string activity)
		{
			if (string.IsNullOrWhiteSpace(activity))
				throw new ArgumentNullException(nameof(activity));

			return HasRaiderClaim(ACTIVITY_CLAIM_NAME, activity);
		}

		public bool HasAllActivityClaims(params string[] activities)
		{
			if (activities == null)
				throw new ArgumentNullException(nameof(activities));

			return activities.All(activity => HasRaiderClaim(ACTIVITY_CLAIM_NAME, activity));
		}

		public bool HasAnyActivityClaim(params string[] activities)
		{
			if (activities == null)
				throw new ArgumentNullException(nameof(activities));

			return activities.Any(activity => HasRaiderClaim(ACTIVITY_CLAIM_NAME, activity));
		}

		public override void AddClaim(Claim claim)
		{
			if (claim == null)
				throw new ArgumentNullException(nameof(claim));

			base.AddClaim(new Claim(
								claim.Type,
								claim.Value,
								claim.ValueType,
								ISSUER,
								ISSUER,
								claim.Subject));
		}

		public override void AddClaims(IEnumerable<Claim?> claims)
		{
			if (claims == null)
				throw new ArgumentNullException(nameof(claims));

#pragma warning disable CS8602 // Dereference of a possibly null reference.
			base.AddClaims(claims
				.Where(claim => claim != null)
				.Select(claim => new Claim(
										claim.Type,
										claim.Value,
										claim.ValueType,
										ISSUER,
										ISSUER,
										claim.Subject)));
#pragma warning restore CS8602 // Dereference of a possibly null reference.
		}

		protected void AddClaims(string type, IEnumerable<string> values)
		{
			if (string.IsNullOrWhiteSpace(type))
				throw new ArgumentNullException(nameof(type));
			if (values == null)
				throw new ArgumentNullException(nameof(values));

			base.AddClaims(
				values
					.Select(v => new Claim(type, v, null, ISSUER)));
		}

		protected void AddClaims(string type, IEnumerable<int> values)
		{
			if (string.IsNullOrWhiteSpace(type))
				throw new ArgumentNullException(nameof(type));
			if (values == null)
				throw new ArgumentNullException(nameof(values));

			base.AddClaims(
				values
					.Select(v => new Claim(type, v.ToString(), null, ISSUER)));
		}

		public Claim? FindFirstClaim(string type, string value)
		{
			if (type == null)
				throw new ArgumentNullException(nameof(type));

			if (string.IsNullOrWhiteSpace(type) || string.IsNullOrWhiteSpace(value))
				return null;

			foreach (Claim claim in Claims)
				if (claim != null
						&& string.Equals(claim.Type, type, StringComparison.OrdinalIgnoreCase)
						&& string.Equals(claim.Value, value, StringComparison.Ordinal))
					return claim;

			return null;
		}

		public bool HasClaim(string type)
		{
			if (type == null)
				throw new ArgumentNullException(nameof(type));

			if (string.IsNullOrWhiteSpace(type))
				return false;

			foreach (Claim claim in Claims)
				if (claim != null
						&& string.Equals(claim.Type, type, StringComparison.OrdinalIgnoreCase))
					return true;

			return false;
		}

		public static bool IsRaiderClaim(Claim claim)
		{
			if (claim == null)
				throw new ArgumentNullException(nameof(claim));

			return
				   string.Equals(claim.Issuer, ISSUER, StringComparison.OrdinalIgnoreCase)
				&& string.Equals(claim.OriginalIssuer, ISSUER, StringComparison.OrdinalIgnoreCase);
		}

		public IEnumerable<Claim?> FindAllRaiderClaims(string type)
		{
			if (type == null)
				throw new ArgumentNullException(nameof(type));

			if (string.IsNullOrWhiteSpace(type))
				yield return null;
			else
				foreach (Claim claim in Claims)
					if (claim != null)
						if (string.Equals(claim.Type, type, StringComparison.OrdinalIgnoreCase) && IsRaiderClaim(claim))
							yield return claim;
		}

		public IEnumerable<Claim> FindAllRaiderClaims(Predicate<Claim> match)
		{
			if (match == null)
				throw new ArgumentNullException(nameof(match));

			foreach (Claim claim in Claims)
				if (match(claim) && IsRaiderClaim(claim))
					yield return claim;
		}

		public Claim? FindFirstRaiderClaim(Predicate<Claim> match)
		{
			if (match == null)
				throw new ArgumentNullException(nameof(match));

			foreach (Claim claim in Claims)
				if (match(claim) && IsRaiderClaim(claim))
					return claim;

			return null;
		}

		public Claim? FindFirstRaiderClaim(string type)
		{
			if (type == null)
				throw new ArgumentNullException(nameof(type));

			if (string.IsNullOrWhiteSpace(type))
				return null;

			foreach (Claim claim in Claims)
				if (claim != null
						&& string.Equals(claim.Type, type, StringComparison.OrdinalIgnoreCase)
						&& IsRaiderClaim(claim))
					return claim;

			return null;
		}

		public Claim? FindFirstRaiderClaim(string type, string value)
		{
			if (type == null)
				throw new ArgumentNullException(nameof(type));

			if (string.IsNullOrWhiteSpace(type) || string.IsNullOrWhiteSpace(value))
				return null;

			foreach (Claim claim in Claims)
				if (claim != null
						&& string.Equals(claim.Type, type, StringComparison.OrdinalIgnoreCase)
						&& string.Equals(claim.Value, value, StringComparison.Ordinal)
						&& IsRaiderClaim(claim))
					return claim;

			return null;
		}

		public bool HasRaiderClaim(string type)
		{
			if (type == null)
				throw new ArgumentNullException(nameof(type));

			if (string.IsNullOrWhiteSpace(type))
				return false;

			foreach (Claim claim in Claims)
				if (claim != null
						&& string.Equals(claim.Type, type, StringComparison.OrdinalIgnoreCase)
						&& IsRaiderClaim(claim))
					return true;

			return false;
		}

		public bool HasRaiderClaim(string type, string value)
		{
			if (type == null)
				throw new ArgumentNullException(nameof(type));

			if (value == null)
				throw new ArgumentNullException(nameof(value));

			if (string.IsNullOrWhiteSpace(type) || string.IsNullOrWhiteSpace(value))
				return false;

			foreach (Claim claim in Claims)
				if (claim != null
						&& string.Equals(claim.Type, type, StringComparison.OrdinalIgnoreCase)
						&& string.Equals(claim.Value, value, StringComparison.Ordinal)
						&& IsRaiderClaim(claim))
					return true;

			return false;
		}

		public bool HasRaiderClaim(Predicate<Claim> match)
		{
			if (match == null)
				throw new ArgumentNullException(nameof(match));

			foreach (Claim claim in Claims)
				if (match(claim) && IsRaiderClaim(claim))
					return true;

			return false;
		}
	}

	public class RaiderIdentity<TIdentity> : RaiderIdentity, IIdentity
		where TIdentity : struct
	{
		public new TIdentity UserId { get; }

		//public RaiderIdentity(string name)
		//    : base(name)
		//{
		//}

		//public RaiderIdentity(string name, string authenticationType)
		//    : base(name, authenticationType)
		//{
		//}

		public RaiderIdentity(
			string name,
			string authenticationType,
			TIdentity userId,
			string login,
			string displayName,
			object? userData,
			List<string> roles,
			List<int> roleIds,
			List<string> activities,
			bool rolesToClams,
			bool activitiesToClaims)
			: base(
				  name,
				  authenticationType,
#pragma warning disable CS8604 // Possible null reference argument.
				  userId.ToString(),
#pragma warning restore CS8604 // Possible null reference argument.
				  login,
				  displayName,
				  userData,
				  roles,
				  roleIds,
				  activities,
				  rolesToClams,
				  activitiesToClaims)
		{
			this.UserId = userId;
		}

		public RaiderIdentity(
			IIdentity identity,
			TIdentity userId,
			string login,
			string displayName,
			object? userData,
			List<string> roles,
			List<int> roleIds,
			List<string> activities,
			bool rolesToClams,
			bool activitiesToClaims)
			: base(
				  identity,
#pragma warning disable CS8604 // Possible null reference argument.
				  userId.ToString(),
#pragma warning restore CS8604 // Possible null reference argument.
				  login,
				  displayName,
				  userData,
				  roles,
				  roleIds,
				  activities,
				  rolesToClams,
				  activitiesToClaims)
		{
			this.UserId = userId;
		}
	}
}

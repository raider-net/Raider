﻿using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Raider.AspNetCore.Identity;
using Raider.Extensions;
using Raider.Identity;
using System;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;

namespace Raider.AspNetCore.Authentication
{
	public static class AuthenticationService
	{
		private static ILogger GetLogger(HttpContext context)
		{
			var loggerFactory = context.RequestServices.GetRequiredService<ILoggerFactory>();
			var logger = loggerFactory.CreateLogger(typeof(AuthenticationService).FullName);
			return logger;
		}

		public static async Task<RaiderPrincipal?> CreateFromWindowsIdentityAsync(HttpContext context, string authenticationSchemeType, ILogger? logger, bool allowStaticLogin)
		{
			if (!OperatingSystem.IsWindows())
				return null;

			if (context == null)
				throw new ArgumentNullException(nameof(context));

			if (string.IsNullOrWhiteSpace(authenticationSchemeType))
				throw new ArgumentNullException(nameof(authenticationSchemeType));

			if (logger == null)
				logger = GetLogger(context);

			WindowsPrincipal? windowsPrincipal = context.User as WindowsPrincipal;

			IIdentity? identity = null;
			string? logonWithoutDomain;
			string? windowsIdentityName = null;
			UserRoleActivities? user = null;

			if (windowsPrincipal?.Identity is WindowsIdentity windowsIdentity)
			{
				logonWithoutDomain = windowsIdentity.GetLogonNameWithoutDomain().ToLower();
				windowsIdentityName = windowsIdentity.Name.ToLower();

				var authenticationManager = context.RequestServices.GetRequiredService<IAuthenticationManager>();
				user = await authenticationManager.CreateFromWindowsIdentityAsync(logonWithoutDomain, windowsIdentityName);
			}

			if (user == null)
				return await CreateStaticAsync(context, authenticationSchemeType, logger, allowStaticLogin);

			if (user != null && !string.IsNullOrWhiteSpace(windowsIdentityName))
			{
				var claimsIdentity = new ClaimsIdentity(authenticationSchemeType);
				claimsIdentity.AddClaim(new Claim(ClaimTypes.Name, windowsIdentityName));
				identity = claimsIdentity;
			}

			return CreateRaiderPrincipal(identity, user, true, true);
		}

		public static async Task<RaiderPrincipal?> CreateFromRequestAsync(HttpContext context, string authenticationSchemeType, ILogger? logger)
		{
			if (context == null)
				throw new ArgumentNullException(nameof(context));

			if (string.IsNullOrWhiteSpace(authenticationSchemeType))
				throw new ArgumentNullException(nameof(authenticationSchemeType));

			if (logger == null)
				logger = GetLogger(context);

			var claimsIdentity = new ClaimsIdentity(authenticationSchemeType);
			claimsIdentity.AddClaim(new Claim(ClaimTypes.Name, ""));
			var identity = claimsIdentity;

			var authenticationManager = context.RequestServices.GetRequiredService<IAuthenticationManager>();
			var user = await authenticationManager.CreateFromRequestAsync(context.Request.Headers.ToDictionary(x => x.Key, x => (string[])x.Value));
			return CreateRaiderPrincipal(identity, user, true, true);
		}

		public static async Task<RaiderPrincipal?> CreateStaticAsync(HttpContext context, string authenticationSchemeType, ILogger? logger, bool allowStaticLogin)
		{
			if (!allowStaticLogin)
				return null;

			if (context == null)
				throw new ArgumentNullException(nameof(context));

			if (string.IsNullOrWhiteSpace(authenticationSchemeType))
				throw new ArgumentNullException(nameof(authenticationSchemeType));

			if (logger == null)
				logger = GetLogger(context);

			IIdentity? identity = null;
			UserRoleActivities? user = null;

			var authenticationManager = context.RequestServices.GetRequiredService<IAuthenticationManager>();
			if (authenticationManager.StaticUserId.HasValue)
				user = await authenticationManager.CreateFromUserIdAsync(authenticationManager.StaticUserId);

			if (user != null && !string.IsNullOrWhiteSpace(user.Login))
			{
				var claimsIdentity = new ClaimsIdentity(authenticationSchemeType);
				claimsIdentity.AddClaim(new Claim(ClaimTypes.Name, user.Login));
				identity = claimsIdentity;
			}

			return CreateRaiderPrincipal(identity, user, true, true);
		}

		public static async Task<RaiderPrincipal?> RecreateCookieIdentityAsync(HttpContext context, string? userName, string authenticationSchemeType, ILogger? logger)
		{
			if (string.IsNullOrWhiteSpace(userName))
				return null;

			if (context == null)
				throw new ArgumentNullException(nameof(context));

			if (string.IsNullOrWhiteSpace(authenticationSchemeType))
				throw new ArgumentNullException(nameof(authenticationSchemeType));

			if (logger == null)
				logger = GetLogger(context);

			var authenticationManager = context.RequestServices.GetRequiredService<IAuthenticationManager>();
			var user = await authenticationManager.CreateFromLoginAsync(userName.ToLower());
			var claimsIdentity = new ClaimsIdentity(authenticationSchemeType);
			claimsIdentity.AddClaim(new Claim(ClaimTypes.Name, userName));
			return CreateRaiderPrincipal(claimsIdentity, user, true, true);
		}

		public static async Task<RaiderPrincipal?> RenewTokenIdentityAsync(HttpContext context, ClaimsPrincipal? principal, ILogger? logger)
		{
			if (principal == null)
				return null;

			if (context == null)
				throw new ArgumentNullException(nameof(context));

			if (logger == null)
				logger = GetLogger(context);

			var userIdClaim = principal
							.Claims
							.FirstOrDefault(c => RaiderIdentity.IsRaiderClaim(c)
												&& string.Equals(c.Type, RaiderIdentity.USER_ID_CLAIM_NAME, StringComparison.OrdinalIgnoreCase));

			int userId = -1;
			if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out userId))
				return null;

			var loginClaim = principal
							.Claims
							.FirstOrDefault(c => RaiderIdentity.IsRaiderClaim(c)
												&& string.Equals(c.Type, RaiderIdentity.LOGIN_CLAIM_NAME, StringComparison.OrdinalIgnoreCase));
			if (loginClaim == null)
				return null;

			var displayNameClaim = principal
							.Claims
							.FirstOrDefault(c => RaiderIdentity.IsRaiderClaim(c)
												&& string.Equals(c.Type, RaiderIdentity.DISPLAYNAME_CLAIM_NAME, StringComparison.OrdinalIgnoreCase));
			if (displayNameClaim == null)
				return null;

			var roleClaims = principal
							.Claims
							.Where(c => RaiderIdentity.IsRaiderClaim(c)
												&& string.Equals(c.Type, RaiderIdentity.ROLE_CLAIM_NAME, StringComparison.OrdinalIgnoreCase));

			var roleIdClaims = principal
							.Claims
							.Where(c => RaiderIdentity.IsRaiderClaim(c)
												&& string.Equals(c.Type, RaiderIdentity.ROLE_ID_CLAIM_NAME, StringComparison.OrdinalIgnoreCase));

			var activityClaims = principal
							.Claims
							.Where(c => RaiderIdentity.IsRaiderClaim(c)
												&& string.Equals(c.Type, RaiderIdentity.ACTIVITY_CLAIM_NAME, StringComparison.OrdinalIgnoreCase));

			var user = new UserRoleActivities(userId, loginClaim.Value, displayNameClaim.Value)
			{
				UserData = null,
				Roles = roleClaims?.Select(c => c.Value).ToList(),
				RoleIds = roleIdClaims?.Select(c => int.Parse(c.Value)).ToList(),
				Activities = activityClaims?.Select(c => c.Value).ToList()
			};

			var authenticationManager = context.RequestServices.GetRequiredService<IAuthenticationManager>();
			user = await authenticationManager.SetUserDataAsync(user);
			return CreateRaiderPrincipal(principal.Identity, user, true, true);
		}

		public static async Task<RaiderPrincipal?> CreateIdentityAsync(HttpContext context, string? login, string? password, string authenticationSchemeType, ILogger? logger/*, out string? error, out string? passwordTemporaryUrlSlug*/)
		{
			//error = null;
			//passwordTemporaryUrlSlug = null;

			if (string.IsNullOrWhiteSpace(login))
				return null;

			if (context == null)
				throw new ArgumentNullException(nameof(context));

			if (string.IsNullOrWhiteSpace(authenticationSchemeType))
				throw new ArgumentNullException(nameof(authenticationSchemeType));

			if (logger == null)
				logger = GetLogger(context);

			var authenticationManager = context.RequestServices.GetRequiredService<IAuthenticationManager>();
			var user = await authenticationManager.CreateFromLoginPasswordAsync(login, password);
			if (user == null)
				return null;

			var success = PasswordHelper.VerifyHashedPassword(user.Password, user.Salt, password);

			if (success)
			{
				//error = user.Error;
				//passwordTemporaryUrlSlug = user.PasswordTemporaryUrlSlug;

				user = await authenticationManager.SetRolesAndActivities(user);
			}

			var claimsIdentity = new ClaimsIdentity(authenticationSchemeType);
			claimsIdentity.AddClaim(new Claim(ClaimTypes.Name, login));
			return CreateRaiderPrincipal(claimsIdentity, user, false, false);
		}

		#region withhout HttpContext

		//public static async Task<RaiderPrincipal?> CreateFromWindowsIdentityAsync(string authenticationSchemeType, ILogger? logger)
		//{
		//	if (authenticationManager == null)
		//		throw new InvalidOperationException("Not initialized");

		//	if (!OperatingSystem.IsWindows())
		//		return null;

		//	if (string.IsNullOrWhiteSpace(authenticationSchemeType))
		//		throw new ArgumentNullException(nameof(authenticationSchemeType));

		//	string? logonWithoutDomain;
		//	string? windowsIdentityName;
		//	IIdentity? identity = null;

		//	try
		//	{
		//		var windowsIdentity = WindowsIdentity.GetCurrent();
		//		if (windowsIdentity != null)
		//		{
		//			logonWithoutDomain = windowsIdentity.GetLogonNameWithoutDomain().ToLower();
		//			windowsIdentityName = windowsIdentity.Name.ToLower();
		//		}
		//		else
		//		{
		//			logonWithoutDomain = Environment.UserName?.ToLower();
		//			windowsIdentityName = Environment.UserDomainName?.ToLower() + "\\" + Environment.UserName?.ToLower();

		//			var claimsIdentity = new ClaimsIdentity(authenticationSchemeType);
		//			claimsIdentity.AddClaim(new Claim(ClaimTypes.Name, windowsIdentityName));
		//			identity = claimsIdentity;
		//		}
		//	}
		//	catch
		//	{
		//		logonWithoutDomain = Environment.UserName?.ToLower();
		//		windowsIdentityName = Environment.UserDomainName?.ToLower() + "\\" + Environment.UserName?.ToLower();

		//		var claimsIdentity = new ClaimsIdentity(authenticationSchemeType);
		//		claimsIdentity.AddClaim(new Claim(ClaimTypes.Name, windowsIdentityName));
		//		identity = claimsIdentity;
		//	}

		//	var user = await authenticationManager.CreateFromWindowsIdentityAsync(logonWithoutDomain, windowsIdentityName);
		//	return CreateRaiderPrincipal(identity, user, true, true);
		//}

		//public static async Task<RaiderPrincipal?> CreateFakeIdentityAsync(int? idUser, string authenticationSchemeType, ILogger? logger)
		//{
		//	if (authenticationManager == null)
		//		throw new InvalidOperationException("Not initialized");

		//	if (string.IsNullOrWhiteSpace(authenticationSchemeType))
		//		throw new ArgumentNullException(nameof(authenticationSchemeType));

		//	var user = await authenticationManager.CreateFromUserIdAsync(idUser);
		//	if (user == null)
		//		return null;

		//	var claimsIdentity = new ClaimsIdentity(authenticationSchemeType);
		//	claimsIdentity.AddClaim(new Claim(ClaimTypes.Name, user.Login));
		//	return CreateRaiderPrincipal(claimsIdentity, user, false, false);
		//}

		#endregion withhout HttpContext

		private static RaiderPrincipal? CreateRaiderPrincipal(
			IIdentity? identity,
			UserRoleActivities? userRoleActivities,
			bool rolesToClams,
			bool activitiesToClaims)
		{
			if (identity == null || userRoleActivities == null)
				return null;

			var RaiderIdentity = new RaiderIdentity<int>(
				identity,
				userRoleActivities.UserId,
				userRoleActivities.Login,
				userRoleActivities.DisplayName,
				userRoleActivities.UserData,
				userRoleActivities.Roles,
				userRoleActivities.RoleIds,
				userRoleActivities.Activities,
				rolesToClams,
				activitiesToClaims);

			var RaiderPrincipal = new RaiderPrincipal<int>(RaiderIdentity);
			return RaiderPrincipal;
		}
	}
}
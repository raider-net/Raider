using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Raider.AspNetCore.Identity;
using Raider.AspNetCore.Logging;
using Raider.Extensions;
using Raider.Identity;
using Raider.Logging.Extensions;
using Raider.Trace;
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
			AuthenticatedUser? user = null;

			IAuthenticationManager? authenticationManager = null;
			if (windowsPrincipal?.Identity is WindowsIdentity windowsIdentity)
			{
				logonWithoutDomain = windowsIdentity.GetLogonNameWithoutDomain().ToLower();
				windowsIdentityName = windowsIdentity.Name.ToLower();

				authenticationManager = context.RequestServices.GetRequiredService<IAuthenticationManager>();
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

			return CreateRaiderPrincipal(identity, user, true, true, logger, authenticationManager);
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
			return CreateRaiderPrincipal(identity, user, true, true, logger, authenticationManager);
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
			AuthenticatedUser? user = null;

			var authenticationManager = context.RequestServices.GetRequiredService<IAuthenticationManager>();
			if (authenticationManager.StaticUserId.HasValue)
				user = await authenticationManager.CreateFromUserIdAsync(authenticationManager.StaticUserId);

			if (user != null && !string.IsNullOrWhiteSpace(user.Login))
			{
				var claimsIdentity = new ClaimsIdentity(authenticationSchemeType);
				claimsIdentity.AddClaim(new Claim(ClaimTypes.Name, user.Login));
				identity = claimsIdentity;
			}

			return CreateRaiderPrincipal(identity, user, true, true, logger, authenticationManager);
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
			return CreateRaiderPrincipal(claimsIdentity, user, true, true, logger, authenticationManager);
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

			var premissionClaims = principal
							.Claims
							.Where(c => RaiderIdentity.IsRaiderClaim(c)
												&& string.Equals(c.Type, RaiderIdentity.PERMISSION_CLAIM_NAME, StringComparison.OrdinalIgnoreCase));

			var tc = context.RequestServices.GetRequiredService<TraceContext>();

			var user = new AuthenticatedUser(userId, loginClaim.Value, displayNameClaim.Value, tc.Next())
			{
				UserData = null,
				Roles = roleClaims?.Select(c => c.Value).ToList(),
				RoleIds = roleIdClaims?.Select(c => int.Parse(c.Value)).ToList(),
				Permissions = premissionClaims?.Select(c => c.Value).ToList()
			};

			var authenticationManager = context.RequestServices.GetRequiredService<IAuthenticationManager>();
			user = await authenticationManager.SetUserDataAsync(user);
			return CreateRaiderPrincipal(principal.Identity, user, true, true, logger, authenticationManager);
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

				user = await authenticationManager.SetRolesAndPremissions(user);
			}

			var claimsIdentity = new ClaimsIdentity(authenticationSchemeType);
			claimsIdentity.AddClaim(new Claim(ClaimTypes.Name, login));
			return CreateRaiderPrincipal(claimsIdentity, user, false, false, logger, authenticationManager);
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
			AuthenticatedUser? userRolePermissions,
			bool rolesToClams,
			bool permissionsToClaims,
			ILogger logger,
			IAuthenticationManager? authenticationManager)
		{
			if (identity == null || userRolePermissions == null)
				return null;

			var raiderIdentity = new RaiderIdentity<int>(
				identity,
				userRolePermissions.UserId,
				userRolePermissions.Login,
				userRolePermissions.DisplayName,
				userRolePermissions.UserData,
				userRolePermissions.Roles,
				userRolePermissions.RoleIds,
				userRolePermissions.Permissions,
				userRolePermissions.PermissionIds,
				rolesToClams,
				permissionsToClaims);

			if (authenticationManager?.LogRequestAuthentication ?? false)
			{
				AspNetLogWriter.Instance.WriteRequestAuthentication(new Logging.Dto.RequestAuthentication
				{
					CorrelationId = userRolePermissions.TraceInfo?.CorrelationId,
					ExternalCorrelationId = userRolePermissions.TraceInfo?.ExternalCorrelationId,
					IdUser = raiderIdentity.UserId,
					Roles = authenticationManager.LogRoles
					? ((0 < raiderIdentity.RoleIds?.Count)
						? System.Text.Json.JsonSerializer.Serialize(raiderIdentity.RoleIds)
						: (0 < raiderIdentity.Roles?.Count ? System.Text.Json.JsonSerializer.Serialize(raiderIdentity.Roles) : null))
					: null,
					Permissions = authenticationManager.LogPermissions
					? ((0 < raiderIdentity.PermissionIds?.Count)
						? System.Text.Json.JsonSerializer.Serialize(raiderIdentity.PermissionIds)
						: (0 < raiderIdentity.Permissions?.Count ? System.Text.Json.JsonSerializer.Serialize(raiderIdentity.Permissions) : null))
					: null
				});
			}

			var RaiderPrincipal = new RaiderPrincipal<int>(raiderIdentity);
			return RaiderPrincipal;
		}
	}
}

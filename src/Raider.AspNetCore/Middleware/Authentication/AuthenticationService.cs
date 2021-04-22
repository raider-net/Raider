using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Raider.AspNetCore.Identity;
using Raider.AspNetCore.Logging;
using Raider.Extensions;
using Raider.Identity;
using Raider.Trace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;

namespace Raider.AspNetCore.Authentication
{
	public static class AuthenticationService
	{
		private static Dictionary<string, string>? _cookieDataProtectionPurposes; //Dictionary<cookieName, purpose>
		private static bool _dataProtectorsCreated = false;
		private static Dictionary<string, IDataProtector>? _dataProtectors; //Dictionary<cookieName, IDataProtector>

		private static readonly object _initLock = new();
		private static bool _initialized = false;
		public static void Initialize(Dictionary<string, string>? cookieDataProtectionPurposes)
		{
			if (_initialized)
				throw new InvalidOperationException("Already initialized");

			lock (_initLock)
			{
				if (_initialized)
					throw new InvalidOperationException("Already initialized");

				_cookieDataProtectionPurposes = cookieDataProtectionPurposes;
				_initialized = true;
			}
		}

		public static IDataProtector? GetDataProtector(HttpContext context, string cookieName)
		{
			if (!_dataProtectorsCreated)
				GetDataProtectors(context);

			return GetDataProtector(cookieName);
		}

		public static IDataProtector? GetDataProtector(string cookieName)
		{
			if (_dataProtectors == null)
				return null;

			_dataProtectors.TryGetValue(cookieName, out IDataProtector? dataProtector);
			return dataProtector;
		}

		private static IReadOnlyDictionary<string, IDataProtector>? GetDataProtectors(HttpContext context)
		{
			if (context == null)
				return null;

			if (_dataProtectorsCreated)
				return _dataProtectors;

			if (_cookieDataProtectionPurposes == null)
				return null;

			_dataProtectors = new Dictionary<string, IDataProtector>();
			var dataProtectionProvider = context.RequestServices.GetRequiredService<IDataProtectionProvider>();

			foreach (var kvp in _cookieDataProtectionPurposes)
			{
				var dataProtector = dataProtectionProvider.CreateProtector(kvp.Value);
				_dataProtectors.Add(kvp.Key, dataProtector);
			}

			_dataProtectorsCreated = true;
			return _dataProtectors;
		}

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
				user = await authenticationManager.CreateFromWindowsIdentityAsync(logonWithoutDomain, windowsIdentityName, context.Request.ToRequestMetadata(cookieDataProtectionPurposes: GetDataProtectors(context)));
			}

			if (user == null)
				return await CreateStaticAsync(context, authenticationSchemeType, logger, allowStaticLogin);

			if (user != null && !string.IsNullOrWhiteSpace(windowsIdentityName))
			{
				var claimsIdentity = new ClaimsIdentity(authenticationSchemeType);
				claimsIdentity.AddClaim(new Claim(ClaimTypes.Name, windowsIdentityName));
				identity = claimsIdentity;
			}

			var applicationContext = context.RequestServices.GetService<IApplicationContext>();
			return CreateRaiderPrincipal(identity, user, true, true, logger, applicationContext, authenticationManager);
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
			var user = await authenticationManager.CreateFromRequestAsync(context.Request.ToRequestMetadata(cookieDataProtectionPurposes: GetDataProtectors(context)));
			var applicationContext = context.RequestServices.GetService<IApplicationContext>();
			return CreateRaiderPrincipal(identity, user, true, true, logger, applicationContext, authenticationManager);
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
				user = await authenticationManager.CreateFromUserIdAsync(authenticationManager.StaticUserId, context.Request.ToRequestMetadata(cookieDataProtectionPurposes: GetDataProtectors(context)));

			if (user != null && !string.IsNullOrWhiteSpace(user.Login))
			{
				var claimsIdentity = new ClaimsIdentity(authenticationSchemeType);
				claimsIdentity.AddClaim(new Claim(ClaimTypes.Name, user.Login));
				identity = claimsIdentity;
			}

			var applicationContext = context.RequestServices.GetService<IApplicationContext>();
			return CreateRaiderPrincipal(identity, user, true, true, logger, applicationContext, authenticationManager);
		}

		public static async Task<RaiderPrincipal?> RenewTokenIdentityAsync(HttpContext context, ClaimsPrincipal? principal, ILogger? logger)
		{
			if (principal == null)
				return null;

			if (context == null)
				throw new ArgumentNullException(nameof(context));

			if (logger == null)
				logger = GetLogger(context);

			var userIdClaim =
				principal
					.Claims
					.FirstOrDefault(c => RaiderIdentity.IsRaiderClaim(c)
						&& string.Equals(c.Type, RaiderIdentity.USER_ID_CLAIM_NAME, StringComparison.OrdinalIgnoreCase));

			int userId = -1;
			if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out userId))
				return null;

			var loginClaim =
				principal
					.Claims
					.FirstOrDefault(c => RaiderIdentity.IsRaiderClaim(c)
						&& string.Equals(c.Type, RaiderIdentity.LOGIN_CLAIM_NAME, StringComparison.OrdinalIgnoreCase));

			if (loginClaim == null)
				return null;

			var displayNameClaim =
				principal
					.Claims
					.FirstOrDefault(c => RaiderIdentity.IsRaiderClaim(c)
						&& string.Equals(c.Type, RaiderIdentity.DISPLAYNAME_CLAIM_NAME, StringComparison.OrdinalIgnoreCase));

			if (displayNameClaim == null)
				return null;

			var roleClaims =
				principal
					.Claims
					.Where(c => RaiderIdentity.IsRaiderClaim(c)
						&& string.Equals(c.Type, RaiderIdentity.ROLE_CLAIM_NAME, StringComparison.OrdinalIgnoreCase));

			var roleIdClaims =
				principal
					.Claims
					.Where(c => RaiderIdentity.IsRaiderClaim(c)
						&& string.Equals(c.Type, RaiderIdentity.ROLE_ID_CLAIM_NAME, StringComparison.OrdinalIgnoreCase));

			var premissionClaims =
				principal
					.Claims
					.Where(c => RaiderIdentity.IsRaiderClaim(c)
						&& string.Equals(c.Type, RaiderIdentity.PERMISSION_CLAIM_NAME, StringComparison.OrdinalIgnoreCase));

			var applicationContext = context.RequestServices.GetRequiredService<IApplicationContext>();

			var user = new AuthenticatedUser(userId, loginClaim.Value, displayNameClaim.Value, applicationContext.Next())
			{
				UserData = null,
				Roles = roleClaims?.Select(c => c.Value).ToList(),
				RoleIds = roleIdClaims?.Select(c => int.Parse(c.Value)).ToList(),
				Permissions = premissionClaims?.Select(c => c.Value).ToList()
			};

			var authenticationManager = context.RequestServices.GetRequiredService<IAuthenticationManager>();
			user = await authenticationManager.SetUserDataAsync(user, context.Request.ToRequestMetadata(cookieDataProtectionPurposes: GetDataProtectors(context)));
			
			return CreateRaiderPrincipal(principal.Identity, user, true, true, logger, applicationContext, authenticationManager);
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

				user = await authenticationManager.SetUserDataRolesPremissions(user, context.Request.ToRequestMetadata(cookieDataProtectionPurposes: GetDataProtectors(context)));
			}
			else
			{
				user.UserData = null;
			}

			var claimsIdentity = new ClaimsIdentity(authenticationSchemeType);
			claimsIdentity.AddClaim(new Claim(ClaimTypes.Name, login));
			var applicationContext = context.RequestServices.GetService<IApplicationContext>();
			return CreateRaiderPrincipal(claimsIdentity, user, false, false, logger, applicationContext, authenticationManager);
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
			var user = await authenticationManager.CreateFromLoginAsync(userName.ToLower(), context.Request.ToRequestMetadata(cookieDataProtectionPurposes: GetDataProtectors(context)));
			var claimsIdentity = new ClaimsIdentity(authenticationSchemeType);
			claimsIdentity.AddClaim(new Claim(ClaimTypes.Name, userName));
			var applicationContext = context.RequestServices.GetService<IApplicationContext>();
			return CreateRaiderPrincipal(claimsIdentity, user, true, true, logger, applicationContext, authenticationManager);
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

		private static RaiderPrincipal<int>? CreateRaiderPrincipal(
			IIdentity? identity,
			AuthenticatedUser? authenticatedUser,
			bool rolesToClams,
			bool permissionsToClaims,
			ILogger logger,
			IApplicationContext? applicationContext,
			IAuthenticationManager? authenticationManager)
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

			if (authenticationManager?.LogRequestAuthentication ?? false)
			{
				AspNetLogWriter.Instance.WriteRequestAuthentication(new Logging.Dto.RequestAuthentication
				{
					CorrelationId = authenticatedUser.TraceInfo.CorrelationId,
					ExternalCorrelationId = authenticatedUser.TraceInfo.ExternalCorrelationId,
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

			var raiderPrincipal = new RaiderPrincipal<int>(raiderIdentity);

			if (applicationContext != null)
				applicationContext.AddTraceFrame(TraceInfo.Create(authenticatedUser.TraceInfo).TraceFrame, raiderPrincipal);

			return raiderPrincipal;
		}
	}
}

using Raider.Identity;
using Raider.AspNetCore.Middleware.Authentication.Events;
using Raider.AspNetCore.Middleware.Authentication.RequestAuth.Events;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using static Raider.AspNetCore.Middleware.Authentication.AuthenticationOptions;
using Microsoft.Extensions.DependencyInjection;
using Raider.Trace;
using Raider.Logging.Extensions;
using Raider.Extensions;
using Raider.AspNetCore.Identity;
using Raider.AspNetCore.Middleware.Authentication.RequestAuth;

namespace Raider.AspNetCore.Middleware.Authentication
{
	/* USAGE
	 
		Startup.cs:
		public IServiceProvider ConfigureServices(IServiceCollection services)
		
			services.AddRaiderAuthentication(options =>
				options
					.SetWindowsAuthentication(UserService.CreateFromWindowsIdentity)
					.SetCookieAuthenticationReplacePrincipal(
						new Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationOptions
						{
							AccessDeniedPath = "/Account/AccessDenied",
							ExpireTimeSpan = TimeSpan.FromDays(14),
							LoginPath = "/Account/Login",
							LogoutPath = "/Account/Logout",
							ReturnUrlParameter = "ReturnUrl"
						},
						UserService.RecreateCookieIdentity)
					.SetTokenAuthenticationConvertPrincipal(
						new TokenValidationParameters()
						{
							ValidateIssuer = true,
							ValidateActor = true,
							ValidateAudience = true,
							ValidateLifetime = true,
							ValidateIssuerSigningKey = true,
							ValidIssuer = RaiderIdentity.ISSUER,
							ValidAudience = "Audience",
							IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("SecretSigningKey"))
						},
						UserService.RenewTokenIdentity)
					.SetAuthenticationFlowFallback(AuthenticationType.Cookie)
				);
	*/

	public class RaiderAuthenticationHandler :
		AuthenticationHandler<AuthenticationOptions>,
		IAuthenticationSignInHandler,
		IAuthenticationSignOutHandler
	{
		private readonly ILogger _logger;

		private const string HeaderValueNoCache = "no-cache";
		private const string HeaderValueMinusOne = "-1";
		private const string SessionIdClaim = "Microsoft.AspNetCore.Authentication.Cookies-SessionId";

		private bool _shouldRefresh;
		private bool _signInCalled;
		private bool _signOutCalled;

		private DateTimeOffset? _refreshIssuedUtc;
		private DateTimeOffset? _refreshExpiresUtc;
		private string _sessionKey;
		private Task<AuthenticateResult> _readCookieTask;

		private OpenIdConnectConfiguration _configuration;

		public RaiderAuthenticationHandler(
			IOptionsMonitor<AuthenticationOptions> options,
			ILoggerFactory logger,
			UrlEncoder encoder,
			ISystemClock clock)
			: base(options, logger, encoder, clock)
		{
			_logger = logger?.CreateLogger<RaiderAuthenticationHandler>() ?? throw new ArgumentNullException(nameof(logger));
		}

		/// <summary>
		/// The handler calls methods on the events which give the application control at certain points where processing is occurring. 
		/// If it is not provided a default instance is supplied which does nothing when the methods are called.
		/// </summary>
		protected new AuthenticationEvents Events
		{
			get { return (AuthenticationEvents)base.Events; }
			set { base.Events = value; }
		}

		protected override Task<object> CreateEventsAsync() => Task.FromResult<object>(new AuthenticationEvents(Context, Options));

		protected override Task InitializeHandlerAsync()
		{
			if (Options.UseCookieAuthentication)
			{
				// Cookies needs to finish the response
				Context.Response.OnStarting(FinishResponseAsync);
				return Task.CompletedTask;
			}
			else
			{
				return base.InitializeHandlerAsync();
			}
		}

		#region Cookie

		private Task<AuthenticateResult> EnsureCookieTicket()
		{
			// We only need to read the ticket once
			if (_readCookieTask == null)
			{
				_readCookieTask = ReadCookieTicket();
			}
			return _readCookieTask;
		}

		private void CheckForRefresh(AuthenticationTicket ticket)
		{
			var currentUtc = Clock.UtcNow;
			var issuedUtc = ticket.Properties.IssuedUtc;
			var expiresUtc = ticket.Properties.ExpiresUtc;
			var allowRefresh = ticket.Properties.AllowRefresh ?? true;
			if (issuedUtc != null && expiresUtc != null && Options.CookieAuthenticationOptions.SlidingExpiration && allowRefresh)
			{
				var timeElapsed = currentUtc.Subtract(issuedUtc.Value);
				var timeRemaining = expiresUtc.Value.Subtract(currentUtc);

				//re-issue a new cookie with a new expiration time any time it processes a request which is more than halfway through the expiration window
				if (timeRemaining < timeElapsed)
				{
					RequestRefresh(ticket);
				}
			}
		}

		private void RequestRefresh(AuthenticationTicket ticket)
		{
			var issuedUtc = ticket.Properties.IssuedUtc;
			var expiresUtc = ticket.Properties.ExpiresUtc;

			if (issuedUtc != null && expiresUtc != null)
			{
				_shouldRefresh = true;
				var currentUtc = Clock.UtcNow;
				_refreshIssuedUtc = currentUtc;
				var timeSpan = expiresUtc.Value.Subtract(issuedUtc.Value);
				_refreshExpiresUtc = currentUtc.Add(timeSpan);
			}
		}

		private async Task<AuthenticateResult> ReadCookieTicket()
		{
			var cookie = Options.CookieAuthenticationOptions.CookieManager.GetRequestCookie(Context, Options.CookieAuthenticationOptions.Cookie.Name);
			if (string.IsNullOrEmpty(cookie))
				return AuthenticateResult.NoResult();

			var ticket = Options.CookieAuthenticationOptions.TicketDataFormat.Unprotect(cookie, GetTlsTokenBinding());
			if (ticket == null)
			{
				return AuthenticateResult.Fail("Unprotect ticket failed");
			}

			if (Options.CookieAuthenticationOptions.SessionStore != null)
			{
				var claim = ticket.Principal.Claims.FirstOrDefault(c => c.Type.Equals(SessionIdClaim));
				if (claim == null)
				{
					return AuthenticateResult.Fail("SessionId missing");
				}
				_sessionKey = claim.Value;
				ticket = await Options.CookieAuthenticationOptions.SessionStore.RetrieveAsync(_sessionKey);
				if (ticket == null)
				{
					return AuthenticateResult.Fail("Identity missing in session store");
				}
			}

			var currentUtc = Clock.UtcNow;
			var issuedUtc = ticket.Properties.IssuedUtc;
			var expiresUtc = ticket.Properties.ExpiresUtc;

			if (expiresUtc != null && expiresUtc.Value < currentUtc)
			{
				if (Options.CookieAuthenticationOptions.SessionStore != null)
				{
					await Options.CookieAuthenticationOptions.SessionStore.RemoveAsync(_sessionKey);
				}
				return AuthenticateResult.Fail("Ticket expired");
			}

			var cookieStore = Context.RequestServices.GetService<ICookieStore>();
			if (cookieStore != null)
			{
				var existsInStore = await cookieStore.ExistsAsync(Context, cookie);
				if (!existsInStore)
					return AuthenticateResult.NoResult();
			}

			CheckForRefresh(ticket);

			// Finally we have a valid ticket
			return AuthenticateResult.Success(ticket);
		}

		private CookieOptions BuildCookieOptions()
		{
			var cookieOptions = Options.CookieAuthenticationOptions.Cookie.Build(Context);
			// ignore the 'Expires' value as this will be computed elsewhere
			cookieOptions.Expires = null;

			return cookieOptions;
		}

		protected virtual async Task FinishResponseAsync()
		{
			// Only renew if requested, and neither sign in or sign out was called
			if (!_shouldRefresh || _signInCalled || _signOutCalled)
			{
				return;
			}

			var ticket = (await HandleAuthenticateOnceSafeAsync())?.Ticket;
			if (ticket != null)
			{
				var properties = ticket.Properties;

				if (_refreshIssuedUtc.HasValue)
				{
					properties.IssuedUtc = _refreshIssuedUtc;
				}

				if (_refreshExpiresUtc.HasValue)
				{
					properties.ExpiresUtc = _refreshExpiresUtc;
				}

				if (Options.CookieAuthenticationOptions.SessionStore != null && _sessionKey != null)
				{
					await Options.CookieAuthenticationOptions.SessionStore.RenewAsync(_sessionKey, ticket);
					var principal = new ClaimsPrincipal(
						new ClaimsIdentity(
							new[] { new Claim(SessionIdClaim, _sessionKey, ClaimValueTypes.String, Options.CookieAuthenticationOptions.ClaimsIssuer) },
							Scheme.Name));
					ticket = new AuthenticationTicket(principal, null, Scheme.Name);
				}

				var cookieValue = Options.CookieAuthenticationOptions.TicketDataFormat.Protect(ticket, GetTlsTokenBinding());

				var cookieOptions = BuildCookieOptions();
				if (properties.IsPersistent && _refreshExpiresUtc.HasValue)
				{
					cookieOptions.Expires = _refreshExpiresUtc.Value.ToUniversalTime();
				}

				Options.CookieAuthenticationOptions.CookieManager.AppendResponseCookie(
					Context,
					Options.CookieAuthenticationOptions.Cookie.Name,
					cookieValue,
					cookieOptions);

				await ApplyHeaders(shouldRedirectToReturnUrl: false, properties: properties);

				var cookieStore = Context.RequestServices.GetService<ICookieStore>();
				if (cookieStore != null)
				{
					int? idUser = null;
					if (ticket.Principal is RaiderPrincipal<int> principal)
						idUser = principal.IdentityBase?.UserId;

					var issuedUtc = properties.IssuedUtc ?? Clock.UtcNow;
					var expiresUtc = properties.ExpiresUtc ?? issuedUtc.Add(Options.CookieAuthenticationOptions.ExpireTimeSpan);

					await cookieStore.InsertAsync(Context, cookieValue, issuedUtc.UtcDateTime, expiresUtc.UtcDateTime, idUser);

					//cannot delete previous cookie because parallel requests can be rejected
					//var cookie = Options.CookieAuthenticationOptions.CookieManager.GetRequestCookie(Context, Options.CookieAuthenticationOptions.Cookie.Name);
					//if (!string.IsNullOrWhiteSpace(cookie))
					//	await cookieStore.DeleteAsync(Context, cookie, true);
				}
			}
		}

		private async Task ApplyHeaders(bool shouldRedirectToReturnUrl, AuthenticationProperties properties)
		{
			Response.Headers[HeaderNames.CacheControl] = HeaderValueNoCache;
			Response.Headers[HeaderNames.Pragma] = HeaderValueNoCache;
			Response.Headers[HeaderNames.Expires] = HeaderValueMinusOne;

			if (shouldRedirectToReturnUrl && Response.StatusCode == 200)
			{
				// set redirect uri in order:
				// 1. properties.RedirectUri
				// 2. query parameter ReturnUrlParameter
				//
				// Absolute uri is not allowed if it is from query string as query string is not
				// a trusted source.
				var redirectUri = properties.RedirectUri;
				if (string.IsNullOrEmpty(redirectUri))
				{
					redirectUri = Request.Query[Options.CookieAuthenticationOptions.ReturnUrlParameter];
					if (string.IsNullOrEmpty(redirectUri) || !IsHostRelative(redirectUri))
					{
						redirectUri = null;
					}
				}

				if (redirectUri != null)
				{
					await Events.CookieEvents.RedirectToReturnUrl(
						new RedirectContext<CookieAuthenticationOptions>(Context, Scheme, Options.CookieAuthenticationOptions, properties, redirectUri));
				}
			}
		}

		private static bool IsHostRelative(string path)
		{
			if (string.IsNullOrEmpty(path))
			{
				return false;
			}
			if (path.Length == 1)
			{
				return path[0] == '/';
			}
			return path[0] == '/' && path[1] != '/' && path[1] != '\\';
		}

		private string? GetTlsTokenBinding()
		{
			var binding = Context.Features.Get<ITlsTokenBindingFeature>()?.GetProvidedTokenBindingId();
			return binding == null ? null : Convert.ToBase64String(binding);
		}

		public async virtual Task SignInAsync(ClaimsPrincipal user, AuthenticationProperties properties)
		{
			if (user == null)
				throw new ArgumentNullException(nameof(user));

			if (Options.UseCookieAuthentication)
			{
				properties = properties ?? new AuthenticationProperties();

				_signInCalled = true;

				// Process the request cookie to initialize members like _sessionKey.
				var result = await EnsureCookieTicket();
				var cookieOptions = BuildCookieOptions();

				var signInContext = new CookieSigningInContext(
					Context,
					Scheme,
					Options.CookieAuthenticationOptions,
					user,
					properties,
					cookieOptions);

				DateTimeOffset issuedUtc;
				if (signInContext.Properties.IssuedUtc.HasValue)
				{
					issuedUtc = signInContext.Properties.IssuedUtc.Value;
				}
				else
				{
					issuedUtc = Clock.UtcNow;
					signInContext.Properties.IssuedUtc = issuedUtc;
				}

				if (!signInContext.Properties.ExpiresUtc.HasValue)
				{
					signInContext.Properties.ExpiresUtc = issuedUtc.Add(Options.CookieAuthenticationOptions.ExpireTimeSpan);
				}

				await Events.CookieEvents.SigningIn(signInContext);

				DateTimeOffset expiresUtc = issuedUtc.Add(Options.CookieAuthenticationOptions.ExpireTimeSpan);
				if (signInContext.Properties.IsPersistent)
				{
					if (signInContext.Properties.ExpiresUtc.HasValue)
						expiresUtc = signInContext.Properties.ExpiresUtc.Value;

					expiresUtc = expiresUtc.ToUniversalTime();
					signInContext.CookieOptions.Expires = expiresUtc;
				}

				var ticket = new AuthenticationTicket(signInContext.Principal, signInContext.Properties, signInContext.Scheme.Name);

				if (Options.CookieAuthenticationOptions.SessionStore != null)
				{
					if (_sessionKey != null)
					{
						await Options.CookieAuthenticationOptions.SessionStore.RemoveAsync(_sessionKey);
					}
					_sessionKey = await Options.CookieAuthenticationOptions.SessionStore.StoreAsync(ticket);
					var principal = new ClaimsPrincipal(
						new ClaimsIdentity(
							new[] { new Claim(SessionIdClaim, _sessionKey, ClaimValueTypes.String, Options.CookieAuthenticationOptions.ClaimsIssuer) },
							Options.CookieAuthenticationOptions.ClaimsIssuer));
					ticket = new AuthenticationTicket(principal, null, Scheme.Name);
				}

				var cookieValue = Options.CookieAuthenticationOptions.TicketDataFormat.Protect(ticket, GetTlsTokenBinding());

				Options.CookieAuthenticationOptions.CookieManager.AppendResponseCookie(
					Context,
					Options.CookieAuthenticationOptions.Cookie.Name,
					cookieValue,
					signInContext.CookieOptions);

				var signedInContext = new CookieSignedInContext(
					Context,
					Scheme,
					signInContext.Principal,
					signInContext.Properties,
					Options.CookieAuthenticationOptions);

				await Events.CookieEvents.SignedIn(signedInContext);

				var cookieStore = Context.RequestServices.GetService<ICookieStore>();
				if (cookieStore != null)
				{
					int? idUser = null;
					if (user is RaiderPrincipal<int> principal)
						idUser = principal.IdentityBase?.UserId;

					await cookieStore.InsertAsync(Context, cookieValue, issuedUtc.UtcDateTime, expiresUtc.UtcDateTime, idUser);
				}

				// Only redirect on the login path
				var shouldRedirect = Options.CookieAuthenticationOptions.LoginPath.HasValue && OriginalPath == Options.CookieAuthenticationOptions.LoginPath;
				await ApplyHeaders(shouldRedirect, signedInContext.Properties);

				var appCtx = Context.RequestServices.GetRequiredService<IApplicationContext>();
				_logger.LogInformationMessage(appCtx.Next(), x => x.InternalMessage($"AuthenticationScheme: {Scheme.Name} signed in."));
			}
			else
			{
				throw new InvalidOperationException();
			}
		}

		public async virtual Task SignOutAsync(AuthenticationProperties properties)
		{
			if (Options.UseCookieAuthentication)
			{
				properties = properties ?? new AuthenticationProperties();

				_signOutCalled = true;

				// Process the request cookie to initialize members like _sessionKey.
				var ticket = await EnsureCookieTicket();
				var cookieOptions = BuildCookieOptions();
				if (Options.CookieAuthenticationOptions.SessionStore != null && _sessionKey != null)
				{
					await Options.CookieAuthenticationOptions.SessionStore.RemoveAsync(_sessionKey);
				}

				var context = new CookieSigningOutContext(
					Context,
					Scheme,
					Options.CookieAuthenticationOptions,
					properties,
					cookieOptions);

				await Events.CookieEvents.SigningOut(context);

				var cookieStore = Context.RequestServices.GetService<ICookieStore>();
				if (cookieStore != null)
				{
					var cookie = Options.CookieAuthenticationOptions.CookieManager.GetRequestCookie(Context, Options.CookieAuthenticationOptions.Cookie.Name);
					if (!string.IsNullOrWhiteSpace(cookie))
						await cookieStore.DeleteAsync(Context, cookie, true);
				}

				Options.CookieAuthenticationOptions.CookieManager.DeleteCookie(
					Context,
					Options.CookieAuthenticationOptions.Cookie.Name,
					context.CookieOptions);

				// Only redirect on the logout path
				var shouldRedirect = Options.CookieAuthenticationOptions.LogoutPath.HasValue && OriginalPath == Options.CookieAuthenticationOptions.LogoutPath;
				await ApplyHeaders(shouldRedirect, context.Properties);

				var appCtx = Context.RequestServices.GetRequiredService<IApplicationContext>();
				_logger.LogInformationMessage(appCtx.Next(), x => x.InternalMessage($"AuthenticationScheme: {Scheme.Name} signed out."));
			}
			else
			{
				throw new InvalidOperationException();
			}
		}

		#endregion Cookie

		#region Token

		private static string CreateErrorDescription(Exception authFailure)
		{
			IEnumerable<Exception> exceptions;
			if (authFailure is AggregateException)
			{
				var agEx = authFailure as AggregateException;
				exceptions = agEx.InnerExceptions;
			}
			else
			{
				exceptions = new[] { authFailure };
			}

			var messages = new List<string>();

			foreach (var ex in exceptions)
			{
				// Order sensitive, some of these exceptions derive from others
				// and we want to display the most specific message possible.
				if (ex is SecurityTokenInvalidAudienceException)
				{
					messages.Add("The audience is invalid");
				}
				else if (ex is SecurityTokenInvalidIssuerException)
				{
					messages.Add("The issuer is invalid");
				}
				else if (ex is SecurityTokenNoExpirationException)
				{
					messages.Add("The token has no expiration");
				}
				else if (ex is SecurityTokenInvalidLifetimeException)
				{
					messages.Add("The token lifetime is invalid");
				}
				else if (ex is SecurityTokenNotYetValidException)
				{
					messages.Add("The token is not valid yet");
				}
				else if (ex is SecurityTokenExpiredException)
				{
					messages.Add("The token is expired");
				}
				else if (ex is SecurityTokenSignatureKeyNotFoundException)
				{
					messages.Add("The signature key was not found");
				}
				else if (ex is SecurityTokenInvalidSignatureException)
				{
					messages.Add("The signature is invalid");
				}
			}

			return string.Join("; ", messages);
		}

		#endregion Token

		private static Task Redirect<TOptions>(RedirectContext<TOptions> context)
			where TOptions : AuthenticationSchemeOptions
		{
			if (IsAjaxRequest(context.Request))
			{
				context.Response.Headers[HeaderNames.Location] = context.RedirectUri;
				context.Response.StatusCode = 401;
			}
			else
			{
				context.Response.Redirect(context.RedirectUri);
			}
			return Task.CompletedTask;
		}

		private static bool IsAjaxRequest(HttpRequest request)
		{
			return string.Equals(request.Query[HeaderNames.XRequestedWith], "XMLHttpRequest", StringComparison.Ordinal) ||
				string.Equals(request.Headers[HeaderNames.XRequestedWith], "XMLHttpRequest", StringComparison.Ordinal);
		}

		protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
		{
			var appCtx = Context.RequestServices.GetRequiredService<IApplicationContext>();

			if (Options.AuthenticationFlow != null && 0 < Options.AuthenticationFlow.Count)
			{
				AuthenticateResult result = null;
				AuthenticationType fallbackType = AuthenticationType.WindowsIntegrated;
				AuthenticateResult fallback = null;
				for (int i = 0; i < Options.AuthenticationFlow.Count; i++)
				{
					var currentAuthType = Options.AuthenticationFlow[i];
					bool isLast = i == Options.AuthenticationFlow.Count - 1;
					result = currentAuthType switch
					{
						AuthenticationType.WindowsIntegrated => await HandleWindowsAuthenticateAsync(),
						AuthenticationType.Cookie => await HandleCookieAuthenticateAsync(),
						AuthenticationType.Token => await HandleTokenAuthenticateAsync(),
						AuthenticationType.Request => await HandleRequestAuthenticateAsync(),
						_ => throw new NotImplementedException($"{nameof(AuthenticationType)}.{Options.AuthenticationFlow[i]}"),
					};
					if (result.Succeeded)
						return result;

					if ((!Options.AuthenticationFlowFallback.HasValue && isLast)
						|| Options.AuthenticationFlowFallback == currentAuthType)
					{
						fallbackType = currentAuthType;
						fallback = result;
					}
				}

				_logger.LogWarningMessage(appCtx.Next(), x => x.InternalMessage($"{fallbackType}: Authentication failed: {fallback?.Failure?.ToStringTrace()}"));
				return fallback;
			}

			_logger.LogErrorMessage(appCtx.Next(), x => x.InternalMessage($"No authentication configured."));

			ThrowAccessDenied401Unauthorized(Context, null);
			return AuthenticateResult.Fail("Invalid auth.");
		}

		protected async Task<AuthenticateResult> HandleWindowsAuthenticateAsync()
		{
			if ((Options.UseWindowsAuthentication) && Context != null)
			{
				var appCtx = Context.RequestServices.GetRequiredService<IApplicationContext>();

				WindowsValidatePrincipalContext context = null;
				if (Options.WindowsAuthenticationOptions.AllowStaticLogin || IdentityHelper.IsWindowsAuthentication(Context))
				{
					try
					{
						context = new WindowsValidatePrincipalContext(Context, Scheme, Options.WindowsAuthenticationOptions, _logger);
						await Events.WindowsEvents.ValidatePrincipal(context);

						if (context.Principal is RaiderPrincipal principal)
						{
							var ticket = new AuthenticationTicket(principal, Options.Scheme);
							return AuthenticateResult.Success(ticket);
						}
					}
					catch (Exception ex)
					{
						_logger.LogErrorMessage(appCtx.Next(), x => x.ExceptionInfo(ex).Detail($"{nameof(AuthenticationType.WindowsIntegrated)}: Failed to validate windows principal name = {context?.Principal?.Identity?.Name ?? "NULL"}"));

						ThrowAccessDenied401Unauthorized(Context, ex);
					}
				}
				else
				{
					_logger.LogWarningMessage(appCtx.Next(), x => x.InternalMessage($"{nameof(AuthenticationType.WindowsIntegrated)}: {nameof(HttpContext)}.{nameof(HttpContext.User)} = {Context.User?.GetType().FullName ?? "NULL"}, {nameof(HttpContext)}.{nameof(HttpContext.User)}.{nameof(HttpContext.User.Identity)} = {Context.User?.Identity?.GetType().FullName ?? "NULL"}, Name = {Context.User?.Identity?.Name} is not {nameof(WindowsIdentity)}"));
				}

				_logger.LogWarningMessage(appCtx.Next(), x => x.InternalMessage($"{nameof(AuthenticationType.WindowsIntegrated)}: Principal {context?.Principal?.GetType().FullName ?? "NULL"} is not {nameof(RaiderPrincipal)}"));

				return AuthenticateResult.Fail("Invalid auth.");
			}
			else
			{
				throw new InvalidOperationException("WindowsAuthentication is not allowed.");
			}
		}

		protected async Task<AuthenticateResult> HandleCookieAuthenticateAsync()
		{
			if (Options.UseCookieAuthentication)
			{
				var appCtx = Context.RequestServices.GetRequiredService<IApplicationContext>();

				var result = await EnsureCookieTicket();
				if (!result.Succeeded)
				{
					_logger.LogWarningMessage(appCtx.Next(), x => x.InternalMessage($"{nameof(AuthenticationType.Cookie)}: Cannot create {nameof(AuthenticationTicket)} from the cookie."));

					return result;
				}

				var context = new CookieValidatePrincipalContext(Context, Scheme, Options.CookieAuthenticationOptions, result.Ticket);
				await Events.CookieEvents.ValidatePrincipal(context);

				if (context.Principal == null)
				{
					_logger.LogWarningMessage(appCtx.Next(), x => x.InternalMessage($"{nameof(AuthenticationType.Cookie)}: {nameof(context.Principal)} NULL is not {nameof(RaiderPrincipal)}"));
					return AuthenticateResult.Fail("No principal.");
				}
				else if (!(context.Principal is RaiderPrincipal))
				{
					_logger.LogWarningMessage(appCtx.Next(), x => x.InternalMessage($"{nameof(AuthenticationType.Cookie)}: {nameof(context.Principal)} {context?.Principal?.GetType().FullName ?? "NULL"} is not {nameof(RaiderPrincipal)}"));
				}

				if (context.ShouldRenew)
				{
					RequestRefresh(result.Ticket);
				}

				return AuthenticateResult.Success(new AuthenticationTicket(context.Principal, context.Properties, Scheme.Name));
			}
			else
			{
				throw new InvalidOperationException("CookieAuthentication is not allowed.");
			}
		}

		protected async Task<AuthenticateResult> HandleTokenAuthenticateAsync()
		{
			if (Options.UseTokenAuthentication)
			{
				var appCtx = Context.RequestServices.GetRequiredService<IApplicationContext>();

				string token = null;
				try
				{
					// Give application opportunity to find from a different location, adjust, or reject token
					var messageReceivedContext = new MessageReceivedContext(Context, Scheme, Options.TokenAuthenticationOptions);

					// event can set the token
					await Events.TokenEvents.MessageReceived(messageReceivedContext);
					if (messageReceivedContext.Result != null)
					{
						return messageReceivedContext.Result;
					}

					// If application retrieved token from somewhere else, use that.
					token = messageReceivedContext.Token;

					if (string.IsNullOrEmpty(token))
					{
						string authorization = Request.Headers["Authorization"];

						// If no authorization header found, nothing to process further
						if (string.IsNullOrEmpty(authorization))
						{
							_logger.LogWarningMessage(appCtx.Next(), x => x.InternalMessage($"{nameof(AuthenticationType.Token)}: Authorization http header is not provided."));
							return AuthenticateResult.NoResult();
						}

						if (authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
						{
							token = authorization.Substring("Bearer ".Length).Trim();
						}

						// If no token found, no further work possible
						if (string.IsNullOrEmpty(token))
						{
							_logger.LogWarningMessage(appCtx.Next(), x => x.InternalMessage($"{nameof(AuthenticationType.Token)}: Authorization http header is not a 'Bearer' token."));
							return AuthenticateResult.NoResult();
						}
					}

					if (_configuration == null && Options.TokenAuthenticationOptions.ConfigurationManager != null)
					{
						_configuration = await Options.TokenAuthenticationOptions.ConfigurationManager.GetConfigurationAsync(Context.RequestAborted);
					}

					var validationParameters = Options.TokenAuthenticationOptions.TokenValidationParameters.Clone();
					if (_configuration != null)
					{
						var issuers = new[] { _configuration.Issuer };
						validationParameters.ValidIssuers = validationParameters.ValidIssuers?.Concat(issuers) ?? issuers;

						validationParameters.IssuerSigningKeys = validationParameters.IssuerSigningKeys?.Concat(_configuration.SigningKeys)
							?? _configuration.SigningKeys;
					}

					List<Exception> validationFailures = null;
					SecurityToken validatedToken;
					foreach (var validator in Options.TokenAuthenticationOptions.SecurityTokenValidators)
					{
						if (validator.CanReadToken(token))
						{
							ClaimsPrincipal principal;
							try
							{
								principal = validator.ValidateToken(token, validationParameters, out validatedToken);
							}
							catch (Exception ex)
							{
								_logger.LogErrorMessage(appCtx.Next(), x => x.ExceptionInfo(ex).Detail($"Failed to validate the token {token}."));

								// Refresh the configuration for exceptions that may be caused by key rollovers. The user can also request a refresh in the event.
								if (Options.TokenAuthenticationOptions.RefreshOnIssuerKeyNotFound && Options.TokenAuthenticationOptions.ConfigurationManager != null
									&& ex is SecurityTokenSignatureKeyNotFoundException)
								{
									Options.TokenAuthenticationOptions.ConfigurationManager.RequestRefresh();
								}

								if (validationFailures == null)
								{
									validationFailures = new List<Exception>(1);
								}
								validationFailures.Add(ex);
								continue;
							}

							_logger.LogInformationMessage(appCtx.Next(), x => x.InternalMessage("Successfully validated the token."));

							var tokenValidatedContext = new TokenValidatedContext(Context, Scheme, Options.TokenAuthenticationOptions)
							{
								Principal = principal,
								SecurityToken = validatedToken
							};

							await Events.TokenEvents.TokenValidated(tokenValidatedContext);
							if (tokenValidatedContext.Result != null)
							{
								return tokenValidatedContext.Result;
							}

							if (Options.TokenAuthenticationOptions.SaveToken)
							{
								tokenValidatedContext.Properties.StoreTokens(new[]
								{
									new AuthenticationToken { Name = "access_token", Value = token }
								});
							}

							tokenValidatedContext.Success();
							return tokenValidatedContext.Result;
						}
					}

					if (validationFailures != null)
					{
						var authenticationFailedContext = new AuthenticationFailedContext(Context, Scheme, Options.TokenAuthenticationOptions)
						{
							Exception = (validationFailures.Count == 1) ? validationFailures[0] : new AggregateException(validationFailures)
						};

						await Events.TokenEvents.AuthenticationFailed(authenticationFailedContext);
						if (authenticationFailedContext.Result != null)
						{
							return authenticationFailedContext.Result;
						}

						_logger.LogWarningMessage(appCtx.Next(), x => x.ExceptionInfo(authenticationFailedContext.Exception).Detail($"{nameof(AuthenticationType.Token)}: Token authentication failed."));
						return AuthenticateResult.Fail(authenticationFailedContext.Exception);
					}

					_logger.LogWarningMessage(appCtx.Next(), x => x.InternalMessage($"{nameof(AuthenticationType.Token)}: No SecurityTokenValidator available for token: {token ?? "NULL"}"));
					return AuthenticateResult.Fail("No SecurityTokenValidator available for token: " + token ?? "NULL");
				}
				catch (Exception ex)
				{
					_logger.LogErrorMessage(appCtx.Next(), x => x.ExceptionInfo(ex).Detail("Exception occurred while processing token message."));

					var authenticationFailedContext = new AuthenticationFailedContext(Context, Scheme, Options.TokenAuthenticationOptions)
					{
						Exception = ex
					};

					await Events.TokenEvents.AuthenticationFailed(authenticationFailedContext);
					if (authenticationFailedContext.Result != null)
					{
						return authenticationFailedContext.Result;
					}

					throw;
				}
			}
			else
			{
				throw new InvalidOperationException("TokenAuthentication is not allowed.");
			}

		}

		protected async Task<AuthenticateResult> HandleRequestAuthenticateAsync()
		{
			if ((Options.UseRequestAuthentication) && Context != null)
			{
				RequestValidatePrincipalContext context = null;
				var appCtx = Context.RequestServices.GetRequiredService<IApplicationContext>();

				try
				{
					context = new RequestValidatePrincipalContext(Context, Scheme, Options.RequestAuthenticationOptions, _logger);
					await Events.RequestEvents.ValidatePrincipal(context);

					if (context.Principal is RaiderPrincipal principal)
					{
						var ticket = new AuthenticationTicket(principal, Options.Scheme);
						return AuthenticateResult.Success(ticket);
					}
				}
				catch (Exception ex)
				{
					_logger.LogErrorMessage(appCtx.Next(), x => x.ExceptionInfo(ex).Detail($"{nameof(AuthenticationType.Request)}: Failed to validate request principal name = {context?.Principal?.Identity?.Name ?? "NULL"}"));
					ThrowAccessDenied401Unauthorized(Context, ex);
				}

				_logger.LogWarningMessage(appCtx.Next(), x => x.InternalMessage($"{nameof(AuthenticationType.Request)}: Principal {context?.Principal?.GetType().FullName ?? "NULL"} is not {nameof(RaiderPrincipal)}"));
				return AuthenticateResult.Fail("Invalid auth.");
			}
			else
			{
				throw new InvalidOperationException("RequestAuthentication is not allowed.");
			}
		}

		/// <summary>
		/// Called from <see cref="Raider.AspNetCore.Middleware.Authorization.PermissionAuthorizationFilter.OnAuthorizationAsync(Microsoft.AspNetCore.Mvc.Filters.AuthorizationFilterContext)"/>
		/// </summary>
		/// <param name="properties"></param>
		/// <returns></returns>
		protected override async Task HandleForbiddenAsync(AuthenticationProperties properties)
		{
			if (Options.AuthenticationFlow != null && 0 < Options.AuthenticationFlow.Count)
			{
				var fallback = Options.AuthenticationFlowFallback ?? Options.AuthenticationFlow[Options.AuthenticationFlow.Count - 1]; //last
				switch (fallback)
				{
					case AuthenticationType.WindowsIntegrated:
						await HandleWindowsForbiddenAsync(properties);
						break;
					case AuthenticationType.Cookie:
						await HandleCookieForbiddenAsync(properties);
						break;
					case AuthenticationType.Token:
						await HandleTokenForbiddenAsync(properties);
						break;
					case AuthenticationType.Request:
						await HandleRequestForbiddenAsync(properties);
						break;
					default:
						throw new NotImplementedException($"{nameof(AuthenticationType)}.{fallback}");
				}
			}
			else
				await base.HandleForbiddenAsync(properties);
		}

		protected async Task HandleWindowsForbiddenAsync(AuthenticationProperties properties)
		{
			var appCtx = Context.RequestServices.GetRequiredService<IApplicationContext>();
			_logger.LogWarningMessage(appCtx.Next(), x => x.InternalMessage($"{nameof(HandleWindowsForbiddenAsync)} occured."));
			if (Options.UseWindowsAuthentication)
				await base.HandleForbiddenAsync(properties);
			else
				throw new InvalidOperationException("WindowsAuthentication is not allowed.");

			if (!string.IsNullOrWhiteSpace(Options.WindowsAuthenticationOptions.AccessDeniedPath))
			{
				if (Options.UseWindowsAuthentication)
				{
					var returnUrl = properties.RedirectUri;
					if (string.IsNullOrEmpty(returnUrl))
					{
						returnUrl = OriginalPathBase + Request.Path + Request.QueryString;
					}
					var accessDeniedUri = $"{Options.WindowsAuthenticationOptions.AccessDeniedPath}{(string.IsNullOrWhiteSpace(Options.WindowsAuthenticationOptions.ReturnUrlParameter) ? "" : QueryString.Create(Options.WindowsAuthenticationOptions.ReturnUrlParameter, returnUrl))}";
					var redirectContext = new RedirectContext<WindowsAuthenticationOptions>(Context, Scheme, Options.WindowsAuthenticationOptions, properties, BuildRedirectUri(accessDeniedUri));
					await Redirect(redirectContext);
				}
				else
				{
					throw new InvalidOperationException("WindowsAuthentication is not allowed.");
				}
			}
		}

		protected async Task HandleCookieForbiddenAsync(AuthenticationProperties properties)
		{
			var appCtx = Context.RequestServices.GetRequiredService<IApplicationContext>();
			_logger.LogWarningMessage(appCtx.Next(), x => x.InternalMessage($"{nameof(HandleCookieForbiddenAsync)} occured."));
			if (Options.UseCookieAuthentication)
				await base.HandleForbiddenAsync(properties);
			else
				throw new InvalidOperationException("CookieAuthentication is not allowed.");

			if (!string.IsNullOrWhiteSpace(Options.CookieAuthenticationOptions.AccessDeniedPath))
			{
				if (Options.UseCookieAuthentication)
				{
					var returnUrl = properties.RedirectUri;
					if (string.IsNullOrEmpty(returnUrl))
					{
						returnUrl = OriginalPathBase + Request.Path + Request.QueryString;
					}
					var accessDeniedUri = $"{Options.CookieAuthenticationOptions.AccessDeniedPath}{(string.IsNullOrWhiteSpace(Options.CookieAuthenticationOptions.ReturnUrlParameter) ? "" : QueryString.Create(Options.CookieAuthenticationOptions.ReturnUrlParameter, returnUrl))}";
					var redirectContext = new RedirectContext<CookieAuthenticationOptions>(Context, Scheme, Options.CookieAuthenticationOptions, properties, BuildRedirectUri(accessDeniedUri));
					await Events.CookieEvents.RedirectToAccessDenied(redirectContext);
				}
				else
				{
					throw new InvalidOperationException("CookieAuthentication is not allowed.");
				}
			}
		}

		protected async Task HandleTokenForbiddenAsync(AuthenticationProperties properties)
		{
			var appCtx = Context.RequestServices.GetRequiredService<IApplicationContext>();
			_logger.LogWarningMessage(appCtx.Next(), x => x.InternalMessage($"{nameof(HandleTokenForbiddenAsync)} occured."));
			if (Options.UseTokenAuthentication)
				await base.HandleForbiddenAsync(properties);
			else
				throw new InvalidOperationException("TokenAuthentication is not allowed.");

			//if (!string.IsNullOrWhiteSpace(Options.TokenAuthenticationOptions.AccessDeniedPath))
			//{
			//	if (Options.UseTokenAuthentication)
			//	{
			//		var returnUrl = properties.RedirectUri;
			//		if (string.IsNullOrEmpty(returnUrl))
			//		{
			//			returnUrl = OriginalPathBase + Request.Path + Request.QueryString;
			//		}
			//		var accessDeniedUri = $"{Options.TokenAuthenticationOptions.AccessDeniedPath}{(string.IsNullOrWhiteSpace(Options.TokenAuthenticationOptions.ReturnUrlParameter) ? "" : QueryString.Create(Options.TokenAuthenticationOptions.ReturnUrlParameter, returnUrl))}";
			//		var redirectContext = new RedirectContext<JwtBearerOptions>(Context, Scheme, Options.TokenAuthenticationOptions, properties, BuildRedirectUri(accessDeniedUri));
			//		await Redirect(redirectContext);
			//	}
			//	else
			//	{
			//		throw new InvalidOperationException("TokenAuthentication is not allowed.");
			//	}
			//}
		}

		protected async Task HandleRequestForbiddenAsync(AuthenticationProperties properties)
		{
			var appCtx = Context.RequestServices.GetRequiredService<IApplicationContext>();
			_logger.LogWarningMessage(appCtx.Next(), x => x.InternalMessage($"{nameof(HandleRequestForbiddenAsync)} occured."));
			if (Options.UseRequestAuthentication)
				await base.HandleForbiddenAsync(properties);
			else
				throw new InvalidOperationException("RequestAuthentication is not allowed.");

			if (!string.IsNullOrWhiteSpace(Options.RequestAuthenticationOptions.AccessDeniedPath))
			{
				if (Options.UseRequestAuthentication)
				{
					var returnUrl = properties.RedirectUri;
					if (string.IsNullOrEmpty(returnUrl))
					{
						returnUrl = OriginalPathBase + Request.Path + Request.QueryString;
					}
					var accessDeniedUri = $"{Options.RequestAuthenticationOptions.AccessDeniedPath}{(string.IsNullOrWhiteSpace(Options.RequestAuthenticationOptions.ReturnUrlParameter) ? "" : QueryString.Create(Options.RequestAuthenticationOptions.ReturnUrlParameter, returnUrl))}";
					var redirectContext = new RedirectContext<RequestAuthenticationOptions>(Context, Scheme, Options.RequestAuthenticationOptions, properties, BuildRedirectUri(accessDeniedUri));
					await Redirect(redirectContext);
				}
				else
				{
					throw new InvalidOperationException("RequestAuthentication is not allowed.");
				}
			}
		}

		/// <summary>
		/// Called from <see cref="Raider.AspNetCore.Middleware.Authorization.PermissionAuthorizationFilter.OnAuthorizationAsync(Microsoft.AspNetCore.Mvc.Filters.AuthorizationFilterContext)"/>
		/// </summary>
		/// <param name="properties"></param>
		/// <returns></returns>
		protected override async Task HandleChallengeAsync(AuthenticationProperties properties)
		{
			if (Options.AuthenticationFlow != null && 0 < Options.AuthenticationFlow.Count)
			{
				var fallback = Options.AuthenticationFlowFallback ?? Options.AuthenticationFlow[Options.AuthenticationFlow.Count - 1]; //last
				switch (fallback)
				{
					case AuthenticationType.WindowsIntegrated:
						await HandleWindowsChallengeAsync(properties);
						break;
					case AuthenticationType.Cookie:
						await HandleCookieChallengeAsync(properties);
						break;
					case AuthenticationType.Token:
						await HandleTokenChallengeAsync(properties);
						break;
					case AuthenticationType.Request:
						await HandleRequestChallengeAsync(properties);
						break;
					default:
						throw new NotImplementedException($"{nameof(AuthenticationType)}.{fallback}");
				}
			}
			else
				await base.HandleChallengeAsync(properties);
		}

		protected async Task HandleWindowsChallengeAsync(AuthenticationProperties properties)
		{
			var appCtx = Context.RequestServices.GetRequiredService<IApplicationContext>();
			_logger.LogWarningMessage(appCtx.Next(), x => x.InternalMessage($"{nameof(HandleWindowsChallengeAsync)} occured."));
			if (Options.UseWindowsAuthentication)
				await base.HandleChallengeAsync(properties);
			else
				throw new InvalidOperationException("WindowsAuthentication is not allowed.");

			if (!string.IsNullOrWhiteSpace(Options.WindowsAuthenticationOptions.UnauthorizedPath))
			{
				if (Options.UseWindowsAuthentication)
				{
					var returnUrl = properties.RedirectUri;
					if (string.IsNullOrEmpty(returnUrl))
					{
						returnUrl = OriginalPathBase + Request.Path + Request.QueryString;
					}
					var accessDeniedUri = $"{Options.WindowsAuthenticationOptions.UnauthorizedPath}{(string.IsNullOrWhiteSpace(Options.WindowsAuthenticationOptions.ReturnUrlParameter) ? "" : QueryString.Create(Options.WindowsAuthenticationOptions.ReturnUrlParameter, returnUrl))}";
					var redirectContext = new RedirectContext<WindowsAuthenticationOptions>(Context, Scheme, Options.WindowsAuthenticationOptions, properties, BuildRedirectUri(accessDeniedUri));
					await Redirect(redirectContext);
				}
				else
				{
					throw new InvalidOperationException("WindowsAuthentication is not allowed.");
				}
			}
		}

		protected async Task HandleCookieChallengeAsync(AuthenticationProperties properties)
		{
			var appCtx = Context.RequestServices.GetRequiredService<IApplicationContext>();
			_logger.LogWarningMessage(appCtx.Next(), x => x.InternalMessage($"{nameof(HandleCookieChallengeAsync)} occured."));
			if (Options.UseCookieAuthentication)
			{
				var redirectUri = properties.RedirectUri;
				if (string.IsNullOrEmpty(redirectUri))
				{
					redirectUri = OriginalPathBase + Request.Path + Request.QueryString;
				}

				var loginUri = Options.CookieAuthenticationOptions.LoginPath + QueryString.Create(Options.CookieAuthenticationOptions.ReturnUrlParameter, redirectUri);
				var redirectContext = new RedirectContext<CookieAuthenticationOptions>(Context, Scheme, Options.CookieAuthenticationOptions, properties, BuildRedirectUri(loginUri));
				await Events.CookieEvents.RedirectToLogin(redirectContext);
			}
			else
			{
				throw new InvalidOperationException("CookieAuthentication is not allowed.");
			}
		}

		protected async Task HandleTokenChallengeAsync(AuthenticationProperties properties)
		{
			var appCtx = Context.RequestServices.GetRequiredService<IApplicationContext>();
			_logger.LogWarningMessage(appCtx.Next(), x => x.InternalMessage($"{nameof(HandleTokenChallengeAsync)} occured."));
			if (Options.UseTokenAuthentication)
			{
				var authResult = await HandleAuthenticateOnceSafeAsync();
				var eventContext = new JwtBearerChallengeContext(Context, Scheme, Options.TokenAuthenticationOptions, properties)
				{
					AuthenticateFailure = authResult?.Failure
				};

				// Avoid returning error=invalid_token if the error is not caused by an authentication failure (e.g missing token).
				if (Options.TokenAuthenticationOptions.IncludeErrorDetails && eventContext.AuthenticateFailure != null)
				{
					eventContext.Error = "invalid_token";
					eventContext.ErrorDescription = CreateErrorDescription(eventContext.AuthenticateFailure);
				}

				await Events.TokenEvents.Challenge(eventContext);
				if (eventContext.Handled)
				{
					return;
				}

				Response.StatusCode = 401;

				if (string.IsNullOrEmpty(eventContext.Error) &&
					string.IsNullOrEmpty(eventContext.ErrorDescription) &&
					string.IsNullOrEmpty(eventContext.ErrorUri))
				{
					Response.Headers.Append(HeaderNames.WWWAuthenticate, Options.TokenAuthenticationOptions.Challenge);
				}
				else
				{
					// https://tools.ietf.org/html/rfc6750#section-3.1
					// WWW-Authenticate: Bearer realm="example", error="invalid_token", error_description="The access token expired"
					var builder = new StringBuilder(Options.TokenAuthenticationOptions.Challenge);
					if (Options.TokenAuthenticationOptions.Challenge.IndexOf(" ", StringComparison.Ordinal) > 0)
					{
						// Only add a comma after the first param, if any
						builder.Append(',');
					}
					if (!string.IsNullOrEmpty(eventContext.Error))
					{
						builder.Append(" error=\"");
						builder.Append(eventContext.Error);
						builder.Append('"');
					}
					if (!string.IsNullOrEmpty(eventContext.ErrorDescription))
					{
						if (!string.IsNullOrEmpty(eventContext.Error))
						{
							builder.Append(',');
						}

						builder.Append(" error_description=\"");
						builder.Append(eventContext.ErrorDescription);
						builder.Append('\"');
					}
					if (!string.IsNullOrEmpty(eventContext.ErrorUri))
					{
						if (!string.IsNullOrEmpty(eventContext.Error) ||
							!string.IsNullOrEmpty(eventContext.ErrorDescription))
						{
							builder.Append(',');
						}

						builder.Append(" error_uri=\"");
						builder.Append(eventContext.ErrorUri);
						builder.Append('\"');
					}

					Response.Headers.Append(HeaderNames.WWWAuthenticate, builder.ToString());
				}
			}
			else
			{
				throw new InvalidOperationException("TokenAuthentication is not allowed.");
			}
		}

		protected async Task HandleRequestChallengeAsync(AuthenticationProperties properties)
		{
			var appCtx = Context.RequestServices.GetRequiredService<IApplicationContext>();
			_logger.LogWarningMessage(appCtx.Next(), x => x.InternalMessage($"{nameof(HandleRequestChallengeAsync)} occured."));
			if (Options.UseRequestAuthentication)
				await base.HandleChallengeAsync(properties);
			else
				throw new InvalidOperationException("RequestAuthentication is not allowed.");

			if (!string.IsNullOrWhiteSpace(Options.RequestAuthenticationOptions.UnauthorizedPath))
			{
				if (Options.UseRequestAuthentication)
				{
					var returnUrl = properties.RedirectUri;
					if (string.IsNullOrEmpty(returnUrl))
					{
						returnUrl = OriginalPathBase + Request.Path + Request.QueryString;
					}
					var accessDeniedUri = $"{Options.RequestAuthenticationOptions.UnauthorizedPath}{(string.IsNullOrWhiteSpace(Options.RequestAuthenticationOptions.ReturnUrlParameter) ? "" : QueryString.Create(Options.RequestAuthenticationOptions.ReturnUrlParameter, returnUrl))}";
					var redirectContext = new RedirectContext<RequestAuthenticationOptions>(Context, Scheme, Options.RequestAuthenticationOptions, properties, BuildRedirectUri(accessDeniedUri));
					await Redirect(redirectContext);
				}
				else
				{
					throw new InvalidOperationException("RequestAuthentication is not allowed.");
				}
			}
		}

		private static void ThrowAccessDenied401Unauthorized(HttpContext context, Exception? ex = null)
		{
			if (context == null)
				throw new ArgumentNullException(nameof(context));

			var statusCode = (int)Microsoft.AspNetCore.Http.StatusCodes.Status401Unauthorized;
			context.Response.StatusCode = statusCode;
			throw new Exception($"Global exception with HTTP Status Code {statusCode}. Request path = {context.Request.GetUri().AbsoluteUri}", ex);
		}
	}
}

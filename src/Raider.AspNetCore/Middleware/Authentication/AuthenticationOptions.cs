using Raider.AspNetCore.Middleware.Authentication.Events;
using Raider.AspNetCore.Middleware.Authentication.RequestAuth;
using Raider.AspNetCore.Middleware.Authentication.RequestAuth.Events;
using Raider.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Linq;

namespace Raider.AspNetCore.Middleware.Authentication
{
	public class AuthenticationOptions : AuthenticationSchemeOptions
    {
		public enum AuthenticationType
		{
			WindowsIntegrated,
			Cookie,
			Token,
			Request
		}

        public string Scheme => AuthenticationDefaults.AuthenticationScheme;

		public new AuthenticationEvents Events
		{
			get => (AuthenticationEvents)base.Events;
			set => base.Events = value;
		}

		private List<AuthenticationType> _authenticationFlow = new List<AuthenticationType>();
		public IReadOnlyList<AuthenticationType> AuthenticationFlow { get; private set; }
		public AuthenticationType? AuthenticationFlowFallback { get; private set; }

		public WindowsAuthenticationOptions WindowsAuthenticationOptions { get; private set; }
		public bool UseWindowsAuthentication => WindowsAuthenticationOptions != null;
		public Func<WindowsValidatePrincipalContext, Task> OnValidateWindowsPrincipal { get; private set; }

		//HttpContext context, string authenticationSchemeType, ILogger logger
		public Func<HttpContext, string, ILogger, Task<RaiderPrincipal?>> CreateWindowsPrincipal { get; private set; }

		public bool DisableCookieAuthenticationChallenge { get; set; }
		public CookieAuthenticationOptions CookieAuthenticationOptions { get; private set; }

		private string _cookieName;
		public string CookieName => _cookieName ??= $"{CookieAuthenticationDefaults.CookiePrefix}{AuthenticationDefaults.AuthenticationScheme}";

		public bool UseCookieAuthentication => CookieAuthenticationOptions != null;
		public Func<CookieValidatePrincipalContext, Task> OnValidateCookiePrincipal { get; private set; }

		//HttpContext context, string userName, string authenticationSchemeType, ILogger logger
		public Func<HttpContext, string, string, ILogger, Task<RaiderPrincipal?>> RecreateCookiePrincipal { get; private set; }
		public Func<ClaimsPrincipal, RaiderPrincipal> ConvertCookiePrincipal { get; private set; }

		public bool DisableTokenAuthenticationChallenge { get; set; }
		public JwtBearerOptions TokenAuthenticationOptions { get; private set; }
		public bool UseTokenAuthentication => TokenAuthenticationOptions != null;
		public Func<TokenValidatedContext, Task> OnValidateTokenPrincipal { get; private set; }

		//HttpContext context, string userName, string authenticationSchemeType, ILogger logger
		public Func<HttpContext, string, string, ILogger, RaiderPrincipal> RecreateTokenPrincipal { get; private set; }
		public Func<ClaimsPrincipal, RaiderPrincipal> ConvertTokenPrincipal { get; private set; }

		public RequestAuthenticationOptions RequestAuthenticationOptions { get; private set; }
		public bool UseRequestAuthentication => RequestAuthenticationOptions != null;
		public Func<RequestValidatePrincipalContext, Task> OnValidateRequestPrincipal { get; private set; }

		//HttpContext context, string authenticationSchemeType, ILogger logger
		public Func<HttpContext, string, ILogger, Task<RaiderPrincipal?>> CreateRequestPrincipal { get; private set; }

		public AuthenticationOptions SetWindowsAuthentication(WindowsAuthenticationOptions options, Func<WindowsValidatePrincipalContext, Task> onValidatePrincipal)
		{
			AddAuthentication(AuthenticationType.WindowsIntegrated);
			WindowsAuthenticationOptions = options ?? throw new ArgumentNullException(nameof(options));
			OnValidateWindowsPrincipal = onValidatePrincipal ?? throw new ArgumentNullException(nameof(onValidatePrincipal));
			return this;
		}

		public AuthenticationOptions SetWindowsAuthentication(Func<HttpContext, string, ILogger, Task<RaiderPrincipal?>> createPrincipal, bool disableWindowsAuthenticationChallenge = false)
		{
			AddAuthentication(AuthenticationType.WindowsIntegrated);
			WindowsAuthenticationOptions = new WindowsAuthenticationOptions { DisableAuthenticationChallenge = disableWindowsAuthenticationChallenge };
			CreateWindowsPrincipal = createPrincipal ?? throw new ArgumentNullException(nameof(createPrincipal));
			OnValidateWindowsPrincipal = async context =>
			{
				if (context.HttpContext?.User != null && context.HttpContext.User.Identity != null)
					context.Principal = await CreateWindowsPrincipal(context.HttpContext, context.Scheme.Name, context.Logger);
			};

			return this;
		}

		public AuthenticationOptions SetCookieAuthentication(Func<CookieValidatePrincipalContext, Task> onValidatePrincipal, bool disableCookieAuthenticationChallenge = false)
			=> SetCookieAuthentication(new CookieAuthenticationOptions(), onValidatePrincipal, disableCookieAuthenticationChallenge);

		public AuthenticationOptions SetCookieAuthenticationReplacePrincipal(Func<HttpContext, string, string, ILogger, Task<RaiderPrincipal?>> recreatePrincipal, bool disableCookieAuthenticationChallenge = false)
			=> SetCookieAuthenticationReplacePrincipal(new CookieAuthenticationOptions(), recreatePrincipal, disableCookieAuthenticationChallenge);

		public AuthenticationOptions SetCookieAuthenticationConvertPrincipal(Func<ClaimsPrincipal, RaiderPrincipal> convertPrincipal, bool disableCookieAuthenticationChallenge = false)
			=> SetCookieAuthenticationConvertPrincipal(new CookieAuthenticationOptions(), convertPrincipal, disableCookieAuthenticationChallenge);

		public AuthenticationOptions SetCookieAuthentication(CookieAuthenticationOptions options, Func<CookieValidatePrincipalContext, Task> onValidatePrincipal, bool disableCookieAuthenticationChallenge = false)
		{
			AddAuthentication(AuthenticationType.Cookie);
			CookieAuthenticationOptions = options ?? throw new ArgumentNullException(nameof(options));
			DisableCookieAuthenticationChallenge = disableCookieAuthenticationChallenge;
			OnValidateCookiePrincipal = onValidatePrincipal ?? throw new ArgumentNullException(nameof(onValidatePrincipal));
			return this;
		}

		public AuthenticationOptions SetCookieAuthenticationReplacePrincipal(CookieAuthenticationOptions options, Func<HttpContext, string, string, ILogger, Task<RaiderPrincipal?>> recreatePrincipal, bool disableCookieAuthenticationChallenge = false)
		{
			AddAuthentication(AuthenticationType.Cookie);
			CookieAuthenticationOptions = options ?? throw new ArgumentNullException(nameof(options));
			DisableCookieAuthenticationChallenge = disableCookieAuthenticationChallenge;
			RecreateCookiePrincipal = recreatePrincipal ?? throw new ArgumentNullException(nameof(recreatePrincipal));
			OnValidateCookiePrincipal = async context =>
			{
				if (context.Principal != null && context.Principal.Identity != null)
					context.Principal = await RecreateCookiePrincipal(context.HttpContext, context.Principal?.Identity?.Name, context.Scheme.Name, null);
			};

			return this;
		}

		public AuthenticationOptions SetCookieAuthenticationConvertPrincipal(CookieAuthenticationOptions options, Func<ClaimsPrincipal, RaiderPrincipal> convertPrincipal, bool disableCookieAuthenticationChallenge = false)
		{
			AddAuthentication(AuthenticationType.Cookie);
			CookieAuthenticationOptions = options ?? throw new ArgumentNullException(nameof(options));
			DisableCookieAuthenticationChallenge = disableCookieAuthenticationChallenge;
			ConvertCookiePrincipal = convertPrincipal ?? throw new ArgumentNullException(nameof(convertPrincipal));
			OnValidateCookiePrincipal = context =>
			{
				if (context.Principal != null && context.Principal.Identity != null)
					context.Principal = ConvertCookiePrincipal(context.Principal);

				return Task.FromResult(0);
			};

			return this;
		}

		public AuthenticationOptions SetCookieName(string cookieName)
		{
			_cookieName = cookieName;
			return this;
		}

		public AuthenticationOptions SetTokenAuthentication(TokenValidationParameters tokenValidationParameters, Func<TokenValidatedContext, Task> onValidatePrincipal, bool disableTokenAuthenticationChallenge = false)
		{
			return SetTokenAuthentication(
				new JwtBearerOptions
				{
					TokenValidationParameters = tokenValidationParameters
				},
				onValidatePrincipal,
				disableTokenAuthenticationChallenge);
		}

		public AuthenticationOptions SetTokenAuthenticationReplacePrincipal(TokenValidationParameters tokenValidationParameters, Func<HttpContext, string, string, ILogger, RaiderPrincipal> recreatePrincipal, bool disableTokenAuthenticationChallenge = false)
		{
			return SetTokenAuthenticationReplacePrincipal(
				new JwtBearerOptions
				{
					TokenValidationParameters = tokenValidationParameters
				},
				recreatePrincipal,
				disableTokenAuthenticationChallenge);
		}

		public AuthenticationOptions SetTokenAuthenticationConvertPrincipal(TokenValidationParameters tokenValidationParameters, Func<ClaimsPrincipal, RaiderPrincipal> convertPrincipal, bool disableTokenAuthenticationChallenge = false)
		{
			return SetTokenAuthenticationConvertPrincipal(
				new JwtBearerOptions
				{
					TokenValidationParameters = tokenValidationParameters
				},
				convertPrincipal,
				disableTokenAuthenticationChallenge);
		}

		public AuthenticationOptions SetTokenAuthentication(JwtBearerOptions options, Func<TokenValidatedContext, Task> onValidatePrincipal, bool disableTokenAuthenticationChallenge = false)
		{
			AddAuthentication(AuthenticationType.Token);
			TokenAuthenticationOptions = options ?? throw new ArgumentNullException(nameof(options));
			DisableTokenAuthenticationChallenge = disableTokenAuthenticationChallenge;
			OnValidateTokenPrincipal = onValidatePrincipal ?? throw new ArgumentNullException(nameof(onValidatePrincipal));
			return this;
		}

		public AuthenticationOptions SetTokenAuthenticationReplacePrincipal(JwtBearerOptions options, Func<HttpContext, string, string, ILogger, RaiderPrincipal> recreatePrincipal, bool disableTokenAuthenticationChallenge = false)
		{
			AddAuthentication(AuthenticationType.Token);
			TokenAuthenticationOptions = options ?? throw new ArgumentNullException(nameof(options));
			DisableTokenAuthenticationChallenge = disableTokenAuthenticationChallenge;
			RecreateTokenPrincipal = recreatePrincipal ?? throw new ArgumentNullException(nameof(recreatePrincipal));
			OnValidateTokenPrincipal = context =>
			{
				if (context.Principal != null && context.Principal.Identity != null)
					context.Principal = RecreateTokenPrincipal(context.HttpContext, context.Principal?.Identity?.Name, context.Scheme.Name, null);

				return Task.FromResult(0);
			};

			return this;
		}

		public AuthenticationOptions SetTokenAuthenticationConvertPrincipal(JwtBearerOptions options, Func<ClaimsPrincipal, RaiderPrincipal> convertPrincipal, bool disableTokenAuthenticationChallenge = false)
		{
			AddAuthentication(AuthenticationType.Token);
			TokenAuthenticationOptions = options ?? throw new ArgumentNullException(nameof(options));
			DisableTokenAuthenticationChallenge = disableTokenAuthenticationChallenge;
			ConvertTokenPrincipal = convertPrincipal ?? throw new ArgumentNullException(nameof(convertPrincipal));
			OnValidateTokenPrincipal = context =>
			{
				if (context.Principal != null && context.Principal.Identity != null)
					context.Principal = ConvertTokenPrincipal(context.Principal);

				return Task.FromResult(0);
			};

			return this;
		}

		public AuthenticationOptions SetRequestAuthentication(RequestAuthenticationOptions options, Func<RequestValidatePrincipalContext, Task> onValidatePrincipal)
		{
			AddAuthentication(AuthenticationType.Request);
			RequestAuthenticationOptions = options ?? throw new ArgumentNullException(nameof(options));
			OnValidateRequestPrincipal = onValidatePrincipal ?? throw new ArgumentNullException(nameof(onValidatePrincipal));
			return this;
		}

		public AuthenticationOptions SetRequestAuthentication(Func<HttpContext, string, ILogger, Task<RaiderPrincipal?>> createPrincipal, bool disableRequestAuthenticationChallenge = false, List<string>? anonymousUrlPathPrefixes = null)
		{
			AddAuthentication(AuthenticationType.Request);
			RequestAuthenticationOptions = new RequestAuthenticationOptions { DisableAuthenticationChallenge = disableRequestAuthenticationChallenge, AnonymousUrlPathPrefixes = anonymousUrlPathPrefixes?.Select(x => x.ToLowerInvariant()).ToList() };
			CreateRequestPrincipal = createPrincipal ?? throw new ArgumentNullException(nameof(createPrincipal));
			OnValidateRequestPrincipal = async context =>
			{
				if (context.HttpContext?.User != null && context.HttpContext.User.Identity != null)
					context.Principal = await CreateRequestPrincipal(context.HttpContext, context.Scheme.Name, context.Logger);
			};

			return this;
		}

		public AuthenticationOptions SetAuthenticationFlowFallback(AuthenticationType fallback)
		{
			AuthenticationFlowFallback = fallback;
			return this;
		}

		public override void Validate()
        {
			WindowsAuthenticationOptions?.Validate();
			CookieAuthenticationOptions?.Validate();
			TokenAuthenticationOptions?.Validate();
			RequestAuthenticationOptions?.Validate();
		}

		private void AddAuthentication(AuthenticationType authenticationType)
		{
			if (_authenticationFlow.Contains(authenticationType))
				throw new InvalidOperationException($"{authenticationType} authentication is already set.");

			_authenticationFlow.Add(authenticationType);
		}

		internal void SetAuthenticationFlow()
		{
			if (AuthenticationFlow != null)
				throw new InvalidOperationException($"{AuthenticationFlow} is already set.");

			AuthenticationFlow = _authenticationFlow;
		}
    }
}

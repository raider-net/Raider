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
		public Func<HttpContext, string, ILogger, RaiderPrincipal> CreateWindowsPrincipal { get; private set; }

		public CookieAuthenticationOptions CookieAuthenticationOptions { get; private set; }
		public string CookieName { get; private set; }
		public bool UseCookieAuthentication => CookieAuthenticationOptions != null;
		public Func<CookieValidatePrincipalContext, Task> OnValidateCookiePrincipal { get; private set; }

		//HttpContext context, string userName, string authenticationSchemeType, ILogger logger
		public Func<HttpContext, string, string, ILogger, Task<RaiderPrincipal?>> RecreateCookiePrincipal { get; private set; }
		public Func<ClaimsPrincipal, RaiderPrincipal> ConvertCookiePrincipal { get; private set; }

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
		public Func<HttpContext, string, ILogger, RaiderPrincipal> CreateRequestPrincipal { get; private set; }

		public AuthenticationOptions SetWindowsAuthentication(WindowsAuthenticationOptions options, Func<WindowsValidatePrincipalContext, Task> onValidatePrincipal)
		{
			AddAuthentication(AuthenticationType.WindowsIntegrated);
			WindowsAuthenticationOptions = options ?? throw new ArgumentNullException(nameof(options));
			OnValidateWindowsPrincipal = onValidatePrincipal ?? throw new ArgumentNullException(nameof(onValidatePrincipal));
			return this;
		}

		public AuthenticationOptions SetWindowsAuthentication(Func<HttpContext, string, ILogger, RaiderPrincipal> createPrincipal)
		{
			AddAuthentication(AuthenticationType.WindowsIntegrated);
			WindowsAuthenticationOptions = new WindowsAuthenticationOptions();
			CreateWindowsPrincipal = createPrincipal ?? throw new ArgumentNullException(nameof(createPrincipal));
			OnValidateWindowsPrincipal = context =>
			{
				if (context.HttpContext?.User != null && context.HttpContext.User.Identity != null)
					context.Principal = CreateWindowsPrincipal(context.HttpContext, context.Scheme.Name, context.Logger);
				
				return Task.FromResult<object>(null);
			};

			return this;
		}

		public AuthenticationOptions SetCookieAuthentication(Func<CookieValidatePrincipalContext, Task> onValidatePrincipal)
			=> SetCookieAuthentication(new CookieAuthenticationOptions(), onValidatePrincipal);

		public AuthenticationOptions SetCookieAuthenticationReplacePrincipal(Func<HttpContext, string, string, ILogger, Task<RaiderPrincipal?>> recreatePrincipal)
			=> SetCookieAuthenticationReplacePrincipal(new CookieAuthenticationOptions(), recreatePrincipal);

		public AuthenticationOptions SetCookieAuthenticationConvertPrincipal(Func<ClaimsPrincipal, RaiderPrincipal> convertPrincipal)
			=> SetCookieAuthenticationConvertPrincipal(new CookieAuthenticationOptions(), convertPrincipal);

		public AuthenticationOptions SetCookieAuthentication(CookieAuthenticationOptions options, Func<CookieValidatePrincipalContext, Task> onValidatePrincipal)
		{
			AddAuthentication(AuthenticationType.Cookie);
			CookieAuthenticationOptions = options ?? throw new ArgumentNullException(nameof(options));
			OnValidateCookiePrincipal = onValidatePrincipal ?? throw new ArgumentNullException(nameof(onValidatePrincipal));
			return this;
		}

		public AuthenticationOptions SetCookieAuthenticationReplacePrincipal(CookieAuthenticationOptions options, Func<HttpContext, string, string, ILogger, Task<RaiderPrincipal?>> recreatePrincipal)
		{
			AddAuthentication(AuthenticationType.Cookie);
			CookieAuthenticationOptions = options ?? throw new ArgumentNullException(nameof(options));
			RecreateCookiePrincipal = recreatePrincipal ?? throw new ArgumentNullException(nameof(recreatePrincipal));
			OnValidateCookiePrincipal = async context =>
			{
				if (context.Principal != null && context.Principal.Identity != null)
					context.Principal = await RecreateCookiePrincipal(context.HttpContext, context.Principal?.Identity?.Name, context.Scheme.Name, null);
			};

			return this;
		}

		public AuthenticationOptions SetCookieAuthenticationConvertPrincipal(CookieAuthenticationOptions options, Func<ClaimsPrincipal, RaiderPrincipal> convertPrincipal)
		{
			AddAuthentication(AuthenticationType.Cookie);
			CookieAuthenticationOptions = options ?? throw new ArgumentNullException(nameof(options));
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
			CookieName = cookieName;
			return this;
		}

		public AuthenticationOptions SetTokenAuthentication(TokenValidationParameters tokenValidationParameters, Func<TokenValidatedContext, Task> onValidatePrincipal)
		{
			return SetTokenAuthentication(
				new JwtBearerOptions
				{
					TokenValidationParameters = tokenValidationParameters
				},
				onValidatePrincipal);
		}

		public AuthenticationOptions SetTokenAuthenticationReplacePrincipal(TokenValidationParameters tokenValidationParameters, Func<HttpContext, string, string, ILogger, RaiderPrincipal> recreatePrincipal)
		{
			return SetTokenAuthenticationReplacePrincipal(
				new JwtBearerOptions
				{
					TokenValidationParameters = tokenValidationParameters
				},
				recreatePrincipal);
		}

		public AuthenticationOptions SetTokenAuthenticationConvertPrincipal(TokenValidationParameters tokenValidationParameters, Func<ClaimsPrincipal, RaiderPrincipal> convertPrincipal)
		{
			return SetTokenAuthenticationConvertPrincipal(
				new JwtBearerOptions
				{
					TokenValidationParameters = tokenValidationParameters
				},
				convertPrincipal);
		}

		public AuthenticationOptions SetTokenAuthentication(JwtBearerOptions options, Func<TokenValidatedContext, Task> onValidatePrincipal)
		{
			AddAuthentication(AuthenticationType.Token);
			TokenAuthenticationOptions = options ?? throw new ArgumentNullException(nameof(options));
			OnValidateTokenPrincipal = onValidatePrincipal ?? throw new ArgumentNullException(nameof(onValidatePrincipal));
			return this;
		}

		public AuthenticationOptions SetTokenAuthenticationReplacePrincipal(JwtBearerOptions options, Func<HttpContext, string, string, ILogger, RaiderPrincipal> recreatePrincipal)
		{
			AddAuthentication(AuthenticationType.Token);
			TokenAuthenticationOptions = options ?? throw new ArgumentNullException(nameof(options));
			RecreateTokenPrincipal = recreatePrincipal ?? throw new ArgumentNullException(nameof(recreatePrincipal));
			OnValidateTokenPrincipal = context =>
			{
				if (context.Principal != null && context.Principal.Identity != null)
					context.Principal = RecreateTokenPrincipal(context.HttpContext, context.Principal?.Identity?.Name, context.Scheme.Name, null);

				return Task.FromResult(0);
			};

			return this;
		}

		public AuthenticationOptions SetTokenAuthenticationConvertPrincipal(JwtBearerOptions options, Func<ClaimsPrincipal, RaiderPrincipal> convertPrincipal)
		{
			AddAuthentication(AuthenticationType.Token);
			TokenAuthenticationOptions = options ?? throw new ArgumentNullException(nameof(options));
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

		public AuthenticationOptions SetRequestAuthentication(Func<HttpContext, string, ILogger, RaiderPrincipal> createPrincipal)
		{
			AddAuthentication(AuthenticationType.Request);
			RequestAuthenticationOptions = new RequestAuthenticationOptions();
			CreateRequestPrincipal = createPrincipal ?? throw new ArgumentNullException(nameof(createPrincipal));
			OnValidateRequestPrincipal = context =>
			{
				if (context.HttpContext?.User != null && context.HttpContext.User.Identity != null)
					context.Principal = CreateRequestPrincipal(context.HttpContext, context.Scheme.Name, context.Logger);

				return Task.FromResult<object>(null);
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

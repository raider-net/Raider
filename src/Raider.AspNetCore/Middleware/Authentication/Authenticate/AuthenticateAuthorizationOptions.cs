using System;
using System.Security.Claims;

namespace Raider.AspNetCore.Middleware.Authentication.Authenticate
{
	public class AuthenticateAuthorizationOptions
	{
		public Action<ClaimsPrincipal, AuthenticateAuthorizationRequirement, Type> OnSuccess { get; set; }
		public Action<ClaimsPrincipal, AuthenticateAuthorizationRequirement, Type> OnFail { get; set; }
	}
}

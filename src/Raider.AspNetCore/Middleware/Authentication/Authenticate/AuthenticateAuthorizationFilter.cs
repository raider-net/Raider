using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Raider.Identity;
using System;
using System.Threading.Tasks;

namespace Raider.AspNetCore.Middleware.Authentication.Authenticate
{
	public class AuthenticateAuthorizationFilter : Attribute, IAsyncAuthorizationFilter
	{
		private readonly IAuthorizationService _authService;
		private readonly AuthenticateAuthorizationRequirement _requirement;

		public AuthenticateAuthorizationFilter(IAuthorizationService authService, AuthenticateAuthorizationRequirement requirement)
		{
			_authService = authService;
			_requirement = requirement;
		}

		public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
		{
			if (context.HttpContext.User is RaiderPrincipal principal)
			{
				AuthorizationResult result = await _authService.AuthorizeAsync(principal, null, _requirement);

				if (!result.Succeeded)
				{
					context.Result = new ForbidResult(Authentication.AuthenticationDefaults.AuthenticationScheme);
					//context.HttpContext.ThrowAccessDenied403Forbidden();
				}
			}
			else
			{
				context.Result = new ChallengeResult(Authentication.AuthenticationDefaults.AuthenticationScheme);
			}
		}
	}
}

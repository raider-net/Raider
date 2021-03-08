using Raider.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Raider.AspNetCore.Middleware.Authorization
{
	public class PermissionAuthorizationFilter : Attribute, IAsyncAuthorizationFilter, IAsyncActionFilter
	{
        private readonly IAuthorizationService _authService;
        private readonly PermissionAuthorizationRequirement _requirement;

        public PermissionAuthorizationFilter(IAuthorizationService authService, PermissionAuthorizationRequirement requirement)
        {
            _authService = authService;
            _requirement = requirement;
        }

		public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
		{
			var controllerActionDescriptor = (ControllerActionDescriptor)context.ActionDescriptor;
			var premission = _requirement?.Tokens?.Select(x => x.ToString()).ToList();

			if (premission != null && 0 < premission.Count)
				context.HttpContext.Items[Raider.AspNetCore.Defaults.Keys.Premission] = premission;

			await next();
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

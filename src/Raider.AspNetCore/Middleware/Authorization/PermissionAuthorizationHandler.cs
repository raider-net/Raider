using Raider.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using System.Linq;
using System.Threading.Tasks;

namespace Raider.AspNetCore.Middleware.Authorization
{
	public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionAuthorizationRequirement>
	{
		private readonly PermissionAuthorizationOptions _options;

		public PermissionAuthorizationHandler(IOptions<PermissionAuthorizationOptions> options)
		{
			_options = options?.Value;
		}

        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            PermissionAuthorizationRequirement requirement)
        {
			if (context.User is RaiderPrincipal principal)
			{
				if (principal.HasAnyPermissionClaim(
						requirement
							.Tokens
							.Where(x => x != null)
							.Select(x => x.ToString())
							.ToArray()))
				{
					context.Succeed(requirement);
					if (_options != null)
					{
						try
						{
							_options.OnSuccess?.Invoke(context.User, requirement, this.GetType());
						}
						catch { }
					}
				}
				else if (_options != null)
					{
						try
						{
							_options.OnFail?.Invoke(context.User, requirement, this.GetType());
						}
						catch { }
					}
			}

			return Task.CompletedTask;
        }
    }
}

using Raider.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;

namespace Raider.AspNetCore.Middleware.Authentication.Authenticate
{
	public class AuthenticateAuthorizationHandler : AuthorizationHandler<AuthenticateAuthorizationRequirement>
	{
		private readonly AuthenticateAuthorizationOptions _options;

		public AuthenticateAuthorizationHandler(IOptions<AuthenticateAuthorizationOptions> options)
		{
			_options = options?.Value;
		}

		protected override Task HandleRequirementAsync(
			AuthorizationHandlerContext context,
			AuthenticateAuthorizationRequirement requirement)
		{
			if (context.User is RaiderPrincipal)
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

			return Task.CompletedTask;
		}
	}
}

using Microsoft.AspNetCore.Authorization;

namespace Raider.AspNetCore.Middleware.Authorization
{
    public class PermissionAuthorizationRequirement : IAuthorizationRequirement
    {
        public object[] Tokens { get; set; }

        public PermissionAuthorizationRequirement(object[] tokens)
        {
			Tokens = tokens;
        }
    }
}

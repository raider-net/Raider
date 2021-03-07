using Microsoft.AspNetCore.Authorization;

namespace Raider.AspNetCore.Middleware.Authorization
{
    public class ActivityAuthorizationRequirement : IAuthorizationRequirement
    {
        public object[] Tokens { get; set; }

        public ActivityAuthorizationRequirement(object[] tokens)
        {
			Tokens = tokens;
        }
    }
}

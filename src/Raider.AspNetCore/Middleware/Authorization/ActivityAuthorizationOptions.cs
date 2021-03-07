using System;
using System.Security.Claims;

namespace Raider.AspNetCore.Middleware.Authorization
{
	public class ActivityAuthorizationOptions
	{
		public Action<ClaimsPrincipal, ActivityAuthorizationRequirement, Type> OnSuccess { get; set; }
		public Action<ClaimsPrincipal, ActivityAuthorizationRequirement, Type> OnFail { get; set; }

		public ActivityAuthorizationOptions()
		{

		}
	}
}

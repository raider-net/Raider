using System;
using System.Security.Claims;

namespace Raider.AspNetCore.Middleware.Authorization
{
	public class PermissionAuthorizationOptions
	{
		public Action<ClaimsPrincipal, PermissionAuthorizationRequirement, Type> OnSuccess { get; set; }
		public Action<ClaimsPrincipal, PermissionAuthorizationRequirement, Type> OnFail { get; set; }

		public PermissionAuthorizationOptions()
		{

		}
	}
}

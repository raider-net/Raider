using Raider.AspNetCore.Middleware.Authentication.RequestAuth.Events;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;

namespace Raider.AspNetCore.Middleware.Authentication.RequestAuth
{
	public class RequestAuthenticationOptions : AuthenticationSchemeOptions
	{
		public PathString? AccessDeniedPath { get; set; }
		public PathString? UnauthorizedPath { get; set; }
		public string? ReturnUrlParameter { get; set; }

		public new RequestAuthenticationEvents Events
		{
			get => (RequestAuthenticationEvents)base.Events;
			set => base.Events = value;
		}

		public RequestAuthenticationOptions()
		{
		}
	}
}

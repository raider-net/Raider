using Raider.AspNetCore.Middleware.Authentication.RequestAuth.Events;
using Microsoft.AspNetCore.Authentication;

namespace Raider.AspNetCore.Middleware.Authentication.RequestAuth
{
	public class RequestAuthenticationOptions : AuthenticationSchemeOptions
	{
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

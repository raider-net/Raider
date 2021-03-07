using Raider.AspNetCore.Middleware.Authentication.Events;
using Microsoft.AspNetCore.Authentication;

namespace Raider.AspNetCore.Middleware.Authentication
{
	public class WindowsAuthenticationOptions : AuthenticationSchemeOptions
	{
		public bool AllowStaticLogin { get; set; }

		public new WindowsAuthenticationEvents Events
		{
			get => (WindowsAuthenticationEvents)base.Events;
			set => base.Events = value;
		}

		public WindowsAuthenticationOptions()
		{
		}
	}
}

namespace Raider.AspNetCore.Middleware.Authentication
{
	public static class AuthenticationDefaults
	{
		public const string DefaultAuthenticationScheme = "RaiderAuth";
		public static string AuthenticationScheme { get; set; } = DefaultAuthenticationScheme;
	}
}

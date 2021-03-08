using Raider.Identity;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Security.Claims;

namespace Raider.AspNetCore.Middleware.Authentication
{
	public class DummyClaimsIdentity<TIdentity>
		where TIdentity : struct
	{
		public TIdentity Identifier { get; set; }
		public string Name { get; set; }
		public string DisplayName { get; set; }
		public object UserData { get; }

		public DummyClaimsIdentity()
		{

		}

		public RaiderPrincipal CreateRaiderPrincipal(HttpContext context, string authenticationSchemeType)
		{
			var claimsIdentity = new ClaimsIdentity(authenticationSchemeType);
			claimsIdentity.AddClaim(new Claim(ClaimTypes.Name, Name));

			var identity = new RaiderIdentity<TIdentity>(
				claimsIdentity,
				Identifier,
				Name,
				DisplayName,
				UserData,
				new List<string> { "Role1" },
				new List<int> { 1 },
				new List<string>() { "action1" },
				new List<int> { 1 },
				false,
				false);

			return new RaiderPrincipal<TIdentity>(identity);
		}
	}
}

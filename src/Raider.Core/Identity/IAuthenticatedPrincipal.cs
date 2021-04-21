namespace Raider.Identity
{
	public interface IAuthenticatedPrincipal
	{
		RaiderPrincipal<int>? Principal { get; set; }
		RaiderIdentity<int>? User => Principal?.IdentityBase;
	}

	public class AuthenticatedPrincipal : IAuthenticatedPrincipal
	{
		public RaiderPrincipal<int>? Principal { get; set; }
	}
}

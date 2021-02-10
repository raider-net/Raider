namespace Raider.EntityFrameworkCore.Audit
{
	public enum DbOperation
	{
		None = 0,
		Create = 1,
		Update = 2,
		Delete = 3,
		Read = 4
	}
}

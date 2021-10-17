namespace Raider.Generator.Compilation
{
	public interface IPermission
	{
		int IdPermission { get; set; }
		string Name { get; set; }
		string? Description { get; set; }
		bool IsSystemPermission { get; set; }
	}
}

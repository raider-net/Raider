namespace Raider.Generator.Compilation
{
	public interface IPermission
	{
		public int IdPermission { get; set; }
		public string Name { get; set; }
		public string? Description { get; set; }
	}
}

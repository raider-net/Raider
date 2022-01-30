namespace Raider.Reflection.Loader
{
	public enum UseHostApplicationAssembliesEnum
	{
		/// <summary>
		/// Never use user host application's assemblies
		/// </summary>
		Never,

		/// <summary>
		/// Only use the listed hosted application assemblies
		/// </summary>
		Selected,

		/// <summary>
		/// Always try to use host application's assemblies
		/// </summary>
		Always,

		/// <summary>
		/// Prefer external referenced assemblies, fallback to host application's assemblies
		/// </summary>
		PreferExternal
	}
}

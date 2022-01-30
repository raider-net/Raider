namespace Raider.Plugins.Options
{
	public class AssemblyPluginCatalogOptions
	{
		/// <summary>
		/// Gets or sets the <see cref="PluginLoadContextOptions"/>.
		/// </summary>
		public PluginLoadContextOptions? PluginLoadContextOptions { get; set; } = new PluginLoadContextOptions();

		/// <summary>
		/// Gets or sets how the plugin names and version should be defined. <seealso cref="PluginNameOptions"/>.
		/// </summary>
		public PluginNameOptions? PluginNameOptions { get; set; } = new PluginNameOptions();

		/// <summary>
		/// Gets or sets the <see cref="TypeFinderOptions"/>. 
		/// </summary>
		public TypeFinderOptions? TypeFinderOptions { get; set; } = new TypeFinderOptions();
	}
}

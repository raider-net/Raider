using Raider.Reflection.Loader;

namespace Raider.Plugins.Options
{
	/// <summary>
	/// Options for configuring how the <see cref="TypePluginRepositoryOptions"/> works.
	/// </summary>
	public class TypePluginRepositoryOptions
	{
		/// <summary>
		/// Gets or sets how the plugin names and version should be defined. <seealso cref="PluginNameOptions"/>.
		/// </summary>
		public PluginNameOptions? PluginNameOptions { get; set; } = new PluginNameOptions();

		/// <summary>
		/// Gets or sets the <see cref="TypeFinderOptions"/>. 
		/// </summary>
		public TypeFinderOptions? TypeFinderOptions { get; set; } = new TypeFinderOptions();

		/// <summary>
		/// Gets or sets the <see cref="ITypeFindingContext"/>.
		/// </summary>
		public ITypeFindingContext? TypeFindingContext { get; set; } = null;
	}
}

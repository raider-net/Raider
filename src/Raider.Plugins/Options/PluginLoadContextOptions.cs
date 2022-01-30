using Raider.Reflection.Loader;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Raider.Plugins.Options
{
	/// <summary>
	/// Options for AssemblyLoadContext
	/// </summary>
	public class PluginLoadContextOptions
	{
		/// <summary>
		/// Gets or sets if the plugin should by default to use the assemblies referenced by the plugin or by the host application. Useful in situations where it is important that the host application
		/// and the plugin use the same version of the assembly, even if they reference different versions.
		/// </summary>
		public UseHostApplicationAssembliesEnum UseHostApplicationAssemblies { get; set; } = UseHostApplicationAssembliesEnum.Always;

		/// <summary>
		/// Gets or sets the assemblies which the plugin should use if UseHostApplicationAssemblies is set to Selected. These assemblies are used
		/// even if the plugin itself references an another version of the same assembly.
		/// </summary>
		public List<AssemblyName> HostApplicationAssemblies { get; set; } = new List<AssemblyName>();

		/// <summary>
		/// Gets or sets the function which logs debug message
		/// </summary>
		public Action<string, object[]> LogDebug { get; set; } = (msg, args) => { };

		/// <summary>
		/// Gets or sets the function which logs warning message
		/// </summary>
		public Action<string, object[]> LogWarning { get; set; } = (msg, args) => { };

		/// <summary>
		/// Gets or sets the additional runtime paths which are used when locating plugin assemblies  
		/// </summary>
		public List<string> AdditionalRuntimePaths { get; set; } = Defaults.AdditionalRuntimePaths;

		/// <summary>
		/// Gets or sets a list of assemblies and paths which can be used to override default assembly loading. Useful in situations where in runtime we want to load a DLL from a separate location.
		/// </summary>
		public List<RuntimeAssemblyHint> RuntimeAssemblyHints { get; set; } = new List<RuntimeAssemblyHint>();

		public static class Defaults
		{
			/// <summary>
			/// Gets or sets the additional runtime paths which are used when locating plugin assemblies  
			/// </summary>
			public static List<string> AdditionalRuntimePaths { get; set; } = new List<string>();
		}
	}
}

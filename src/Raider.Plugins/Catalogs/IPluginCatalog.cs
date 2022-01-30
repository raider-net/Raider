﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Raider.Plugins.Catalogs
{
	/// <summary>
	/// Represents a single Plugin Catalog. Can contain 0-n plugins.
	/// </summary>
	public interface IPluginCatalog
	{
		/// <summary>
		/// Initializes the catalog
		/// </summary>
		Task Initialize();

		/// <summary>
		/// Gets if the catalog is initialized
		/// </summary>
		bool IsInitialized { get; }

		/// <summary>
		/// Gets all the plugins
		/// </summary>
		/// <returns>List of <see cref="Plugin"/></returns>
		List<Plugin> GetPlugins();

		/// <summary>
		/// Gets a single plugin based on its name and version
		/// </summary>
		/// <returns>The <see cref="Plugin"/></returns>
		Plugin? Get(string name, Version version);

		List<Type> FindTypes(TypeFinderCriteria typeFinderCriteria);
	}
}

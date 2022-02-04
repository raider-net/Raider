﻿using System;
using System.Collections.Generic;

namespace Raider.Plugins.Options
{
	/// <summary>
	/// Options for configuring how the <see cref="FolderPluginCatalog"/> works.
	/// </summary>
	public class FolderPluginCatalogOptions
	{
		public string? CurrentDirectory { get; set; } = Environment.CurrentDirectory;

		public string? FolderPath { get; set; }

		/// <summary>
		/// Gets or sets if subfolders should be included. Defaults to true.
		/// </summary>
		public bool IncludeSubfolders { get; set; } = true;

		/// <summary>
		/// Gets or sets the search patterns when locating plugins. By default only located dll-files.
		/// </summary>
		public List<string>? SearchPatterns { get; set; } = new List<string>() { "*.dll" };

		/// <summary>
		/// Gets or sets the <see cref="PluginLoadContextOptions"/>.
		/// </summary>
		public PluginLoadContextOptions? PluginLoadContextOptions { get; set; } = new PluginLoadContextOptions();

		/// <summary>
		/// Gets or sets the <see cref="TypeFinderOptions"/>.
		/// </summary>
		public TypeFinderOptions? TypeFinderOptions { get; set; } = new TypeFinderOptions();

		/// <summary>
		/// Gets or sets how the plugin names and version should be defined. <seealso cref="PluginNameOptions"/>
		/// </summary>
		public PluginNameOptions? PluginNameOptions { get; set; } = new PluginNameOptions();
	}
}

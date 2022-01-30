using Raider.Extensions;
using Raider.Threading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Raider.Plugins.Catalogs
{
	/// <summary>
	/// Composite Plugin Catalog combines 1-n other Plugin Catalogs. 
	/// </summary>
	public class CompositePluginCatalog : IPluginCatalog
	{
		private readonly List<IPluginCatalog> _catalogs;

		/// <inheritdoc />

		public CompositePluginCatalog(params IPluginCatalog[] catalogs)
		{
			_catalogs = catalogs.ToList();
		}

		public void AddCatalog(IPluginCatalog catalog)
			=> _catalogs.AddUniqueItem(catalog);

		public bool IsInitialized { get; private set; }
		private readonly AsyncLock _initLock = new();
		/// <inheritdoc />
		public async Task Initialize()
		{
			if (IsInitialized)
				return;

			using (await _initLock.LockAsync())
			{
				if (IsInitialized)
					return;

				if (_catalogs?.Any() != true)
				{
					IsInitialized = true;
					return;
				}

				foreach (var pluginCatalog in _catalogs)
					await pluginCatalog.Initialize();

				IsInitialized = true;
			}
		}

		private List<Plugin>? _plugins;
		/// <inheritdoc />
		public List<Plugin> GetPlugins()
		{
			if (!IsInitialized)
				throw new InvalidOperationException($"Not initialized {nameof(CompositePluginCatalog)}");

			if (_plugins != null)
				return _plugins;

			var result = new List<Plugin>();

			foreach (var pluginCatalog in _catalogs)
			{
				var pluginsInCatalog = pluginCatalog.GetPlugins();
				result.AddRange(pluginsInCatalog);
			}

			return _plugins ??= result;
		}

		/// <inheritdoc />
		public Plugin? Get(string name, Version version)
		{
			if (!IsInitialized)
				throw new InvalidOperationException($"Not initialized {nameof(CompositePluginCatalog)}");

			foreach (var pluginCatalog in _catalogs)
			{
				var plugin = pluginCatalog.Get(name, version);

				if (plugin == null)
					continue;

				return plugin;
			}

			return null;
		}

		public List<Type> FindTypes(TypeFinderCriteria typeFinderCriteria)
		{
			if (typeFinderCriteria == null)
				throw new ArgumentNullException(nameof(typeFinderCriteria));

			var types = new List<Type>();
			foreach (var pluginCatalog in _catalogs)
			{
				var tps = pluginCatalog.FindTypes(typeFinderCriteria);

				if (tps == null || tps.Count == 0)
					continue;

				types.AddUniqueRange(tps);
			}

			return types;
		}
	}
}

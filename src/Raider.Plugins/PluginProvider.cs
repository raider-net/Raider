using Raider.Plugins.Catalogs;
using Raider.Plugins.Extensions;
using Raider.Plugins.Internal;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Raider.Plugins
{
	public class PluginProvider
	{
		private static readonly ConcurrentDictionary<Type, List<Plugin>> _plugins = new();
		private readonly IEnumerable<IPluginCatalog> _catalogs;
		private readonly IServiceProvider _serviceProvider;

		public PluginProvider(IEnumerable<IPluginCatalog> catalogs, IServiceProvider serviceProvider)
		{
			_catalogs = catalogs ?? throw new ArgumentNullException(nameof(catalogs));
			_serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
		}

		public List<Plugin> GetByTag(string tag)
			=> string.IsNullOrWhiteSpace(tag)
				? new List<Plugin>()
				: _catalogs.SelectMany(x => x.GetByTag(tag)).ToList();

		private List<Plugin>? _allPlugins;
		public List<Plugin> GetAllPlugins()
			=> _allPlugins ??= _catalogs.SelectMany(x => x.GetPlugins()).ToList();

		public Plugin? Get(string name, Version version)
		{
			foreach (var pluginCatalog in _catalogs)
			{
				var result = pluginCatalog.Get(name, version);

				if (result != null)
					return result;
			}

			return null;
		}

		public List<T> GetPluginInstances<T>()
			where T : class
		{
			var type = typeof(T);
			var plugins =_plugins.GetOrAdd(type, t => GetAllPlugins().Where(x => type.IsAssignableFrom(x.Type)).ToList());

			var result = new List<T>();

			foreach (var plugin in plugins)
			{
				var pluginInstance = plugin.Create<T>(_serviceProvider);

				if (pluginInstance != null)
					result.Add(pluginInstance);
			}

			return result;
		}
	}
}

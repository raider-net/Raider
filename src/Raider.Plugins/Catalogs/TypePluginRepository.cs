using Raider.Plugins.Options;
using Raider.Reflection.Loader;
using Raider.Threading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Raider.Plugins.Catalogs
{
	/// <summary>
	/// Plugin Catalog for a single .NET Type.
	/// </summary>
	internal class TypePluginRepository : IPluginCatalog
	{
		private readonly Type _pluginType;
		private readonly TypePluginRepositoryOptions _options;
		private Plugin? _typePlugin;

		/// <summary>
		/// Gets the <see cref="TypePluginRepositoryOptions"/> for this catalog
		/// </summary>
		public TypePluginRepositoryOptions Options => _options;

		/// <inheritdoc />

		public TypePluginRepository(Type pluginType)
			: this(pluginType, null, null, null)
		{
		}

		public TypePluginRepository(Type pluginType, PluginNameOptions nameOptions)
			: this(pluginType, null, nameOptions, null)
		{
		}

		public TypePluginRepository(Type pluginType, Action<PluginNameOptions> configure)
			: this(pluginType, configure, null, null)
		{
		}

		public TypePluginRepository(Type pluginType, TypePluginRepositoryOptions options)
			: this(pluginType, null, null, options)
		{
		}

		public TypePluginRepository(Type pluginType, Action<PluginNameOptions>? configure, PluginNameOptions? nameOptions, TypePluginRepositoryOptions? options)
		{
			_pluginType = pluginType ?? throw new ArgumentNullException(nameof(pluginType));
			_options = options ?? new TypePluginRepositoryOptions();

			if (_options.TypeFindingContext == null)
				_options.TypeFindingContext = new PluginAssemblyLoadContext(pluginType.Assembly);

			if (_options.TypeFinderOptions == null)
				_options.TypeFinderOptions = new TypeFinderOptions();

			if (_options.TypeFinderOptions.TypeFinderCriterias?.Any() != true)
				_options.TypeFinderOptions.TypeFinderCriterias = new List<TypeFinderCriteria>();

			var naming = nameOptions ?? new PluginNameOptions();
			configure?.Invoke(naming);

			_options.PluginNameOptions = naming;
		}

		public bool IsInitialized { get; private set; }
		private readonly AsyncLock _initLock = new();
		public async Task Initialize()
		{
			if (IsInitialized)
				return;

			using (await _initLock.LockAsync())
			{
				if (IsInitialized)
					return;

				var namingOptions = _options.PluginNameOptions!;
				var version = namingOptions.PluginVersionGenerator(namingOptions, _pluginType);
				var pluginName = namingOptions.PluginNameGenerator(namingOptions, _pluginType);
				var description = namingOptions.PluginDescriptionGenerator(namingOptions, _pluginType);
				var productVersion = namingOptions.PluginProductVersionGenerator(namingOptions, _pluginType);

				var tags = new List<string>();

				if (_options.TypeFinderOptions != null && _options.TypeFindingContext != null)
				{
					foreach (var typeFinderCriteria in _options.TypeFinderOptions.TypeFinderCriterias!)
					{
						var isMatch = TypeFinder.IsMatch(typeFinderCriteria, _pluginType, _options.TypeFindingContext);

						if (isMatch && typeFinderCriteria.Tags?.Any() == true)
							tags.AddRange(typeFinderCriteria.Tags);
					}
				}

				_typePlugin = new Plugin(_pluginType.Assembly, _pluginType, pluginName, version, this, description, productVersion, tags: tags);

				IsInitialized = true;
			}
		}

		private List<Plugin>? _plugins;
		/// <inheritdoc />
		public List<Plugin> GetPlugins()
			=> _plugins ??=
				(IsInitialized
					? (_typePlugin == null
						? new List<Plugin>()
						: new List<Plugin>() { _typePlugin })
					: throw new InvalidOperationException($"Not initialized {nameof(TypePluginRepository)}"));

		/// <inheritdoc />
		public Plugin? Get(string name, Version version)
		{
			if (!IsInitialized)
				throw new InvalidOperationException($"Not initialized {nameof(TypePluginRepository)}");

			if (_typePlugin == null)
				return null;

			if (!string.Equals(name, _typePlugin.Name, StringComparison.InvariantCultureIgnoreCase)
				|| version != _typePlugin.Version)
				return null;

			return _typePlugin;
		}

		public List<Type> FindTypes(TypeFinderCriteria typeFinderCriteria)
		{
			if (typeFinderCriteria == null)
				throw new ArgumentNullException(nameof(typeFinderCriteria));

			return TypeFinder.IsMatch(typeFinderCriteria, _pluginType)
				? new List<Type> { _pluginType }
				: new List<Type>();
		}
	}
}

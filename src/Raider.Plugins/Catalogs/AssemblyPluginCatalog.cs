using Microsoft.Extensions.DependencyInjection;
using Raider.Plugins.Options;
using Raider.Reflection.Loader;
using Raider.Threading;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Raider.Plugins.Catalogs
{
	public class AssemblyPluginCatalog : IPluginCatalog
	{
		private readonly string _assemblyPath;
		private readonly AssemblyPluginCatalogOptions _options;
		private readonly Assembly _assembly;
		private readonly PluginAssemblyLoadContext _assemblyLoader;

		private List<TypePluginRepository>? _typePluginRepositories = null;

		internal Assembly Assembly => _assembly;

		public AssemblyPluginCatalog(string assemblyPath)
			: this(assemblyPath, null, null, null)
		{
		}

		public AssemblyPluginCatalog(Assembly assembly)
			: this(null, assembly)
		{
		}

		public AssemblyPluginCatalog(string assemblyPath, AssemblyPluginCatalogOptions? options = null)
			: this(assemblyPath, null, null, null, null, null,
			options)
		{
		}

		public AssemblyPluginCatalog(Assembly assembly, AssemblyPluginCatalogOptions? options = null)
			: this(null, assembly, null, null, null, null, options)
		{
		}

		public AssemblyPluginCatalog(string assemblyPath, TypeFinderCriteria? criteria = null, AssemblyPluginCatalogOptions? options = null)
			: this(assemblyPath, null, null, null, null, criteria, options)
		{
		}

		public AssemblyPluginCatalog(Assembly assembly, TypeFinderCriteria? criteria = null, AssemblyPluginCatalogOptions? options = null)
			: this(null, assembly, null, null, null, criteria, options)
		{
		}

		public AssemblyPluginCatalog(string assemblyPath, Action<TypeFinderCriteriaBuilder>? configureFinder = null, AssemblyPluginCatalogOptions? options = null)
			: this(assemblyPath, null, null, null, configureFinder, null, options)
		{
		}

		public AssemblyPluginCatalog(Assembly assembly, Action<TypeFinderCriteriaBuilder>? configureFinder = null, AssemblyPluginCatalogOptions? options = null)
			: this(null, assembly, null, null, configureFinder, null, options)
		{
		}

		public AssemblyPluginCatalog(string assemblyPath, Predicate<Type>? filter = null, AssemblyPluginCatalogOptions? options = null)
			: this(assemblyPath, null, filter, null, null, null, options)
		{
		}

		public AssemblyPluginCatalog(Assembly assembly, Predicate<Type>? filter = null, AssemblyPluginCatalogOptions? options = null)
			: this(null, assembly, filter, null, null, null, options)
		{
		}

		public AssemblyPluginCatalog(string assemblyPath, Dictionary<string, Predicate<Type>> taggedFilters, AssemblyPluginCatalogOptions? options = null)
			: this(assemblyPath, null, null, taggedFilters, null, null, options)
		{
		}

		public AssemblyPluginCatalog(Assembly assembly, Dictionary<string, Predicate<Type>> taggedFilters, AssemblyPluginCatalogOptions? options = null)
			: this(null, assembly, null, taggedFilters, null, null, options)
		{
		}

		public AssemblyPluginCatalog(
			string? assemblyPath = null,
			Assembly? assembly = null,
			Predicate<Type>? filter = null,
			Dictionary<string, Predicate<Type>>? taggedFilters = null,
			Action<TypeFinderCriteriaBuilder>? configureFinder = null,
			TypeFinderCriteria? criteria = null,
			AssemblyPluginCatalogOptions? options = null)
		{
			if (assembly == null)
			{
				if (string.IsNullOrWhiteSpace(assemblyPath))
					throw new ArgumentNullException($"{nameof(assembly)} or {nameof(assemblyPath)}");

				_assemblyPath = assemblyPath;
			}
			else
			{
				_assembly = assembly;
				_assemblyPath = _assembly.Location;
			}

			_options = options ?? new AssemblyPluginCatalogOptions();

			SetFilters(filter, taggedFilters, criteria, configureFinder);

			_assemblyLoader = new PluginAssemblyLoadContext(_assemblyPath, _options.PluginLoadContextOptions);
			_assembly = _assemblyLoader.Load();
		}

		private void SetFilters(
			Predicate<Type>? filter,
			Dictionary<string, Predicate<Type>>? taggedFilters,
			TypeFinderCriteria? criteria,
			Action<TypeFinderCriteriaBuilder>? configureFinder)
		{
			if (_options.TypeFinderOptions == null)
				_options.TypeFinderOptions = new TypeFinderOptions();

			if (_options.TypeFinderOptions.TypeFinderCriterias == null)
				_options.TypeFinderOptions.TypeFinderCriterias = new List<TypeFinderCriteria>();

			if (filter != null)
			{
				var filterCriteria = new TypeFinderCriteria { Query = (context, type) => filter(type) };
				filterCriteria.Tags = new List<string>
				{
					string.Empty
				};

				_options.TypeFinderOptions.TypeFinderCriterias.Add(filterCriteria);
			}

			if (taggedFilters != null)
			{
				foreach (var taggedFilter in taggedFilters)
				{
					var taggedCriteria = new TypeFinderCriteria { Query = (context, type) => taggedFilter.Value(type) };
					taggedCriteria.Tags = new List<string>
					{
						taggedFilter.Key
					};

					_options.TypeFinderOptions.TypeFinderCriterias.Add(taggedCriteria);
				}
			}

			if (configureFinder != null)
			{
				var builder = new TypeFinderCriteriaBuilder();
				configureFinder(builder);

				var configuredCriteria = builder.Build();

				_options.TypeFinderOptions.TypeFinderCriterias.Add(configuredCriteria);
			}

			if (criteria != null)
				_options.TypeFinderOptions.TypeFinderCriterias.Add(criteria);

			if (_options.TypeFinderOptions.TypeFinderCriterias.Any() != true)
			{
				var findAll = TypeFinderCriteriaBuilder
					.Create()
					.Tag(string.Empty)
					.Build();

				_options.TypeFinderOptions.TypeFinderCriterias.Add(findAll);
			}
		}

		public bool IsInitialized { get; private set; }
		private readonly AsyncLock _initLock = new();
		public async Task Initialize()
		{
			if (!File.Exists(_assemblyPath))
				throw new ArgumentException($"Assembly in path {_assemblyPath} does not exist.");

			if (IsInitialized)
				return;

			using (await _initLock.LockAsync())
			{
				if (IsInitialized)
					return;

				_typePluginRepositories = new List<TypePluginRepository>();

				var handledPluginTypes = new List<Type>();
				foreach (var typeFinderCriteria in _options.TypeFinderOptions!.TypeFinderCriterias!)
				{
					var pluginTypes = TypeFinder.Find(typeFinderCriteria, _assembly, _assemblyLoader);

					foreach (var type in pluginTypes)
					{
						if (handledPluginTypes.Contains(type))
							continue;

						var typePluginRepository =
							new TypePluginRepository(
								type,
								new TypePluginRepositoryOptions()
								{
									PluginNameOptions = _options.PluginNameOptions ?? new PluginNameOptions(),
									TypeFindingContext = _assemblyLoader,
									TypeFinderOptions = _options.TypeFinderOptions
								});

						await typePluginRepository.Initialize();

						_typePluginRepositories.Add(typePluginRepository);

						handledPluginTypes.Add(type);
					}
				}

				IsInitialized = true;
			}
		}

		private List<Plugin>? _plugins;
		public List<Plugin> GetPlugins()
			=> _plugins ??=
				(IsInitialized
					? (_typePluginRepositories?.SelectMany(x => x.GetPlugins()).ToList() ?? new List<Plugin>())
					: throw new InvalidOperationException($"Not initialized {nameof(AssemblyPluginCatalog)}"));

		public Plugin? Get(string name, Version version)
		{
			if (!IsInitialized)
				throw new InvalidOperationException($"Not initialized {nameof(AssemblyPluginCatalog)}");

			if (_typePluginRepositories == null)
				return null;

			foreach (var pluginCatalog in _typePluginRepositories)
			{
				var foundPlugin = pluginCatalog.Get(name, version);

				if (foundPlugin == null)
					continue;

				return foundPlugin;
			}

			return null;
		}

		public List<Type> FindTypes(TypeFinderCriteria typeFinderCriteria)
		{
			if (typeFinderCriteria == null)
				throw new ArgumentNullException(nameof(typeFinderCriteria));

			var types = TypeFinder.Find(typeFinderCriteria, _assembly, _assemblyLoader);
			return types;
		}
	}
}

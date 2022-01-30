using Raider.Extensions;
using Raider.Plugins.Loader;
using Raider.Plugins.Options;
using Raider.Reflection.Loader;
using Raider.Threading;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.PortableExecutable;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using System.Threading.Tasks;

namespace Raider.Plugins.Catalogs
{
	/// <summary>
	/// Plugin folder for a single folder (including or excluding subfolders). Locates the plugins from the assemblies (by default this means dll-files).
	/// </summary>
	public class FolderPluginCatalog : IPluginCatalog
	{
		private readonly FolderPluginCatalogOptions _options;
		private readonly List<AssemblyPluginCatalog> _assemblyPluginCatalogs;

		/// <inheritdoc />

		public FolderPluginCatalog(string folderPath)
			: this(folderPath, null, null, new FolderPluginCatalogOptions())
		{
		}

		public FolderPluginCatalog(string folderPath, FolderPluginCatalogOptions options)
			: this(folderPath, null, null, options)
		{
		}

		public FolderPluginCatalog(string folderPath, Action<TypeFinderCriteriaBuilder> configureFinder)
			: this(folderPath, configureFinder, null, null)
		{
		}

		public FolderPluginCatalog(string folderPath, TypeFinderCriteria finderCriteria)
			: this(folderPath, null, finderCriteria, null)
		{
		}

		public FolderPluginCatalog(string folderPath, TypeFinderCriteria finderCriteria, FolderPluginCatalogOptions options)
			: this(folderPath, null, finderCriteria, options)
		{
		}

		public FolderPluginCatalog(string folderPath, Action<TypeFinderCriteriaBuilder> configureFinder, FolderPluginCatalogOptions options)
			: this(folderPath, configureFinder, null, options)
		{
		}

		public FolderPluginCatalog(
			string folderPath,
			Action<TypeFinderCriteriaBuilder>? configureFinder,
			TypeFinderCriteria? finderCriteria,
			FolderPluginCatalogOptions? options)
		{
			_assemblyPluginCatalogs = new List<AssemblyPluginCatalog>();

			_options = options ?? new FolderPluginCatalogOptions();

			if (string.IsNullOrWhiteSpace(_options.FolderPath))
				_options.FolderPath = folderPath;

			if (string.IsNullOrWhiteSpace(_options.FolderPath))
				throw new ArgumentNullException(nameof(folderPath));

			if (_options.TypeFinderOptions == null)
				_options.TypeFinderOptions = new TypeFinderOptions();

			if (_options.TypeFinderOptions.TypeFinderCriterias == null)
				_options.TypeFinderOptions.TypeFinderCriterias = new List<TypeFinderCriteria>();

			if (configureFinder != null)
			{
				var builder = new TypeFinderCriteriaBuilder();
				configureFinder(builder);

				var criteria = builder.Build();

				_options.TypeFinderOptions.TypeFinderCriterias.Add(criteria);
			}

			if (finderCriteria != null)
				_options.TypeFinderOptions.TypeFinderCriterias.Add(finderCriteria);

			if (_options.SearchPatterns == null || _options.SearchPatterns.Count == 0)
				_options.SearchPatterns = new List<string>() { "*.dll" };

			var foundFiles = new List<string>();

			foreach (var searchPattern in _options.SearchPatterns!)
			{
				var dllFiles = Directory.GetFiles(
					_options.FolderPath!,
					searchPattern,
					_options.IncludeSubfolders
						? SearchOption.AllDirectories
						: SearchOption.TopDirectoryOnly);

				foundFiles.AddRange(dllFiles);
			}

			foundFiles = foundFiles.Distinct().ToList();

			foreach (var assemblyPath in foundFiles)
			{
				// Assemblies are treated as readonly as long as possible
				var isPluginAssembly = IsPluginAssembly(assemblyPath);

				if (!isPluginAssembly)
					continue;

				var assemblyCatalogOptions = new AssemblyPluginCatalogOptions
				{
					PluginLoadContextOptions = _options.PluginLoadContextOptions ?? new PluginLoadContextOptions(),
					TypeFinderOptions = _options.TypeFinderOptions,
					PluginNameOptions = _options.PluginNameOptions ?? new PluginNameOptions()
				};

				// We are actually just delegating the responsibility from FolderPluginCatalog to AssemblyPluginCatalog. 
				var assemblyCatalog = new AssemblyPluginCatalog(assemblyPath, assemblyCatalogOptions);
				_assemblyPluginCatalogs.Add(assemblyCatalog);
			}
		}

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

				foreach (var assemblyCatalog in _assemblyPluginCatalogs)
					await assemblyCatalog.Initialize();

				IsInitialized = true;
			}
		}

		private bool IsPluginAssembly(string assemblyPath)
		{
			var hasMetadata = PluginAssemblyHasMetadata(assemblyPath);
			if (!hasMetadata)
				return false;

			if (_options.TypeFinderOptions?.TypeFinderCriterias?.Any() != true)
				return true;

			var runtimeDirectory = RuntimeEnvironment.GetRuntimeDirectory();
			var runtimeAssemblies = Directory.GetFiles(runtimeDirectory, "*.dll");
			var paths = new List<string>(runtimeAssemblies) { assemblyPath };

			if (_options.PluginLoadContextOptions!.AdditionalRuntimePaths?.Any() == true)
			{
				foreach (var additionalRuntimePath in _options.PluginLoadContextOptions.AdditionalRuntimePaths)
				{
					var dlls = Directory.GetFiles(additionalRuntimePath, "*.dll");
					paths.AddRange(dlls);
				}
			}

			if (_options.PluginLoadContextOptions.UseHostApplicationAssemblies == UseHostApplicationAssembliesEnum.Always)
			{
				var hostApplicationPath = Environment.CurrentDirectory;
				var hostDlls = Directory.GetFiles(hostApplicationPath, "*.dll", SearchOption.AllDirectories);

				paths.AddRange(hostDlls);

				AddSharedFrameworkDlls(hostApplicationPath, runtimeDirectory, paths);
			}
			else if (_options.PluginLoadContextOptions.UseHostApplicationAssemblies == UseHostApplicationAssembliesEnum.Never)
			{
				var pluginPath = Path.GetDirectoryName(assemblyPath);
				var dllsInPluginPath = Directory.GetFiles(pluginPath!, "*.dll", SearchOption.AllDirectories);

				paths.AddRange(dllsInPluginPath);
			}
			else if (_options.PluginLoadContextOptions.UseHostApplicationAssemblies == UseHostApplicationAssembliesEnum.Selected)
			{
				foreach (var hostApplicationAssembly in _options.PluginLoadContextOptions.HostApplicationAssemblies)
				{
					var assembly = Assembly.Load(hostApplicationAssembly);
					paths.Add(assembly.Location);
				}
			}

			paths = paths.Distinct().ToList();

			//If same dll is found from multiple locations, use the first found dll and remove the others.
			var duplicateDlls = paths.Select(x => new { FullPath = x, FileName = Path.GetFileName(x) }).GroupBy(x => x.FileName)
				.Where(x => x.Count() > 1)
				.ToList();

			var removed = new List<string>();

			foreach (var duplicateDll in duplicateDlls)
				foreach (var duplicateDllPath in duplicateDll.Skip(1))
					removed.Add(duplicateDllPath.FullPath);

			foreach (var re in removed)
				paths.Remove(re);

			var resolver = new PathAssemblyResolver(paths);

			// We use the metadata (readonly) versions of the assemblies before loading them
			using (var metadataContext = new MetadataLoadContext(resolver))
			{
				var metadataPluginLoadContext = new MetadataTypeFindingContext(metadataContext);
				var readonlyAssembly = metadataContext.LoadFromAssemblyPath(assemblyPath);

				var typeFinder = new TypeFinder();

				foreach (var finderCriteria in _options.TypeFinderOptions.TypeFinderCriterias)
				{
					var typesFound = TypeFinder.Find(finderCriteria, readonlyAssembly, metadataPluginLoadContext);

					if (typesFound?.Any() == true)
						return true;
				}
			}

			return false;
		}

		private static bool PluginAssemblyHasMetadata(string assemblyPath)
		{
			using var stream = File.OpenRead(assemblyPath);
			using var reader = new PEReader(stream);
			return reader.HasMetadata;
		}

		private void AddSharedFrameworkDlls(string hostApplicationPath, string runtimeDirectory, List<string> paths)
		{
			// If the main application references a shared framework (for example WinForms), we want to add these dlls also
			var defaultAssemblies = AssemblyLoadContext.Default.Assemblies.ToList();

			var defaultAssemblyDirectories =
				defaultAssemblies
					.Where(x => !x.IsDynamic && !string.IsNullOrWhiteSpace(x.Location))
					.GroupBy(x => Path.GetDirectoryName(x.Location)!)
					.Select(x => x.Key)
					.ToList();

			foreach (var assemblyDirectory in defaultAssemblyDirectories)
			{
				if (string.Equals(assemblyDirectory.TrimEnd('\\').TrimEnd('/'), hostApplicationPath.TrimEnd('\\').TrimEnd('/')))
				{
					continue;
				}

				if (string.Equals(assemblyDirectory.TrimEnd('\\').TrimEnd('/'), runtimeDirectory.TrimEnd('\\').TrimEnd('/')))
				{
					continue;
				}

				if (_options.PluginLoadContextOptions!.AdditionalRuntimePaths == null)
					_options.PluginLoadContextOptions.AdditionalRuntimePaths = new List<string>();

				if (!_options.PluginLoadContextOptions.AdditionalRuntimePaths.Contains(assemblyDirectory))
					_options.PluginLoadContextOptions.AdditionalRuntimePaths.Add(assemblyDirectory);

				var dlls = Directory.GetFiles(assemblyDirectory, "*.dll");
				paths.AddRange(dlls);
			}
		}

		private List<Plugin>? _plugins;
		/// <inheritdoc />
		public List<Plugin> GetPlugins()
			=> _plugins ??=
				(IsInitialized
					? _assemblyPluginCatalogs.SelectMany(x => x.GetPlugins()).ToList()
					: throw new InvalidOperationException($"Not initialized {nameof(FolderPluginCatalog)}"));

		/// <inheritdoc />
		public Plugin? Get(string name, Version version)
		{
			if (!IsInitialized)
				throw new InvalidOperationException($"Not initialized {nameof(FolderPluginCatalog)}");

			foreach (var assemblyPluginCatalog in _assemblyPluginCatalogs)
			{
				var plugin = assemblyPluginCatalog.Get(name, version);

				if (plugin == null)
					continue;

				return plugin;
			}

			return null;
		}

		internal List<AssemblyPluginCatalog> GetAssemblyPluginCatalogs()
			=> _assemblyPluginCatalogs.ToList();

		public List<Type> FindTypes(TypeFinderCriteria typeFinderCriteria)
		{
			if (typeFinderCriteria == null)
				throw new ArgumentNullException(nameof(typeFinderCriteria));

			var types = new List<Type>();
			foreach (var assemblyPluginCatalog in _assemblyPluginCatalogs)
			{
				var tps = assemblyPluginCatalog.FindTypes(typeFinderCriteria);

				if (tps == null || tps.Count == 0)
					continue;

				types.AddUniqueRange(tps);
			}

			return types;
		}
	}
}

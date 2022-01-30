using Raider.Plugins.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;

namespace Raider.Reflection.Loader
{
	/// <summary>
	/// Defines a Plugin Load Context which allows the loading of plugin specific version's of assemblies.
	/// </summary>
	public class PluginAssemblyLoadContext : AssemblyLoadContext, ITypeFindingContext
	{
		private readonly string _assemblyPath;
		private readonly AssemblyDependencyResolver _resolver;
		private readonly PluginLoadContextOptions _options;
		private readonly List<RuntimeAssemblyHint> _runtimeAssemblyHints;

		public PluginAssemblyLoadContext(Assembly assembly, PluginLoadContextOptions? options = null)
			: this(assembly.Location, options)
		{
		}

		public PluginAssemblyLoadContext(string assemblyPath, PluginLoadContextOptions? options = null)
			: base(true)
		{
			_assemblyPath = assemblyPath;
			_resolver = new AssemblyDependencyResolver(assemblyPath);
			_options = options ?? new PluginLoadContextOptions();

			_runtimeAssemblyHints = _options.RuntimeAssemblyHints;

			if (_runtimeAssemblyHints == null)
				_runtimeAssemblyHints = new List<RuntimeAssemblyHint>();
		}

		public Assembly Load()
		{
			var assemblyName = new AssemblyName(Path.GetFileNameWithoutExtension(_assemblyPath));
			var result = LoadFromAssemblyName(assemblyName);
			return result;
		}

		protected override Assembly? Load(AssemblyName assemblyName)
		{
			_options.LogDebug?.Invoke("Loading {AssemblyName}", new object[] { assemblyName });

			if (TryUseHostApplicationAssembly(assemblyName))
			{
				var foundFromHostApplication = LoadHostApplicationAssembly(assemblyName);

				if (foundFromHostApplication)
				{
					_options.LogDebug?.Invoke("Assembly {AssemblyName} is available through host application's AssemblyLoadContext. Use it. ", new object[] { assemblyName });

					return null;
				}

				_options.LogDebug?.Invoke("Host application's AssemblyLoadContext doesn't contain {AssemblyName}. Try to resolve it through the plugin's references.", new object[] { assemblyName });
			}

			string? assemblyPath;

			var assemblyFileName = assemblyName.Name + ".dll";

			if (_runtimeAssemblyHints.Any(x => string.Equals(assemblyFileName, x.FileName)))
			{
				_options.LogDebug?.Invoke("Found assembly hint for {AssemblyName}", new object[] { assemblyName });
				assemblyPath = _runtimeAssemblyHints.First(x => string.Equals(assemblyFileName, x.FileName)).Path;
			}
			else
			{
				_options.LogDebug?.Invoke("No assembly hint found for {AssemblyName}. Using the default resolver for locating the file", new object[] { assemblyName });
				assemblyPath = _resolver.ResolveAssemblyToPath(assemblyName);
			}

			if (assemblyPath != null)
			{
				_options.LogDebug?.Invoke("Loading {AssemblyName} into AssemblyLoadContext from {Path}", new object[] { assemblyName, assemblyPath });

				var result = LoadFromAssemblyPath(assemblyPath);
				return result;
			}

			if (_options.UseHostApplicationAssemblies == UseHostApplicationAssembliesEnum.PreferExternal)
			{
				var foundFromHostApplication = LoadHostApplicationAssembly(assemblyName);

				if (foundFromHostApplication)
				{
					_options.LogDebug?.Invoke("Assembly {AssemblyName} not available from plugin's references but is available through host application's AssemblyLoadContext. Use it.", new object[] { assemblyName });

					return null;
				}
			}

			if (_options.AdditionalRuntimePaths?.Any() != true)
			{
				_options.LogWarning?.Invoke("Couldn't locate assembly using {AssemblyName}. Please try adding AdditionalRuntimePaths using " + nameof(PluginLoadContextOptions.Defaults.AdditionalRuntimePaths), new object[] { assemblyName });

				return null;
			}

			// Solving issue 23. The project doesn't reference WinForms but the plugin does.
			// Try to locate the required dll using AdditionalRuntimePaths
			foreach (var runtimePath in _options.AdditionalRuntimePaths)
			{
				var fileName = assemblyFileName;
				var filePath = Directory.GetFiles(runtimePath, fileName, SearchOption.AllDirectories).FirstOrDefault();

				if (filePath != null)
				{
					_options.LogDebug?.Invoke("Located {AssemblyName} to {AssemblyPath} using {AdditionalRuntimePath}", new object[] { assemblyName, filePath, runtimePath });

					return LoadFromAssemblyPath(filePath);
				}
			}

			_options.LogWarning?.Invoke("Couldn't locate assembly using {AssemblyName}. Didn't find the assembly from AdditionalRuntimePaths. Please try adding AdditionalRuntimePaths using " + nameof(PluginLoadContextOptions.Defaults.AdditionalRuntimePaths), new object[] { assemblyName });

			return null;
		}

		private bool TryUseHostApplicationAssembly(AssemblyName assemblyName)
		{
			_options.LogDebug?.Invoke("Determining if {AssemblyName} should be loaded from host application's or from plugin's AssemblyLoadContext", new object[] { assemblyName });

			if (_options.UseHostApplicationAssemblies == UseHostApplicationAssembliesEnum.Never)
			{
				_options.LogDebug?.Invoke("UseHostApplicationAssemblies is set to Never. Try to load assembly from plugin's AssemblyLoadContext", new object[] { assemblyName });

				return false;
			}

			if (_options.UseHostApplicationAssemblies == UseHostApplicationAssembliesEnum.Always)
			{
				_options.LogDebug?.Invoke("UseHostApplicationAssemblies is set to Always. Try to load assembly from host application's AssemblyLoadContext", new object[] { assemblyName });

				return true;
			}

			if (_options.UseHostApplicationAssemblies == UseHostApplicationAssembliesEnum.Selected)
			{
				var name = assemblyName.Name;

				var result = _options.HostApplicationAssemblies?.Any(x => string.Equals(x.Name, name, StringComparison.InvariantCultureIgnoreCase)) == true;

				_options.LogDebug?.Invoke("UseHostApplicationAssemblies is set to Selected. {AssemblyName} listed in the HostApplicationAssemblies: {Result}", new object[] { assemblyName, result });

				return result;
			}

			if (_options.UseHostApplicationAssemblies == UseHostApplicationAssembliesEnum.PreferExternal)
			{
				_options.LogDebug?.Invoke("UseHostApplicationAssemblies is set to PreferExternal. Try to load assembly from plugin's AssemblyLoadContext and fallback to host application's AssemblyLoadContext", new object[] { assemblyName });

				return false;
			}

			return false;
		}

		private static bool LoadHostApplicationAssembly(AssemblyName assemblyName)
		{
			try
			{
				Default.LoadFromAssemblyName(assemblyName);

				return true;
			}
			catch
			{
				return false;
			}
		}

		protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
		{
			var nativeHint = _runtimeAssemblyHints.FirstOrDefault(x => x.IsNative && string.Equals(x.FileName, unmanagedDllName));

			if (nativeHint != null)
			{
				return LoadUnmanagedDllFromPath(nativeHint.Path);
			}

			var libraryPath = _resolver.ResolveUnmanagedDllToPath(unmanagedDllName);

			if (libraryPath != null)
			{
				return LoadUnmanagedDllFromPath(libraryPath);
			}

			return IntPtr.Zero;
		}

		public Assembly? FindAssembly(string assemblyName)
		{
			return Load(new AssemblyName(assemblyName));
		}

		public Type? FindType(Type type)
		{
			var assemblyName = type.Assembly.GetName();
			var assembly = Load(assemblyName);

			if (assembly == null)
				assembly = Assembly.Load(assemblyName);

			if (string.IsNullOrWhiteSpace(type.FullName))
				return null;

			var result = assembly.GetType(type.FullName);

			return result;
		}
	}
}

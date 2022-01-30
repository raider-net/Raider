using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Raider.Plugins.Catalogs;
using Raider.Plugins.DependencyInjection;
using Raider.Plugins.Internal;
using Raider.Plugins.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Raider.Plugins.Extensions
{
	public static class ServiceCollectionExtensions
	{
		public static IServiceCollection AddPlugins<TPluginType>(
			this IServiceCollection services,
			string? pluginsDirectory = null,
			ServiceCollectionConfiguratorMode servicesConfiguratorMode = ServiceCollectionConfiguratorMode.TryApply,
			object[]? servicesConfiguratorArgs = null,
			Action<TypeFinderCriteriaBuilder>? configureTypeCriteria = null,
			ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
			where TPluginType : class
		{
			services.TryAddServices<TPluginType>();

			if (string.IsNullOrWhiteSpace(pluginsDirectory))
			{
				var entryAssembly = Assembly.GetEntryAssembly();

				if (entryAssembly == null)
					pluginsDirectory = Environment.CurrentDirectory;
				else
					pluginsDirectory = Path.GetDirectoryName(entryAssembly.Location)!;
			}

			TypeFinderCriteria typeFinderCriteria;
			var typeFinderCriteriaBuilder = TypeFinderCriteriaBuilder.Create();

			if (configureTypeCriteria == null)
			{
				typeFinderCriteria = typeFinderCriteriaBuilder
					.AssignableTo(typeof(TPluginType))
					.Build();
			}
			else
			{
				configureTypeCriteria.Invoke(typeFinderCriteriaBuilder);
				typeFinderCriteria = typeFinderCriteriaBuilder.Build();
			}

			var folderPluginCatalog = new FolderPluginCatalog(pluginsDirectory, typeFinderCriteria);
			services.AddPluginCatalog(folderPluginCatalog);
			services.AddPluginInstances<TPluginType>(serviceLifetime);

			foreach (var assemblyPluginCatalog in folderPluginCatalog.GetAssemblyPluginCatalogs())
				ApplyServiceCollectionConfiguration(services, pluginsDirectory, assemblyPluginCatalog, servicesConfiguratorMode, servicesConfiguratorArgs);

			return services;
		}

		public static IServiceCollection AddPlugins<TPluginType>(
			this IServiceCollection services,
			string pluginsDirectory,
			Assembly assembly,
			ServiceCollectionConfiguratorMode servicesConfiguratorMode = ServiceCollectionConfiguratorMode.TryApply,
			object[]? servicesConfiguratorArgs = null,
			Action<TypeFinderCriteriaBuilder>? configureTypeCriteria = null,
			ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
			where TPluginType : class
		{
			if (string.IsNullOrWhiteSpace(pluginsDirectory) && servicesConfiguratorMode != ServiceCollectionConfiguratorMode.Ignore)
				throw new ArgumentNullException(nameof(pluginsDirectory));

			if (assembly == null)
				throw new ArgumentNullException(nameof(assembly));

			services.TryAddServices<TPluginType>();

			TypeFinderCriteria typeFinderCriteria;
			var typeFinderCriteriaBuilder = TypeFinderCriteriaBuilder.Create();

			if (configureTypeCriteria == null)
			{
				typeFinderCriteria = typeFinderCriteriaBuilder
					.AssignableTo(typeof(TPluginType))
					.Build();
			}
			else
			{
				configureTypeCriteria.Invoke(typeFinderCriteriaBuilder);
				typeFinderCriteria = typeFinderCriteriaBuilder.Build();
			}

			var assemblyPluginCatalog = new AssemblyPluginCatalog(assembly, typeFinderCriteria);
			services.AddPluginCatalog(assemblyPluginCatalog);
			services.AddPluginInstances<TPluginType>(serviceLifetime);

			ApplyServiceCollectionConfiguration(services, pluginsDirectory, assemblyPluginCatalog, servicesConfiguratorMode, servicesConfiguratorArgs);

			return services;
		}

		private static void ApplyServiceCollectionConfiguration(
			IServiceCollection services,
			string pluginsDirectory,
			AssemblyPluginCatalog assemblyPluginCatalog,
			ServiceCollectionConfiguratorMode servicesConfiguratorMode = ServiceCollectionConfiguratorMode.TryApply,
			object[]? servicesConfiguratorArgs = null)
		{
			if (servicesConfiguratorMode == ServiceCollectionConfiguratorMode.Ignore)
				return;

			if (assemblyPluginCatalog == null)
				throw new ArgumentNullException(nameof(assemblyPluginCatalog));

			var expectedType = typeof(IServiceCollectionConfigurator);

			var configuratorTypes = assemblyPluginCatalog.FindTypes(new TypeFinderCriteria
			{
				AssignableTo = expectedType
			});

			var found = false;
			foreach (var configuratorType in configuratorTypes)
			{
				//var serviceCollectionConfigurator = Activator.CreateInstance(configuratorType);
				//var configureServiceCollectionMethod = configuratorType.GetMethod(nameof(IServiceCollectionConfigurator.ConfigureServiceCollection));
				//var result = configureServiceCollectionMethod?.Invoke(serviceCollectionConfigurator, new object[] { services, pluginsDirectory, servicesConfiguratorArgs ?? Array.Empty<object>() });
				//found = result is bool boolResult && boolResult == true;

				if (Activator.CreateInstance(configuratorType) is IServiceCollectionConfigurator serviceCollectionConfigurator)
				{
					var result = serviceCollectionConfigurator.ConfigureServiceCollection(services, pluginsDirectory, servicesConfiguratorArgs ?? Array.Empty<object>());
					if (result)
						found = true;
				}
			}

			if (!found && servicesConfiguratorMode == ServiceCollectionConfiguratorMode.MustApply)
				throw new InvalidOperationException($"Cannot found type {expectedType.FullName} in assembly {assemblyPluginCatalog.Assembly.FullName} location = {assemblyPluginCatalog.Assembly.Location}");
		}

		private static IServiceCollection TryAddServices<TPluginType>(this IServiceCollection services)
			where TPluginType : class
		{
			services.AddHostedService<PluginsInitializer>();
			services.TryAddSingleton<PluginProvider>();

			services.TryAddSingleton(sp =>
			{
				var result = new List<Plugin>();
				var catalogs = sp.GetServices<IPluginCatalog>();

				foreach (var catalog in catalogs)
				{
					var plugins = catalog.GetPlugins();

					result.AddRange(plugins);
				}

				return result;
			});

			if (PluginLoadContextOptions.Defaults.AdditionalRuntimePaths == null)
				PluginLoadContextOptions.Defaults.AdditionalRuntimePaths = new List<string>();

			var runtimeAssemblyLocation = typeof(TPluginType).Assembly.Location;

			if (string.IsNullOrWhiteSpace(runtimeAssemblyLocation))
				return services;

			var runtimeAssemblyLocationDirectory = Path.GetDirectoryName(runtimeAssemblyLocation)!;

			if (!PluginLoadContextOptions.Defaults.AdditionalRuntimePaths.Contains(runtimeAssemblyLocationDirectory))
				PluginLoadContextOptions.Defaults.AdditionalRuntimePaths.Add(runtimeAssemblyLocationDirectory);

			return services;
		}

		private static IServiceCollection AddPluginCatalog(this IServiceCollection services, IPluginCatalog pluginCatalog)
		{
			services.TryAddEnumerable(ServiceDescriptor.Singleton(typeof(IPluginCatalog), pluginCatalog));
			return services;
		}

		private static IServiceCollection AddPluginInstances<TPluginType>(this IServiceCollection services, ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
			where TPluginType : class
		{
			var serviceDescriptorEnumerable = new ServiceDescriptor(
				typeof(IEnumerable<TPluginType>),
				sp =>
					{
						var pluginProvider = sp.GetRequiredService<PluginProvider>();
						var result = pluginProvider.GetPluginInstances<TPluginType>();
						return result;
					}
				, serviceLifetime);

			services.Add(serviceDescriptorEnumerable);
			return services;
		}
	}
}

using Microsoft.Extensions.Localization;
using Raider.Text;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;

namespace Raider.Localization
{
	public static class ResourceLoader
    {
        /// <summary>
        /// Create <see cref="ResourceManager"/> based on type and project relative path to *.resx file
        /// </summary>
        /// <param name="resourceSource">Type to resolve the assembly with embeded resource</param>
        /// <param name="resourcesRelativePath">Relative path to resx file in csproj without resx extension. Example: for "Resources\\Labels.resx" relative path is "Resources\\Labels"</param>
        /// <returns></returns>
        public static ResourceManager CreateResourceManager(Type resourceSource, string resourcesRelativePath)
        {
            if (resourceSource == null)
                throw new ArgumentNullException(nameof(resourceSource));

            var typeInfo = resourceSource.GetTypeInfo();
            var resourceAssembly = typeInfo.Assembly;

            return CreateResourceManager(resourceAssembly, resourcesRelativePath);
        }

        public static ResourceManager CreateResourceManager(Assembly resourceAssembly, string resourcesRelativePath)
        {
            if (resourceAssembly == null)
                throw new ArgumentNullException(nameof(resourceAssembly));

            var assemblyName = new AssemblyName(resourceAssembly.FullName);
            var resourcePath = GetResourcePath(resourceAssembly, resourcesRelativePath);
            var baseName = GetResourcePrefix(assemblyName.Name, resourcePath);
            return new ResourceManager(baseName, resourceAssembly);
        }

        public static ResourceSet GetResourceSet(Type resourceSource, string resourcesRelativePath, CultureInfo culture)
        {
            ResourceManager resMgr = CreateResourceManager(resourceSource, resourcesRelativePath);
            return resMgr?.GetResourceSet(culture ?? CultureInfo.InvariantCulture, true, true);
        }

        public static ResourceSet GetResourceSet(Assembly resourceAssembly, string resourcesRelativePath, CultureInfo culture)
        {
            ResourceManager resMgr = CreateResourceManager(resourceAssembly, resourcesRelativePath);
            return resMgr?.GetResourceSet(culture ?? CultureInfo.InvariantCulture, true, true);
        }

        private static string GetResourcePath(Assembly assembly, string resourcesRelativePath)
        {
            var resourceLocationAttribute = GetResourceLocationAttribute(assembly);

            // If we don't have an attribute assume all assemblies use the same resource location.
            var resourceLocation = resourceLocationAttribute == null
                ? (resourcesRelativePath ?? "")
                : resourceLocationAttribute.ResourceLocation + ".";
            resourceLocation = resourceLocation
                .Replace(Path.DirectorySeparatorChar, '.')
                .Replace(Path.AltDirectorySeparatorChar, '.');

            return resourceLocation;
        }

        private static string GetResourcePrefix(string baseNamespace, string resourcesRelativePath)
        {
            if (string.IsNullOrEmpty(baseNamespace))
                throw new ArgumentNullException(nameof(baseNamespace));

            return string.IsNullOrEmpty(resourcesRelativePath)
                ? baseNamespace
                : baseNamespace + "." + resourcesRelativePath /*+ StringHelper.TrimPrefix(typeInfo.FullName, baseNamespace + ".")*/;
        }

        private static ResourceLocationAttribute GetResourceLocationAttribute(Assembly assembly)
        {
            return assembly.GetCustomAttribute<ResourceLocationAttribute>();
        }

		private static List<ResourceFile> LoadResources(string rootFolder, bool readResourcesFromResx = true, CultureInfo searchForCultureIfExists = null, SearchOption searchOption = SearchOption.AllDirectories)
		{
			if (string.IsNullOrWhiteSpace(rootFolder))
				throw new ArgumentNullException(nameof(rootFolder));

			if (!rootFolder.EndsWith("\\"))
				rootFolder += "\\";

			List<ResourceFile> result = new List<ResourceFile>();
			foreach (var resourcePath in Directory.EnumerateFiles(rootFolder, "*.resx", searchOption))
			{
				string resourceFullName = Path.GetFileNameWithoutExtension(resourcePath);
				var relativePath = StringHelper.TrimPrefix(resourcePath, rootFolder);
				CultureInfo cultureInfo = null;

				string resourceCulture = "";
				string resourceName = "";
				int lastDotIndex = resourceFullName.LastIndexOf(".");
				if (lastDotIndex < 0 && lastDotIndex < resourceFullName.Length - 1)
				{
					resourceName = resourceFullName;
					cultureInfo = CultureInfo.InvariantCulture;
					relativePath = StringHelper.TrimPostfix(relativePath, ".resx");
				}
				else
				{
					resourceName = resourceFullName.Substring(0, lastDotIndex);
					resourceCulture = resourceFullName.Substring(lastDotIndex + 1);
					cultureInfo = CultureInfo.GetCultureInfo(resourceCulture);
					relativePath = StringHelper.TrimPostfix(relativePath, $".{resourceCulture}.resx");
				}

				List<Resource> resources = null;
				if (readResourcesFromResx)
				{
					resources = new List<Resource>();
					//XDocument xdoc = XDocument.Load(resourcePath);
					//foreach (var data in xdoc.Descendants().Where(d => d.Name == "data" && d.Parent == xdoc.Root))
					//{
					//	string name = data.Attribute("name").Value;
					//	string value = data.Descendants("value").FirstOrDefault().Value;
					//	resources.Add(new Resource(name, value));
					//}
					var resxBuilder = new ResxBuilder(resourcePath);
					foreach (var data in resxBuilder.Data)
						resources.Add(new Resource(data));
				}

				var resourceFile = new ResourceFile(relativePath, resources)
				{
					FullPath = resourcePath,
					Name = resourceName,
					CultureInfo = cultureInfo
				};
				result.Add(resourceFile);
			}

			if (searchForCultureIfExists != null)
			{
				result = result
					.GroupBy(rf => rf.RelativePath)
					.Select(group =>
					{
						var rf = group.FirstOrDefault(r => r.CultureInfo == searchForCultureIfExists);

						if (rf == null)
							rf = group.OrderBy(x => x.CultureInfo).FirstOrDefault();

						return rf;
					}).ToList();
			}

			return result.OrderBy(rf => rf.RelativePath).ThenBy(rf => rf.CultureInfo?.Name ?? "").ToList();
		}

        public static List<ResourceFile> LoadResources(string path, Assembly resourceAssembly, CultureInfo? searchForCultureIfExists = null, ResourceLoadOptions resourceLoadOptions = ResourceLoadOptions.LoadResxAllResources, SearchOption searchOption = SearchOption.AllDirectories)
        {
			List<ResourceFile> result = LoadResources(
				path,
				resourceLoadOptions == ResourceLoadOptions.LoadResxAllResources,
				searchForCultureIfExists,
				searchOption);

            foreach (var resourceFile in result)
            {
				resourceFile.ResourceAssembly = resourceAssembly;

				if (resourceLoadOptions == ResourceLoadOptions.LoadAssemblyResourceSet
					|| resourceLoadOptions == ResourceLoadOptions.LoadAssemblyResourceSetWithAllResources)
					resourceFile.LoadResourceSet(resourceLoadOptions == ResourceLoadOptions.LoadAssemblyResourceSetWithAllResources);
			}

            return result;
		}

		public static List<ResourceFile> LoadResources(string path, Type resourceSource, CultureInfo? searchForCultureIfExists = null, ResourceLoadOptions resourceLoadOptions = ResourceLoadOptions.None, SearchOption searchOption = SearchOption.AllDirectories)
		{
			if (resourceSource == null)
				throw new ArgumentNullException(nameof(resourceSource));

			var typeInfo = resourceSource.GetTypeInfo();
			var resourceAssembly = typeInfo.Assembly;

			return LoadResources(path, resourceAssembly, searchForCultureIfExists, resourceLoadOptions, searchOption);
		}
	}
}

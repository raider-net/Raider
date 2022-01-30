using Raider.Reflection.Loader;
using System;
using System.Linq;
using System.Reflection;

namespace Raider.Plugins.Loader
{
	public class MetadataTypeFindingContext : ITypeFindingContext
	{
		private readonly MetadataLoadContext _metadataLoadContext;

		public MetadataTypeFindingContext(MetadataLoadContext metadataLoadContext)
		{
			_metadataLoadContext = metadataLoadContext;
		}

		public Assembly? FindAssembly(string assemblyName)
		{
			var result = _metadataLoadContext.LoadFromAssemblyName(assemblyName);

			return result;
		}

		public Type? FindType(Type type)
		{
			var assemblyName = type.Assembly.GetName();
			var assemblies = _metadataLoadContext.GetAssemblies();

			var assembly = assemblies.FirstOrDefault(x => string.Equals(x.FullName, assemblyName.FullName));

			if (assembly == null)
				assembly = _metadataLoadContext.LoadFromAssemblyName(assemblyName);

			if (string.IsNullOrWhiteSpace(type.FullName))
				return null;

			var result = assembly.GetType(type.FullName);

			return result;
		}
	}
}

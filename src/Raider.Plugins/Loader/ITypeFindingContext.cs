using System;
using System.Reflection;

namespace Raider.Reflection.Loader
{
	public interface ITypeFindingContext
	{
		Assembly? FindAssembly(string assemblyName);

		Type? FindType(Type type);
	}
}

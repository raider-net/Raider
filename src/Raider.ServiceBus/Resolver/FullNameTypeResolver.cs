using System;
using System.Linq;
using System.Reflection;

namespace Raider.ServiceBus.Resolver
{
	/// <summary>
	/// <see cref="ITypeResolver"/> that uses the <see cref="Type.FullName"/> for converting type to string.
	/// </summary>
	public class FullNameTypeResolver : ITypeResolver
	{
		public string ToName(Type type)
			=> type?.FullName ?? throw new ArgumentNullException(nameof(type));

		public Type ToType(string fullName)
		{
			if (string.IsNullOrWhiteSpace(fullName))
				throw new ArgumentNullException(nameof(fullName));

			var referencedAssemblies = Assembly.GetEntryAssembly()?
				.GetReferencedAssemblies()
				.Select(a => a.FullName);

			if (referencedAssemblies == null)
				throw new InvalidOperationException("No EntryAssembly.");

			return AppDomain.CurrentDomain.GetAssemblies()
				.Where(a => referencedAssemblies.Contains(a.FullName))
				.SelectMany(a => a.GetTypes().Where(x => x.FullName == fullName))
				.FirstOrDefault()
				?? throw new InvalidOperationException($"{nameof(Type)}.{nameof(Type.FullName)} {fullName} cannot be resolved to any type.");
		}
	}
}

using System;

namespace Raider.ServiceBus.Resolver
{
	public interface ITypeResolver
	{
		string ToName(Type type);
		Type ToType(string name);
	}
}

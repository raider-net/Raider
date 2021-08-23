using System.Collections.Generic;

namespace Raider.Serializer
{
	public interface IFormDataSerializable
	{
		List<KeyValuePair<string, string>> Serialize(string? prefix);
	}
}

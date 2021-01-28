using System.Collections.Generic;

namespace Raider.Serializer
{
	public interface IDictionaryObject
	{
		IReadOnlyDictionary<string, object?> ToDictionary();
	}
}

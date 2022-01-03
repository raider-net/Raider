using System.Collections.Generic;

namespace Raider.Serializer
{
	public interface IDictionaryObject
	{
		IDictionary<string, object?> ToDictionary(ISerializer? serializer = null);
	}
}

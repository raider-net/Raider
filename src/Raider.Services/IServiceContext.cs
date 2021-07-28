using System;
using System.Collections.Generic;

namespace Raider.Services
{
	public interface IServiceContext : ICommandServiceContext
	{
		bool AllowCommit { get; set; }
		Dictionary<object, object?> LocalItems { get; }

		bool TryGetLocalItem<TKey, TValue>(TKey key, out TValue? value);
	}
}

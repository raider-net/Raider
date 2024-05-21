using System;
using System.Collections.Generic;

namespace Raider.QueryServices
{
	public interface IServiceContext : IQueryServiceContext, IDisposable, IAsyncDisposable
	{
		bool AllowCommit { get; set; }
		Dictionary<object, object?> LocalItems { get; }

		bool TryGetLocalItem<TKey, TValue>(TKey key, out TValue? value);

		void SetIsDisposable();
	}
}

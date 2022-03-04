using System;
using System.Threading;
using System.Threading.Tasks;

namespace Raider.Threading
{
	/*
	 USAGE:
	private static readonly AsyncLock _lock = new AsyncLock();
 
	using(await _lock.LockAsync())
	{
		// Critical section... You can await here!
	}
	 */

	public class AsyncLock : IDisposable
	{
		private readonly SemaphoreSlim _semaphoreSlim;
		private bool disposedValue;

		public AsyncLock()
		{
			_semaphoreSlim = new SemaphoreSlim(1, 1);
		}

		public async Task<AsyncLock> LockAsync()
		{
			await _semaphoreSlim.WaitAsync().ConfigureAwait(false);
			return this;
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					_semaphoreSlim.Release();
				}

				disposedValue = true;
			}
		}

		public void Dispose()
		{
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}
	}
}

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
		private SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);

		public async Task<AsyncLock> LockAsync()
		{
			await _semaphoreSlim.WaitAsync();
			return this;
		}

		private bool disposed;
		protected virtual void Dispose(bool disposing)
		{
			if (!disposed)
			{
				if (disposing)
				{
					_semaphoreSlim.Release();
				}

				disposed = true;
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
	}
}

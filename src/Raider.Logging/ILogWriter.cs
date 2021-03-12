using System;

namespace Raider.Logging
{
	public interface ILogWriter : IDisposable
	{
		void Write<T>(T obj);
	}
}

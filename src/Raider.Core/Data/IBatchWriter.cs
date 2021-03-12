using System;

namespace Raider.Data
{
	public interface IBatchWriter : IDisposable
	{
	}

	public interface IBatchWriter<T> : IBatchWriter
	{
		void Write(T obj);
	}
}

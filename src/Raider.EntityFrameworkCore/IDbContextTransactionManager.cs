using Microsoft.EntityFrameworkCore.Storage;
using System;

namespace Raider.EntityFrameworkCore
{
	public interface IDbContextTransactionManager : IDisposable, IAsyncDisposable
	{
		IDbContextTransaction? DbContextTransaction { get; }
		bool IsTransactionCommitted { get; }
	}
}

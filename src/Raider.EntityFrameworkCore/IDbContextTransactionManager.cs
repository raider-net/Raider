using Microsoft.EntityFrameworkCore.Storage;

namespace Raider.EntityFrameworkCore
{
	public interface IDbContextTransactionManager
	{
		IDbContextTransaction? DbContextTransaction { get; }
		bool IsTransactionCommitted { get; }
	}
}

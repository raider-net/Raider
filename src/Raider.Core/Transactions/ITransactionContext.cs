using System;
using System.Threading;
using System.Threading.Tasks;

namespace Raider.Transactions
{
	public interface ITransactionContext : ITransactionContextRegister, IDisposable
#if NET5_0_OR_GREATER
		, IAsyncDisposable
#endif
	{
		/// <summary>
		/// Executes commit actions enlisted in the transaction with <see cref="TransactionContextBuilder.OnCommitted(Action{ITransactionContext})"/>
		/// Then if all succeeded, executes after commit actions enlisted in the transaction with <see cref="TransactionContextBuilder.OnAfterRollback(Action{ITransactionContext})"/>
		/// </summary>
		void Commit();

		/// <summary>
		/// Executes commit actions enlisted in the transaction with <see cref="TransactionContextBuilder.OnCommitted(Func{ITransactionContext, CancellationToken, Task})"/>
		/// Then if all succeeded, executes after commit actions enlisted in the transaction with <see cref="TransactionContextBuilder.OnAfterCommit(Func{ITransactionContext, CancellationToken, Task})"/>
		/// </summary>
		Task CommitAsync(CancellationToken cancellationToken = default);

		/// <summary>
		/// Executes rollback actions enlisted in the transaction with <see cref="TransactionContextBuilder.OnRollback(Action{ITransactionContext})"/>
		/// Then if all succeeded, executes after rollback actions enlisted in the transaction with <see cref="TransactionContextBuilder.OnAfterRollback(Action{ITransactionContext})"/>
		/// </summary>
		void Rollback();

		/// <summary>
		/// Executes rollback actions enlisted in the transaction with <see cref="TransactionContextBuilder.OnRollback(Func{ITransactionContext, CancellationToken, Task})"/>
		/// Then if all succeeded, executes after rollback actions enlisted in the transaction with <see cref="TransactionContextBuilder.OnAfterRollback(Func{ITransactionContext, CancellationToken, Task})"/>
		/// </summary>
		Task RollbackAsync(CancellationToken cancellationToken = default);
	}
}

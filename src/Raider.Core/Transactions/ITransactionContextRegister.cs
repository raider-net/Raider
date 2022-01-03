using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Raider.Transactions
{
	public interface ITransactionContextRegister : IDisposable
#if NET5_0
		, IAsyncDisposable
#endif
	{
		/// <summary>
		/// Gets the transaction context identifier.
		/// </summary>
		Guid TransactionId { get; }

		/// <summary>
		/// Gets custom transaction items.
		/// </summary>
		ConcurrentDictionary<string, object> Items { get; }

		internal event Action? OnBeforeCommit;

		internal event Action? OnBeforeRollback;

		internal event Action? OnBeforeDispose;

		void Merge(ITransactionContextRegister transactionContextRegister, bool force = false);

		/// <summary>
		/// Registers a listener to be called when the transaction is committed.
		/// </summary>
		bool OnCommitted(Func<ITransactionContext, CancellationToken, Task> commitAction);

		/// <summary>
		/// Registers a listener to be called when the transaction is committed.
		/// </summary>
		bool OnCommitted(Action<ITransactionContext> commitAction);

		/// <summary>
		/// Registers a listener to be called after the transaction has been successfully committed - all listeners
		/// registered with <see cref="OnCommitted"/> have been executed.
		/// </summary>
		bool OnAfterCommit(Func<ITransactionContext, CancellationToken, Task> afterCommitAction);

		/// <summary>
		/// Registers a listener to be called after the transaction has been successfully committed - all listeners
		/// registered with <see cref="OnCommitted"/> have been executed.
		/// </summary>
		bool OnAfterCommit(Action<ITransactionContext> afterCommitAction);

		/// <summary>
		/// Registers a listener to be called when all changes have been discarded.
		/// </summary>
		bool OnRollback(Func<ITransactionContext, CancellationToken, Task> rollbackAction);

		/// <summary>
		/// Registers a listener to be called when all changes have been discarded.
		/// </summary>
		bool OnRollback(Action<ITransactionContext> rollbackAction);

		/// <summary>
		/// Registers a listener to be called after rollback - all listeners
		/// registered with <see cref="OnRollback"/> have been executed.
		/// </summary>
		bool OnAfterRollback(Func<ITransactionContext, CancellationToken, Task> afterRollbackAction);

		/// <summary>
		/// Registers a listener to be called after rollback - all listeners
		/// registered with <see cref="OnRollback"/> have been executed.
		/// </summary>
		bool OnAfterRollback(Action<ITransactionContext> afterRollbackAction);

#if NET5_0
		/// <summary>
		/// Registers a listener to be called after the transaction is over.
		/// </summary>
		bool OnDisposed(Func<ITransactionContext, ValueTask> disposedAction);
#endif

		/// <summary>
		/// Registers a listener to be called after the transaction is over.
		/// </summary>
		bool OnDisposed(Action<ITransactionContext> disposedAction);
	}
}

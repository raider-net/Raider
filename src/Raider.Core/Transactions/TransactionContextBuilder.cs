using Raider.Extensions;
using Raider.Transactions.Internal;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Raider.Transactions
{
	public class TransactionContextBuilder
	{
		private readonly TransactionContextOptions _options;

		protected TransactionContextBuilder()
		{
			_options = new TransactionContextOptions();
		}

		public static TransactionContextBuilder Create()
		{
			return new TransactionContextBuilder();
		}

		/// <summary>
		/// After commit register new listeners to be called when the transaction is committed
		/// and new listeners to be called after the transaction has been successfully committed.
		/// Next time commit will be raised, new listeners would be executed.
		/// </summary>
		public TransactionContextBuilder AllowMultipleCommits(bool allow = true)
		{
			_options.AllowMultipleCommits = allow;
			return this;
		}

		/// <summary>
		/// Registers a listener to be called when the transaction is committed.
		/// </summary>
		public TransactionContextBuilder OnCommitted(Func<ITransactionContext, CancellationToken, Task> commitAction)
		{
			if (commitAction == null)
				throw new ArgumentNullException(nameof(commitAction));

			_options.OnCommittedAsyncActions.AddUniqueItem(commitAction);
			return this;
		}

		/// <summary>
		/// Registers a listener to be called when the transaction is committed.
		/// </summary>
		public TransactionContextBuilder OnCommitted(Action<ITransactionContext> commitAction)
		{
			if (commitAction == null)
				throw new ArgumentNullException(nameof(commitAction));

			_options.OnCommittedActions.AddUniqueItem(commitAction);
			return this;
		}

		/// <summary>
		/// Registers a listener to be called after the transaction has been successfully committed - all listeners
		/// registered with <see cref="OnCommitted"/> have been executed.
		/// </summary>
		public TransactionContextBuilder OnAfterCommit(Func<ITransactionContext, CancellationToken, Task> afterCommitAction)
		{
			if (afterCommitAction == null)
				throw new ArgumentNullException(nameof(afterCommitAction));

			_options.OnAfterCommitAsyncActions.AddUniqueItem(afterCommitAction);
			return this;
		}

		/// <summary>
		/// Registers a listener to be called after the transaction has been successfully committed - all listeners
		/// registered with <see cref="OnCommitted"/> have been executed.
		/// </summary>
		public TransactionContextBuilder OnAfterCommit(Action<ITransactionContext> afterCommitAction)
		{
			if (afterCommitAction == null)
				throw new ArgumentNullException(nameof(afterCommitAction));

			_options.OnAfterCommitActions.AddUniqueItem(afterCommitAction);
			return this;
		}

		/// <summary>
		/// Registers a listener to be called when all changes have been discarded.
		/// </summary>
		public TransactionContextBuilder OnRollback(Func<ITransactionContext, CancellationToken, Task> rollbackAction)
		{
			if (rollbackAction == null)
				throw new ArgumentNullException(nameof(rollbackAction));

			_options.OnRollbackAsyncActions.AddUniqueItem(rollbackAction);
			return this;
		}

		/// <summary>
		/// Registers a listener to be called when all changes have been discarded.
		/// </summary>
		public TransactionContextBuilder OnRollback(Action<ITransactionContext> rollbackAction)
		{
			if (rollbackAction == null)
				throw new ArgumentNullException(nameof(rollbackAction));

			_options.OnRollbackActions.AddUniqueItem(rollbackAction);
			return this;
		}

		/// <summary>
		/// Registers a listener to be called after rollback - all listeners
		/// registered with <see cref="OnRollback"/> have been executed.
		/// </summary>
		public TransactionContextBuilder OnAfterRollback(Func<ITransactionContext, CancellationToken, Task> afterRollbackAction)
		{
			if (afterRollbackAction == null)
				throw new ArgumentNullException(nameof(afterRollbackAction));

			_options.OnAfterRollbackAsyncActions.AddUniqueItem(afterRollbackAction);
			return this;
		}

		/// <summary>
		/// Registers a listener to be called after rollback - all listeners
		/// registered with <see cref="OnRollback"/> have been executed.
		/// </summary>
		public TransactionContextBuilder OnAfterRollback(Action<ITransactionContext> afterRollbackAction)
		{
			if (afterRollbackAction == null)
				throw new ArgumentNullException(nameof(afterRollbackAction));

			_options.OnAfterRollbackActions.AddUniqueItem(afterRollbackAction);
			return this;
		}

#if NET5_0
		/// <summary>
		/// Registers a listener to be called after the transaction is over.
		/// </summary>
		public TransactionContextBuilder OnDisposed(Func<ITransactionContext, ValueTask> disposedAction)
		{
			if (disposedAction == null)
				throw new ArgumentNullException(nameof(disposedAction));

			_options.OnDisposedAsyncActions.AddUniqueItem(disposedAction);
			return this;
		}
#endif

		/// <summary>
		/// Registers a listener to be called after the transaction is over.
		/// </summary>
		public TransactionContextBuilder OnDisposed(Action<ITransactionContext> disposedAction)
		{
			if (disposedAction == null)
				throw new ArgumentNullException(nameof(disposedAction));

			_options.OnDisposedActions.AddUniqueItem(disposedAction);
			return this;
		}

		public ITransactionContext Build()
		{
			return new TransactionContext(_options);
		}
	}
}

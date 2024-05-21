using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Raider.Transactions.Internal
{
	internal class TransactionContextOptions
	{
		public bool AllowMultipleCommits { get; set; }

		public List<Action<ITransactionContext>> OnCommittedActions { get; }
		public List<Action<ITransactionContext>> OnAfterCommitActions { get; }
		public List<Action<ITransactionContext>> OnRollbackActions { get; }
		public List<Action<ITransactionContext>> OnAfterRollbackActions { get; }
		public List<Action<ITransactionContext>> OnDisposedActions { get; }

		public List<Func<ITransactionContext, CancellationToken, Task>> OnCommittedAsyncActions { get; }
		public List<Func<ITransactionContext, CancellationToken, Task>> OnAfterCommitAsyncActions { get; }
		public List<Func<ITransactionContext, CancellationToken, Task>> OnRollbackAsyncActions { get; }
		public List<Func<ITransactionContext, CancellationToken, Task>> OnAfterRollbackAsyncActions { get; }
#if NET5_0_OR_GREATER
		public List<Func<ITransactionContext, ValueTask>> OnDisposedAsyncActions { get; }
#endif

		public TransactionContextOptions()
		{
			OnCommittedActions = new List<Action<ITransactionContext>>();
			OnAfterCommitActions = new List<Action<ITransactionContext>>();
			OnRollbackActions = new List<Action<ITransactionContext>>();
			OnAfterRollbackActions = new List<Action<ITransactionContext>>();
			OnDisposedActions = new List<Action<ITransactionContext>>();

			OnCommittedAsyncActions = new List<Func<ITransactionContext, CancellationToken, Task>>();
			OnAfterCommitAsyncActions = new List<Func<ITransactionContext, CancellationToken, Task>>();
			OnRollbackAsyncActions = new List<Func<ITransactionContext, CancellationToken, Task>>();
			OnAfterRollbackAsyncActions = new List<Func<ITransactionContext, CancellationToken, Task>>();
#if NET5_0_OR_GREATER
			OnDisposedAsyncActions = new List<Func<ITransactionContext, ValueTask>>();
#endif
		}
	}
}

using Raider.Extensions;
using Raider.Threading;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Raider.Transactions.Internal
{
	internal class TransactionContext : ITransactionContext, IDisposable
#if NET5_0
		, IAsyncDisposable
#endif
	{
		private readonly object _lock = new();
		private readonly AsyncLock _asyncLock = new();

		private readonly bool _allowMultipleCommits;

		private List<Action<ITransactionContext>> _onCommittedActions;
		private List<Action<ITransactionContext>> _onAfterCommitActions;
		private List<Action<ITransactionContext>> _onRollbackActions;
		private List<Action<ITransactionContext>> _onAfterRollbackActions;
		private List<Action<ITransactionContext>> _onDisposedActions;

		private List<Func<ITransactionContext, CancellationToken, Task>> _onCommittedAsyncActions;
		private List<Func<ITransactionContext, CancellationToken, Task>> _onAfterCommitAsyncActions;
		private List<Func<ITransactionContext, CancellationToken, Task>> _onRollbackAsyncActions;
		private List<Func<ITransactionContext, CancellationToken, Task>> _onAfterRollbackAsyncActions;
#if NET5_0
		private List<Func<ITransactionContext, ValueTask>> _onDisposedAsyncActions;
#endif

		private bool _disposed;
		private bool _commitRaised;
		private bool _rollbackRaised;

		/// <inheritdoc/>
		public Guid TransactionId { get; }

		/// <inheritdoc/>
		public ConcurrentDictionary<string, object> Items { get; }

		private event Action? _onBeforeCommit;
		event Action? ITransactionContextRegister.OnBeforeCommit
		{
			add => _onBeforeCommit += value;
			remove => _onBeforeCommit -= value;
		}

		private event Action? _onBeforeRollback;
		event Action? ITransactionContextRegister.OnBeforeRollback
		{
			add => _onBeforeRollback += value;
			remove => _onBeforeRollback -= value;
		}

		private event Action? _onBeforeDispose;
		event Action? ITransactionContextRegister.OnBeforeDispose
		{
			add => _onBeforeDispose += value;
			remove => _onBeforeDispose -= value;
		}

		public TransactionContext(TransactionContextOptions options)
		{
			if (options == null)
				throw new ArgumentNullException(nameof(options));

			TransactionId = Guid.NewGuid();
			Items = new ConcurrentDictionary<string, object>();

			_allowMultipleCommits = options.AllowMultipleCommits;

			_onCommittedActions = options.OnCommittedActions.ToList();
			_onAfterCommitActions = options.OnAfterCommitActions.ToList();
			_onRollbackActions = options.OnRollbackActions.ToList();
			_onAfterRollbackActions = options.OnAfterRollbackActions.ToList();
			_onDisposedActions = options.OnDisposedActions.ToList();

			_onCommittedAsyncActions = options.OnCommittedAsyncActions.ToList();
			_onAfterCommitAsyncActions = options.OnAfterCommitAsyncActions.ToList();
			_onRollbackAsyncActions = options.OnRollbackAsyncActions.ToList();
			_onAfterRollbackAsyncActions = options.OnAfterRollbackAsyncActions.ToList();
#if NET5_0
			_onDisposedAsyncActions = options.OnDisposedAsyncActions.ToList();
#endif
		}

		/// <inheritdoc/>
		public void Commit()
		{
			if (_disposed)
				throw new InvalidOperationException($"Cannot commit disposed {nameof(TransactionContext)}.");

			if (!_allowMultipleCommits && _commitRaised)
				throw new InvalidOperationException($"{nameof(TransactionContext)} is already committed. Multiple commit is not allowed.");

			if (_rollbackRaised)
				throw new InvalidOperationException($"Cannot commit {nameof(TransactionContext)} that raised rollback.");

			//CLEAR ALL CommittedActions and AfterCommitActions, when _allowMultipleCommits is allowed
			var onCommittedActions = Interlocked.Exchange(ref _onCommittedActions, new List<Action<ITransactionContext>>());
			var onAfterCommitActions = Interlocked.Exchange(ref _onAfterCommitActions, new List<Action<ITransactionContext>>());

			lock (_lock)
			{
				if (_disposed)
					throw new InvalidOperationException($"Cannot commit disposed {nameof(TransactionContext)}.");

				if (!_allowMultipleCommits && _commitRaised)
					throw new InvalidOperationException($"{nameof(TransactionContext)} is already committed. Multiple commit is not allowed.");

				if (_rollbackRaised)
					throw new InvalidOperationException($"Cannot commit {nameof(TransactionContext)} that raised rollback.");

				_commitRaised = true;
				_onBeforeCommit?.Invoke();

				if (onCommittedActions != null)
					foreach (var action in onCommittedActions)
						action(this);

				if (onCommittedActions != null)
					foreach (var action in onAfterCommitActions)
						action(this);
			}
		}

		/// <inheritdoc/>
		public async Task CommitAsync(CancellationToken cancellationToken = default)
		{
			if (_disposed)
				throw new InvalidOperationException($"Cannot commit disposed {nameof(TransactionContext)}.");

			if (!_allowMultipleCommits && _commitRaised)
				throw new InvalidOperationException($"{nameof(TransactionContext)} is already committed. Multiple commit is not allowed.");

			if (_rollbackRaised)
				throw new InvalidOperationException($"Cannot commit {nameof(TransactionContext)} that raised rollback.");

			//CLEAR ALL CommittedActions and AfterCommitActions, when _allowMultipleCommits is allowed
			var onCommittedAsyncActions = Interlocked.Exchange(ref _onCommittedAsyncActions, new List<Func<ITransactionContext, CancellationToken, Task>>());
			var onAfterCommitAsyncActions = Interlocked.Exchange(ref _onAfterCommitAsyncActions, new List<Func<ITransactionContext, CancellationToken, Task>>());

			using(await _asyncLock.LockAsync().ConfigureAwait(false))
			{
				if (_disposed)
					throw new InvalidOperationException($"Cannot commit disposed {nameof(TransactionContext)}.");

				if (!_allowMultipleCommits && _commitRaised)
					throw new InvalidOperationException($"{nameof(TransactionContext)} is already committed. Multiple commit is not allowed.");

				if (_rollbackRaised)
					throw new InvalidOperationException($"Cannot commit {nameof(TransactionContext)} that raised rollback.");

				_commitRaised = true;
				_onBeforeCommit?.Invoke();

				if (onCommittedAsyncActions != null)
					foreach (var action in onCommittedAsyncActions)
						await action(this, cancellationToken).ConfigureAwait(false);

				if (onAfterCommitAsyncActions != null)
					foreach (var action in onAfterCommitAsyncActions)
						await action(this, cancellationToken).ConfigureAwait(false);
			}
		}

		/// <inheritdoc/>
		public void Rollback()
		{
			if (_disposed)
				throw new InvalidOperationException($"Cannot rollback disposed {nameof(TransactionContext)}.");

			if (_rollbackRaised)
				throw new InvalidOperationException($"Cannot rollback {nameof(TransactionContext)} multiple times.");

			var onRollbackActions = Interlocked.Exchange(ref _onRollbackActions, new List<Action<ITransactionContext>>());
			var onAfterRollbackActions = Interlocked.Exchange(ref _onAfterRollbackActions, new List<Action<ITransactionContext>>());

			lock (_lock)
			{
				if (_disposed)
					throw new InvalidOperationException($"Cannot rollback disposed {nameof(TransactionContext)}.");

				if (_rollbackRaised)
					throw new InvalidOperationException($"Cannot rollback {nameof(TransactionContext)} multiple times.");

				_rollbackRaised = true;
				_onBeforeRollback?.Invoke();

				if (onRollbackActions != null)
					foreach (var action in onRollbackActions)
						action(this);

				if (onAfterRollbackActions != null)
					foreach (var action in onAfterRollbackActions)
						action(this);
			}
		}

		/// <inheritdoc/>
		public async Task RollbackAsync(CancellationToken cancellationToken = default)
		{
			if (_disposed)
				throw new InvalidOperationException($"Cannot rollback disposed {nameof(TransactionContext)}.");

			if (_rollbackRaised)
				throw new InvalidOperationException($"Cannot rollback {nameof(TransactionContext)} multiple times.");

			var onRollbackAsyncActions = Interlocked.Exchange(ref _onRollbackAsyncActions, new List<Func<ITransactionContext, CancellationToken, Task>>());
			var onAfterRollbackAsyncActions = Interlocked.Exchange(ref _onAfterRollbackAsyncActions, new List<Func<ITransactionContext, CancellationToken, Task>>());

			using (await _asyncLock.LockAsync().ConfigureAwait(false))
			{
				if (_disposed)
					throw new InvalidOperationException($"Cannot rollback disposed {nameof(TransactionContext)}.");

				if (_rollbackRaised)
					throw new InvalidOperationException($"Cannot rollback {nameof(TransactionContext)} multiple times.");

				_rollbackRaised = true;
				_onBeforeRollback?.Invoke();

				if (onRollbackAsyncActions != null)
					foreach (var action in onRollbackAsyncActions)
						await action(this, cancellationToken).ConfigureAwait(false);

				if (onAfterRollbackAsyncActions != null)
					foreach (var action in onAfterRollbackAsyncActions)
						await action(this, cancellationToken).ConfigureAwait(false);
			}
		}

		/// <inheritdoc/>
		public void Merge(ITransactionContextRegister transactionContextRegister, bool force = false)
		{
			if (transactionContextRegister == null)
				throw new ArgumentNullException(nameof(transactionContextRegister));

			if (force)
			{
				foreach (var item in Items)
					transactionContextRegister.Items[item.Key] = item.Value;
			}
			else
			{
				foreach (var item in Items)
					transactionContextRegister.Items.TryAdd(item.Key, item.Value);
			}

			foreach (var action in _onCommittedActions)
				transactionContextRegister.OnCommitted(action);

			foreach (var action in _onAfterCommitActions)
				transactionContextRegister.OnAfterCommit(action);

			foreach (var action in _onRollbackActions)
				transactionContextRegister.OnRollback(action);

			foreach (var action in _onAfterRollbackActions)
				transactionContextRegister.OnAfterRollback(action);

			foreach (var action in _onDisposedActions)
				transactionContextRegister.OnDisposed(action);

			foreach (var action in _onCommittedAsyncActions)
				transactionContextRegister.OnCommitted(action);

			foreach (var action in _onAfterCommitAsyncActions)
				transactionContextRegister.OnAfterCommit(action);

			foreach (var action in _onRollbackAsyncActions)
				transactionContextRegister.OnRollback(action);

			foreach (var action in _onAfterRollbackAsyncActions)
				transactionContextRegister.OnAfterRollback(action);

#if NET5_0
			foreach (var action in _onDisposedAsyncActions)
				transactionContextRegister.OnDisposed(action);
#endif

			transactionContextRegister.OnBeforeCommit += () =>
			{
				var onCommittedActions = Interlocked.Exchange(ref _onCommittedActions, new List<Action<ITransactionContext>>());
				var onAfterCommitActions = Interlocked.Exchange(ref _onAfterCommitActions, new List<Action<ITransactionContext>>());
				var onCommittedAsyncActions = Interlocked.Exchange(ref _onCommittedAsyncActions, new List<Func<ITransactionContext, CancellationToken, Task>>());
				var onAfterCommitAsyncActions = Interlocked.Exchange(ref _onAfterCommitAsyncActions, new List<Func<ITransactionContext, CancellationToken, Task>>());
				_commitRaised = true;
			};

			transactionContextRegister.OnBeforeRollback += () =>
			{
				var onRollbackActions = Interlocked.Exchange(ref _onRollbackActions, new List<Action<ITransactionContext>>());
				var onAfterRollbackActions = Interlocked.Exchange(ref _onAfterRollbackActions, new List<Action<ITransactionContext>>());
				var onRollbackAsyncActions = Interlocked.Exchange(ref _onRollbackAsyncActions, new List<Func<ITransactionContext, CancellationToken, Task>>());
				var onAfterRollbackAsyncActions = Interlocked.Exchange(ref _onAfterRollbackAsyncActions, new List<Func<ITransactionContext, CancellationToken, Task>>());
				_rollbackRaised = true;
			};

			transactionContextRegister.OnBeforeDispose += () =>
			{
				var onDisposedActions = Interlocked.Exchange(ref _onDisposedActions, new List<Action<ITransactionContext>>());
#if NET5_0
				var onDisposedAsyncActions = Interlocked.Exchange(ref _onDisposedAsyncActions, new List<Func<ITransactionContext, ValueTask>>());
#endif
				Dispose();
			};
		}

		/// <inheritdoc/>
		public bool OnCommitted(Func<ITransactionContext, CancellationToken, Task> commitAction)
		{
			if (commitAction == null)
				throw new ArgumentNullException(nameof(commitAction));

			return _onCommittedAsyncActions.TryAddUniqueItem(commitAction);
		}

		/// <inheritdoc/>
		public bool OnCommitted(Action<ITransactionContext> commitAction)
		{
			if (commitAction == null)
				throw new ArgumentNullException(nameof(commitAction));

			return _onCommittedActions.TryAddUniqueItem(commitAction);
		}

		/// <inheritdoc/>
		public bool OnAfterCommit(Func<ITransactionContext, CancellationToken, Task> afterCommitAction)
		{
			if (afterCommitAction == null)
				throw new ArgumentNullException(nameof(afterCommitAction));

			return _onAfterCommitAsyncActions.TryAddUniqueItem(afterCommitAction);
		}

		/// <inheritdoc/>
		public bool OnAfterCommit(Action<ITransactionContext> afterCommitAction)
		{
			if (afterCommitAction == null)
				throw new ArgumentNullException(nameof(afterCommitAction));

			return _onAfterCommitActions.TryAddUniqueItem(afterCommitAction);
		}

		/// <inheritdoc/>
		public bool OnRollback(Func<ITransactionContext, CancellationToken, Task> rollbackAction)
		{
			if (rollbackAction == null)
				throw new ArgumentNullException(nameof(rollbackAction));

			return _onRollbackAsyncActions.TryAddUniqueItem(rollbackAction);
		}

		/// <inheritdoc/>
		public bool OnRollback(Action<ITransactionContext> rollbackAction)
		{
			if (rollbackAction == null)
				throw new ArgumentNullException(nameof(rollbackAction));

			return _onRollbackActions.TryAddUniqueItem(rollbackAction);
		}

		/// <inheritdoc/>
		public bool OnAfterRollback(Func<ITransactionContext, CancellationToken, Task> afterRollbackAction)
		{
			if (afterRollbackAction == null)
				throw new ArgumentNullException(nameof(afterRollbackAction));

			return _onAfterRollbackAsyncActions.TryAddUniqueItem(afterRollbackAction);
		}

		/// <inheritdoc/>
		public bool OnAfterRollback(Action<ITransactionContext> afterRollbackAction)
		{
			if (afterRollbackAction == null)
				throw new ArgumentNullException(nameof(afterRollbackAction));

			return _onAfterRollbackActions.TryAddUniqueItem(afterRollbackAction);
		}

#if NET5_0
		/// <inheritdoc/>
		public bool OnDisposed(Func<ITransactionContext, ValueTask> disposedAction)
		{
			if (disposedAction == null)
				throw new ArgumentNullException(nameof(disposedAction));

			return _onDisposedAsyncActions.TryAddUniqueItem(disposedAction);
		}
#endif

		/// <inheritdoc/>
		public bool OnDisposed(Action<ITransactionContext> disposedAction)
		{
			if (disposedAction == null)
				throw new ArgumentNullException(nameof(disposedAction));

			return _onDisposedActions.TryAddUniqueItem(disposedAction);
		}

#if NET5_0
		/// <inheritdoc/>
		public async ValueTask DisposeAsync()
		{
			if (!_disposed)
			{
				_onBeforeDispose?.Invoke();
				var onDisposedAsyncActions = Interlocked.Exchange(ref _onDisposedAsyncActions, new List<Func<ITransactionContext, ValueTask>>());

				if (onDisposedAsyncActions != null)
					foreach (var action in onDisposedAsyncActions)
						await action(this).ConfigureAwait(false);

				_disposed = true;
			}
		}
#endif

		/// <inheritdoc/>
		protected virtual void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				if (disposing)
				{
					_onBeforeDispose?.Invoke();
					var onDisposedActions = Interlocked.Exchange(ref _onDisposedActions, new List<Action<ITransactionContext>>());

					if (onDisposedActions != null)
						foreach (var action in onDisposedActions)
							action(this);
				}

				_disposed = true;
			}
		}

		/// <inheritdoc/>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
	}
}

using System;
using System.Diagnostics.CodeAnalysis;

namespace Raider.Transactions
{
	public static class ITransactionContextRegisterExtensions
	{
		public static T GetItem<T>(this ITransactionContextRegister transactionContextRegister, string key)
		{
			if (transactionContextRegister == null)
				throw new ArgumentNullException(nameof(transactionContextRegister));

			if (string.IsNullOrWhiteSpace(key))
				throw new ArgumentNullException(nameof(key));

			if (transactionContextRegister.Items.TryGetValue(key, out var v))
				return (T)v;

			throw new ArgumentOutOfRangeException(nameof(key), $"Item with {nameof(key)} = {key} was to found.");
		}

		public static T? GetItemIfExists<T>(this ITransactionContextRegister transactionContextRegister, string key)
		{
			if (transactionContextRegister == null)
				throw new ArgumentNullException(nameof(transactionContextRegister));

			if (string.IsNullOrWhiteSpace(key))
				throw new ArgumentNullException(nameof(key));

			if (transactionContextRegister.Items.TryGetValue(key, out var v))
				return (T)v;

			return default;
		}

		public static bool TryGetItem<T>(this ITransactionContextRegister transactionContextRegister, string key, [NotNullWhen(true)] out T? value)
		{
			if (transactionContextRegister == null)
				throw new ArgumentNullException(nameof(transactionContextRegister));

			if (string.IsNullOrWhiteSpace(key))
				throw new ArgumentNullException(nameof(key));

			if (transactionContextRegister.Items.TryGetValue(key, out var v))
			{
				value = (T)v;
				return true;
			}

			value = default;
			return false;
		}

		public static TTransactionContextRegister AddItem<TTransactionContextRegister, T>(this TTransactionContextRegister transactionContextRegister, string key, T value, bool force = false)
			where TTransactionContextRegister : ITransactionContextRegister
		{
			if (transactionContextRegister == null)
				throw new ArgumentNullException(nameof(transactionContextRegister));

			if (string.IsNullOrWhiteSpace(key))
				throw new ArgumentNullException(nameof(key));

			if (value == null)
				throw new ArgumentNullException(nameof(value));

			if (force)
			{
				transactionContextRegister.Items.AddOrUpdate(key, value, (k, v) => value);
			}
			else
			{
				transactionContextRegister.Items.TryAdd(key, value);
			}

			return transactionContextRegister;
		}

		public static TTransactionContextRegister AddUniqueItem<TTransactionContextRegister, T>(this TTransactionContextRegister transactionContextRegister, string key, T value)
			where TTransactionContextRegister : ITransactionContextRegister
		{
			if (transactionContextRegister == null)
				throw new ArgumentNullException(nameof(transactionContextRegister));

			if (string.IsNullOrWhiteSpace(key))
				throw new ArgumentNullException(nameof(key));

			if (value == null)
				throw new ArgumentNullException(nameof(value));

			var added = transactionContextRegister.Items.TryAdd(key, value);
			if (!added)
				throw new ArgumentException($"Key {key} was already added.", nameof(key));

			return transactionContextRegister;
		}

		public static TTransactionContextRegister RemoveItem<TTransactionContextRegister, T>(this TTransactionContextRegister transactionContextRegister, string key)
			where TTransactionContextRegister : ITransactionContextRegister
		{
			if (transactionContextRegister == null)
				throw new ArgumentNullException(nameof(transactionContextRegister));

			if (string.IsNullOrWhiteSpace(key))
				throw new ArgumentNullException(nameof(key));

			transactionContextRegister.Items.TryRemove(key, out _);
			return transactionContextRegister;
		}
	}
}

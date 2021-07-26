using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Raider.EntityFrameworkCore.QueryCache;
using Raider.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Raider.EntityFrameworkCore
{
	public static class IQueryableCacheExtensions
	{
		/// <summary>A DbSet&lt;T&gt; extension method that expire cache.</summary>
		/// <param name="dbSet">The dbSet to act on.</param>
		public static void ExpireCache<T>(this DbSet<T> dbSet)
			where T : class
		{
			if (dbSet.TryGetDbContextQueryCacheManager(out QueryCacheManager? queryCacheManager))
				queryCacheManager.ExpireType(typeof(T));
		}

		/// <summary>
		///     Return the result of the <paramref name="query" /> from the cache. If the query is not cached
		///     yet, the query is materialized asynchronously and cached before being returned.
		/// </summary>
		/// <typeparam name="T">The generic type of the query.</typeparam>
		/// <param name="query">The query to cache in the QueryCacheManager.</param>
		/// <param name="tags">
		///     A variable-length parameters list containing tags to expire cached
		///     entries.
		/// </param>
		/// <returns>The result of the query.</returns>
		public static List<T> FromCacheToList<T>(this IQueryable<T> query, params string[] tags)
			where T : class
			=> FromCacheToList<T>(query, false, null, tags);

		/// <summary>
		///     Return the result of the <paramref name="query" /> from the cache. If the query is not cached
		///     yet, the query is materialized asynchronously and cached before being returned.
		/// </summary>
		/// <typeparam name="T">The generic type of the query.</typeparam>
		/// <param name="query">The query to cache in the QueryCacheManager.</param>
		/// <param name="withChangeTracking">Indicates if the results of a query are tracked by the Microsoft.EntityFrameworkCore.ChangeTracking.ChangeTracker.</param>
		/// <param name="tags">
		///     A variable-length parameters list containing tags to expire cached
		///     entries.
		/// </param>
		/// <returns>The result of the query.</returns>
		public static List<T> FromCacheToList<T>(this IQueryable<T> query, bool withChangeTracking, params string[] tags)
			where T : class
			=> FromCacheToList<T>(query, withChangeTracking, null, tags);

		/// <summary>
		///     Return the result of the <paramref name="query" /> from the cache. If the query is not cached
		///     yet, the query is materialized asynchronously and cached before being returned.
		/// </summary>
		/// <typeparam name="T">The generic type of the query.</typeparam>
		/// <param name="query">The query to cache in the QueryCacheManager.</param>
		/// <param name="withChangeTracking">Indicates if the results of a query are tracked by the Microsoft.EntityFrameworkCore.ChangeTracking.ChangeTracker.</param>
		/// <param name="options">The cache entry options to use to cache the query.</param>
		/// <param name="tags">
		///     A variable-length parameters list containing tags to expire cached
		///     entries.
		/// </param>
		/// <returns>The result of the query.</returns>
		public static List<T> FromCacheToList<T>(this IQueryable<T> query, bool withChangeTracking, MemoryCacheEntryOptions? options, params string[] tags)
			where T : class
		{
			query.TryGetDbContextQueryCacheManager(out QueryCacheManager? queryCacheManager);

			if (queryCacheManager == null || !queryCacheManager.IsEnabled)
			{
				if (withChangeTracking)
					return query.ToList();
				else
					return query.AsNoTracking().ToList();
			}

			var key = queryCacheManager.GetCacheKey(query, "ToList", tags);

			if (!queryCacheManager.Cache!.TryGetValue(key, out object? item))
			{
				if (options == null)
					options = queryCacheManager.DefaultMemoryCacheEntryOptions ?? new MemoryCacheEntryOptions();

				options.RegisterPostEvictionCallback(queryCacheManager.RemoveCallback);
				item = withChangeTracking
					? query.ToList()
					: query.AsNoTracking().ToList();
				item = queryCacheManager.Cache.Set(key, item, options);
				queryCacheManager.AddCacheTag(key, tags);
				queryCacheManager.AddCacheTag(key, $"{typeof(T).Name}{queryCacheManager.CacheTypePostfix}");
			}

			if (item == null || item == DBNull.Value)
				return new List<T>();

			return (List<T>)item;
		}

		/// <summary>
		///     Return the result of the <paramref name="query" /> from the cache. If the query is not cached
		///     yet, the query is materialized and cached before being returned.
		/// </summary>
		/// <typeparam name="T">The generic type of the query.</typeparam>
		/// <param name="query">The query to cache in the QueryCacheManager.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <param name="tags">
		///     A variable-length parameters list containing tags to expire cached
		///     entries.
		/// </param>
		/// <returns>The result of the query.</returns>
		public static Task<List<T>> FromCacheToListAsync<T>(this IQueryable<T> query, CancellationToken cancellationToken = default, params string[] tags)
			where T : class
			=> FromCacheToListAsync(query, false, null, cancellationToken, tags);

		/// <summary>
		///     Return the result of the <paramref name="query" /> from the cache. If the query is not cached
		///     yet, the query is materialized and cached before being returned.
		/// </summary>
		/// <typeparam name="T">The generic type of the query.</typeparam>
		/// <param name="query">The query to cache in the QueryCacheManager.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <param name="tags">
		///     A variable-length parameters list containing tags to expire cached
		///     entries.
		/// </param>
		/// <returns>The result of the query.</returns>
		public static Task<List<T>> FromCacheToListAsync<T>(this IQueryable<T> query, bool withChangeTracking, CancellationToken cancellationToken = default, params string[] tags)
			where T : class
			=> FromCacheToListAsync(query, withChangeTracking, null, cancellationToken, tags);

		/// <summary>
		///     Return the result of the <paramref name="query" /> from the cache. If the query is not cached
		///     yet, the query is materialized and cached before being returned.
		/// </summary>
		/// <typeparam name="T">The generic type of the query.</typeparam>
		/// <param name="query">The query to cache in the QueryCacheManager.</param>
		/// <param name="withChangeTracking">Indicates if the results of a query are tracked by the Microsoft.EntityFrameworkCore.ChangeTracking.ChangeTracker.</param>
		/// <param name="options">The cache entry options to use to cache the query.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <param name="tags">
		///     A variable-length parameters list containing tags to expire cached
		///     entries.
		/// </param>
		/// <returns>The result of the query.</returns>
		public static async Task<List<T>> FromCacheToListAsync<T>(this IQueryable<T> query, bool withChangeTracking, MemoryCacheEntryOptions? options, CancellationToken cancellationToken = default, params string[] tags)
			where T : class
		{
			query.TryGetDbContextQueryCacheManager(out QueryCacheManager? queryCacheManager);

			if (queryCacheManager == null || !queryCacheManager.IsEnabled)
			{
				if (withChangeTracking)
					return await query.ToListAsync(cancellationToken).ConfigureAwait(false);
				else
					return await query.AsNoTracking().ToListAsync(cancellationToken).ConfigureAwait(false);
			}

			var key = queryCacheManager.GetCacheKey(query, "ToListAsync", tags);

			if (!queryCacheManager.Cache!.TryGetValue(key, out object? item))
			{
				if (options == null)
					options = queryCacheManager.DefaultMemoryCacheEntryOptions ?? new MemoryCacheEntryOptions();

				options.RegisterPostEvictionCallback(queryCacheManager.RemoveCallback);
				item = withChangeTracking
					? await query.ToListAsync(cancellationToken).ConfigureAwait(false)
					: await query.AsNoTracking().ToListAsync(cancellationToken).ConfigureAwait(false);
				item = queryCacheManager.Cache.Set(key, item, options);
				queryCacheManager.AddCacheTag(key, tags);
				queryCacheManager.AddCacheTag(key, $"{typeof(T).Name}{queryCacheManager.CacheTypePostfix}");
			}

			if (item == null || item == DBNull.Value)
				return new List<T>();

			return (List<T>)item;
		}

		public static T? FromCacheFirstOrDefault<T>(this IQueryable<T> query, params string[] tags)
			where T : class
			=> FromCacheFirstOrDefault<T>(query, false, null, tags);

		public static T? FromCacheFirstOrDefault<T>(this IQueryable<T> query, bool withChangeTracking, params string[] tags)
			where T : class
			=> FromCacheFirstOrDefault<T>(query, withChangeTracking, null, tags);

		public static T? FromCacheFirstOrDefault<T>(this IQueryable<T> query, bool withChangeTracking, MemoryCacheEntryOptions? options, params string[] tags)
			where T : class
		{
			query.TryGetDbContextQueryCacheManager(out QueryCacheManager? queryCacheManager);

			if (queryCacheManager == null || !queryCacheManager.IsEnabled)
			{
				if (withChangeTracking)
					return query.FirstOrDefault();
				else
					return query.AsNoTracking().FirstOrDefault();
			}

			var key = queryCacheManager.GetCacheKey(query, "FirstOrDefault", tags);

			if (!queryCacheManager.Cache!.TryGetValue(key, out object? item))
			{
				if (options == null)
					options = queryCacheManager.DefaultMemoryCacheEntryOptions ?? new MemoryCacheEntryOptions();

				options.RegisterPostEvictionCallback(queryCacheManager.RemoveCallback);
				item = withChangeTracking
					? query.FirstOrDefault()
					: query.AsNoTracking().FirstOrDefault();
				item = queryCacheManager.Cache.Set(key, item, options);
				queryCacheManager.AddCacheTag(key, tags);
				queryCacheManager.AddCacheTag(key, $"{typeof(T).Name}{queryCacheManager.CacheTypePostfix}");
			}

			if (item == null || item == DBNull.Value)
				return default;

			return (T?)item;
		}

		public static Task<T?> FromCacheFirstOrDefaultAsync<T>(this IQueryable<T> query, CancellationToken cancellationToken = default, params string[] tags)
			where T : class
			=> FromCacheFirstOrDefaultAsync(query, false, null, cancellationToken, tags);

		public static Task<T?> FromCacheFirstOrDefaultAsync<T>(this IQueryable<T> query, bool withChangeTracking, CancellationToken cancellationToken = default, params string[] tags)
			where T : class
			=> FromCacheFirstOrDefaultAsync(query, withChangeTracking, null, cancellationToken, tags);

		public static async Task<T?> FromCacheFirstOrDefaultAsync<T>(this IQueryable<T> query, bool withChangeTracking, MemoryCacheEntryOptions? options, CancellationToken cancellationToken = default, params string[] tags)
			where T : class
		{
			query.TryGetDbContextQueryCacheManager(out QueryCacheManager? queryCacheManager);

			if (queryCacheManager == null || !queryCacheManager.IsEnabled)
			{
				if (withChangeTracking)
					return await query.FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
				else
					return await query.AsNoTracking().FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
			}

			var key = queryCacheManager.GetCacheKey(query, "FirstOrDefaultAsync", tags);

			if (!queryCacheManager.Cache!.TryGetValue(key, out object? item))
			{
				if (options == null)
					options = queryCacheManager.DefaultMemoryCacheEntryOptions ?? new MemoryCacheEntryOptions();

				options.RegisterPostEvictionCallback(queryCacheManager.RemoveCallback);
				item = withChangeTracking
					? await query.FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false)
					: await query.AsNoTracking().FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
				item = queryCacheManager.Cache.Set(key, item, options);
				queryCacheManager.AddCacheTag(key, tags);
				queryCacheManager.AddCacheTag(key, $"{typeof(T).Name}{queryCacheManager.CacheTypePostfix}");
			}

			if (item == null || item == DBNull.Value)
				return default;

			return (T?)item;
		}

		public static bool FromCacheAll<T>(this IQueryable<T> query, Expression<Func<T, bool>> predicate, params string[] tags)
			where T : class
			=> FromCacheAll<T>(query, predicate, null, tags);

		public static bool FromCacheAll<T>(this IQueryable<T> query, Expression<Func<T, bool>> predicate, MemoryCacheEntryOptions? options, params string[] tags)
			where T : class
		{
			query.TryGetDbContextQueryCacheManager(out QueryCacheManager? queryCacheManager);

			if (queryCacheManager == null || !queryCacheManager.IsEnabled)
			{
				return query.AsNoTracking().All(predicate);
			}

			var key = queryCacheManager.GetCacheKey(query, "All", tags);

			if (!queryCacheManager.Cache!.TryGetValue(key, out object? item))
			{
				if (options == null)
					options = queryCacheManager.DefaultMemoryCacheEntryOptions ?? new MemoryCacheEntryOptions();

				options.RegisterPostEvictionCallback(queryCacheManager.RemoveCallback);
				item = query.AsNoTracking().All(predicate);
				item = queryCacheManager.Cache.Set(key, item, options);
				queryCacheManager.AddCacheTag(key, tags);
				queryCacheManager.AddCacheTag(key, $"{typeof(T).Name}{queryCacheManager.CacheTypePostfix}");
			}

			if (item == null || item == DBNull.Value)
				return default;

			return (bool)item;
		}

		public static Task<bool> FromCacheAllAsync<T>(this IQueryable<T> query, Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default, params string[] tags)
			where T : class
			=> FromCacheAllAsync(query, predicate, null, cancellationToken, tags);

		public static async Task<bool> FromCacheAllAsync<T>(this IQueryable<T> query, Expression<Func<T, bool>> predicate, MemoryCacheEntryOptions? options, CancellationToken cancellationToken = default, params string[] tags)
			where T : class
		{
			query.TryGetDbContextQueryCacheManager(out QueryCacheManager? queryCacheManager);

			if (queryCacheManager == null || !queryCacheManager.IsEnabled)
			{
				return await query.AsNoTracking().AllAsync(predicate, cancellationToken).ConfigureAwait(false);
			}

			var key = queryCacheManager.GetCacheKey(query, "AllAsync", tags);

			if (!queryCacheManager.Cache!.TryGetValue(key, out object? item))
			{
				if (options == null)
					options = queryCacheManager.DefaultMemoryCacheEntryOptions ?? new MemoryCacheEntryOptions();

				options.RegisterPostEvictionCallback(queryCacheManager.RemoveCallback);
				item = await query.AsNoTracking().AllAsync(predicate, cancellationToken).ConfigureAwait(false);
				item = queryCacheManager.Cache.Set(key, item, options);
				queryCacheManager.AddCacheTag(key, tags);
				queryCacheManager.AddCacheTag(key, $"{typeof(T).Name}{queryCacheManager.CacheTypePostfix}");
			}

			if (item == null || item == DBNull.Value)
				return default;

			return (bool)item;
		}

		public static bool FromCacheAny<T>(this IQueryable<T> query, params string[] tags)
			where T : class
			=> FromCacheAny<T>(query, (MemoryCacheEntryOptions?)null, tags);

		public static bool FromCacheAny<T>(this IQueryable<T> query, MemoryCacheEntryOptions? options, params string[] tags)
			where T : class
		{
			query.TryGetDbContextQueryCacheManager(out QueryCacheManager? queryCacheManager);

			if (queryCacheManager == null || !queryCacheManager.IsEnabled)
			{
				return query.AsNoTracking().Any();
			}

			var key = queryCacheManager.GetCacheKey(query, "Any", tags);

			if (!queryCacheManager.Cache!.TryGetValue(key, out object? item))
			{
				if (options == null)
					options = queryCacheManager.DefaultMemoryCacheEntryOptions ?? new MemoryCacheEntryOptions();

				options.RegisterPostEvictionCallback(queryCacheManager.RemoveCallback);
				item = query.AsNoTracking().Any();
				item = queryCacheManager.Cache.Set(key, item, options);
				queryCacheManager.AddCacheTag(key, tags);
				queryCacheManager.AddCacheTag(key, $"{typeof(T).Name}{queryCacheManager.CacheTypePostfix}");
			}

			if (item == null || item == DBNull.Value)
				return default;

			return (bool)item;
		}

		public static Task<bool> FromCacheAnyAsync<T>(this IQueryable<T> query, CancellationToken cancellationToken = default, params string[] tags)
			where T : class
			=> FromCacheAnyAsync(query, (MemoryCacheEntryOptions?)null, cancellationToken, tags);

		public static async Task<bool> FromCacheAnyAsync<T>(this IQueryable<T> query, MemoryCacheEntryOptions? options, CancellationToken cancellationToken = default, params string[] tags)
			where T : class
		{
			query.TryGetDbContextQueryCacheManager(out QueryCacheManager? queryCacheManager);

			if (queryCacheManager == null || !queryCacheManager.IsEnabled)
			{
				return await query.AsNoTracking().AnyAsync(cancellationToken).ConfigureAwait(false);
			}

			var key = queryCacheManager.GetCacheKey(query, "AnyAsync", tags);

			if (!queryCacheManager.Cache!.TryGetValue(key, out object? item))
			{
				if (options == null)
					options = queryCacheManager.DefaultMemoryCacheEntryOptions ?? new MemoryCacheEntryOptions();

				options.RegisterPostEvictionCallback(queryCacheManager.RemoveCallback);
				item = await query.AsNoTracking().AnyAsync(cancellationToken).ConfigureAwait(false);
				item = queryCacheManager.Cache.Set(key, item, options);
				queryCacheManager.AddCacheTag(key, tags);
				queryCacheManager.AddCacheTag(key, $"{typeof(T).Name}{queryCacheManager.CacheTypePostfix}");
			}

			if (item == null || item == DBNull.Value)
				return default;

			return (bool)item;
		}

		public static bool FromCacheAny<T>(this IQueryable<T> query, Expression<Func<T, bool>> predicate, params string[] tags)
			where T : class
			=> FromCacheAny<T>(query, predicate, null, tags);

		public static bool FromCacheAny<T>(this IQueryable<T> query, Expression<Func<T, bool>> predicate, MemoryCacheEntryOptions? options, params string[] tags)
			where T : class
		{
			query.TryGetDbContextQueryCacheManager(out QueryCacheManager? queryCacheManager);

			if (queryCacheManager == null || !queryCacheManager.IsEnabled)
			{
				return query.AsNoTracking().Any(predicate);
			}

			var key = queryCacheManager.GetCacheKey(query, "Any_Predicate", tags);

			if (!queryCacheManager.Cache!.TryGetValue(key, out object? item))
			{
				if (options == null)
					options = queryCacheManager.DefaultMemoryCacheEntryOptions ?? new MemoryCacheEntryOptions();

				options.RegisterPostEvictionCallback(queryCacheManager.RemoveCallback);
				item = query.AsNoTracking().Any(predicate);
				item = queryCacheManager.Cache.Set(key, item, options);
				queryCacheManager.AddCacheTag(key, tags);
				queryCacheManager.AddCacheTag(key, $"{typeof(T).Name}{queryCacheManager.CacheTypePostfix}");
			}

			if (item == null || item == DBNull.Value)
				return default;

			return (bool)item;
		}

		public static Task<bool> FromCacheAnyAsync<T>(this IQueryable<T> query, Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default, params string[] tags)
			where T : class
			=> FromCacheAnyAsync(query, predicate, null, cancellationToken, tags);

		public static async Task<bool> FromCacheAnyAsync<T>(this IQueryable<T> query, Expression<Func<T, bool>> predicate, MemoryCacheEntryOptions? options, CancellationToken cancellationToken = default, params string[] tags)
			where T : class
		{
			query.TryGetDbContextQueryCacheManager(out QueryCacheManager? queryCacheManager);

			if (queryCacheManager == null || !queryCacheManager.IsEnabled)
			{
				return await query.AsNoTracking().AnyAsync(predicate, cancellationToken).ConfigureAwait(false);
			}

			var key = queryCacheManager.GetCacheKey(query, "AnyAsync_Predicate", tags);

			if (!queryCacheManager.Cache!.TryGetValue(key, out object? item))
			{
				if (options == null)
					options = queryCacheManager.DefaultMemoryCacheEntryOptions ?? new MemoryCacheEntryOptions();

				options.RegisterPostEvictionCallback(queryCacheManager.RemoveCallback);
				item = await query.AsNoTracking().AnyAsync(predicate, cancellationToken).ConfigureAwait(false);
				item = queryCacheManager.Cache.Set(key, item, options);
				queryCacheManager.AddCacheTag(key, tags);
				queryCacheManager.AddCacheTag(key, $"{typeof(T).Name}{queryCacheManager.CacheTypePostfix}");
			}

			if (item == null || item == DBNull.Value)
				return default;

			return (bool)item;
		}

		public static int FromCacheCount<T>(this IQueryable<T> query, params string[] tags)
			where T : class
			=> FromCacheCount<T>(query, (MemoryCacheEntryOptions?)null, tags);

		public static int FromCacheCount<T>(this IQueryable<T> query, MemoryCacheEntryOptions? options, params string[] tags)
			where T : class
		{
			query.TryGetDbContextQueryCacheManager(out QueryCacheManager? queryCacheManager);

			if (queryCacheManager == null || !queryCacheManager.IsEnabled)
			{
				return query.AsNoTracking().Count();
			}

			var key = queryCacheManager.GetCacheKey(query, "Count", tags);

			if (!queryCacheManager.Cache!.TryGetValue(key, out object? item))
			{
				if (options == null)
					options = queryCacheManager.DefaultMemoryCacheEntryOptions ?? new MemoryCacheEntryOptions();

				options.RegisterPostEvictionCallback(queryCacheManager.RemoveCallback);
				item = query.AsNoTracking().Count();
				item = queryCacheManager.Cache.Set(key, item, options);
				queryCacheManager.AddCacheTag(key, tags);
				queryCacheManager.AddCacheTag(key, $"{typeof(T).Name}{queryCacheManager.CacheTypePostfix}");
			}

			if (item == null || item == DBNull.Value)
				return default;

			return (int)item;
		}

		public static Task<int> FromCacheCountAsync<T>(this IQueryable<T> query, CancellationToken cancellationToken = default, params string[] tags)
			where T : class
			=> FromCacheCountAsync(query, (MemoryCacheEntryOptions?)null, cancellationToken, tags);

		public static async Task<int> FromCacheCountAsync<T>(this IQueryable<T> query, MemoryCacheEntryOptions? options, CancellationToken cancellationToken = default, params string[] tags)
			where T : class
		{
			query.TryGetDbContextQueryCacheManager(out QueryCacheManager? queryCacheManager);

			if (queryCacheManager == null || !queryCacheManager.IsEnabled)
			{
				return await query.AsNoTracking().CountAsync(cancellationToken).ConfigureAwait(false);
			}

			var key = queryCacheManager.GetCacheKey(query, "CountAsync", tags);

			if (!queryCacheManager.Cache!.TryGetValue(key, out object? item))
			{
				if (options == null)
					options = queryCacheManager.DefaultMemoryCacheEntryOptions ?? new MemoryCacheEntryOptions();

				options.RegisterPostEvictionCallback(queryCacheManager.RemoveCallback);
				item = await query.AsNoTracking().CountAsync(cancellationToken).ConfigureAwait(false);
				item = queryCacheManager.Cache.Set(key, item, options);
				queryCacheManager.AddCacheTag(key, tags);
				queryCacheManager.AddCacheTag(key, $"{typeof(T).Name}{queryCacheManager.CacheTypePostfix}");
			}

			if (item == null || item == DBNull.Value)
				return default;

			return (int)item;
		}

		public static int FromCacheCount<T>(this IQueryable<T> query, Expression<Func<T, bool>> predicate, params string[] tags)
			where T : class
			=> FromCacheCount<T>(query, predicate, null, tags);

		public static int FromCacheCount<T>(this IQueryable<T> query, Expression<Func<T, bool>> predicate, MemoryCacheEntryOptions? options, params string[] tags)
			where T : class
		{
			query.TryGetDbContextQueryCacheManager(out QueryCacheManager? queryCacheManager);

			if (queryCacheManager == null || !queryCacheManager.IsEnabled)
			{
				return query.AsNoTracking().Count(predicate);
			}

			var key = queryCacheManager.GetCacheKey(query, "Count_Predicate", tags);

			if (!queryCacheManager.Cache!.TryGetValue(key, out object? item))
			{
				if (options == null)
					options = queryCacheManager.DefaultMemoryCacheEntryOptions ?? new MemoryCacheEntryOptions();

				options.RegisterPostEvictionCallback(queryCacheManager.RemoveCallback);
				item = query.AsNoTracking().Count(predicate);
				item = queryCacheManager.Cache.Set(key, item, options);
				queryCacheManager.AddCacheTag(key, tags);
				queryCacheManager.AddCacheTag(key, $"{typeof(T).Name}{queryCacheManager.CacheTypePostfix}");
			}

			if (item == null || item == DBNull.Value)
				return default;

			return (int)item;
		}

		public static Task<int> FromCacheCountAsync<T>(this IQueryable<T> query, Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default, params string[] tags)
			where T : class
			=> FromCacheCountAsync(query, predicate, null, cancellationToken, tags);

		public static async Task<int> FromCacheCountAsync<T>(this IQueryable<T> query, Expression<Func<T, bool>> predicate, MemoryCacheEntryOptions? options, CancellationToken cancellationToken = default, params string[] tags)
			where T : class
		{
			query.TryGetDbContextQueryCacheManager(out QueryCacheManager? queryCacheManager);

			if (queryCacheManager == null || !queryCacheManager.IsEnabled)
			{
				return await query.AsNoTracking().CountAsync(predicate, cancellationToken).ConfigureAwait(false);
			}

			var key = queryCacheManager.GetCacheKey(query, "CountAsync_Predicate", tags);

			if (!queryCacheManager.Cache!.TryGetValue(key, out object? item))
			{
				if (options == null)
					options = queryCacheManager.DefaultMemoryCacheEntryOptions ?? new MemoryCacheEntryOptions();

				options.RegisterPostEvictionCallback(queryCacheManager.RemoveCallback);
				item = await query.AsNoTracking().CountAsync(predicate, cancellationToken).ConfigureAwait(false);
				item = queryCacheManager.Cache.Set(key, item, options);
				queryCacheManager.AddCacheTag(key, tags);
				queryCacheManager.AddCacheTag(key, $"{typeof(T).Name}{queryCacheManager.CacheTypePostfix}");
			}

			if (item == null || item == DBNull.Value)
				return default;

			return (int)item;
		}
	}
}

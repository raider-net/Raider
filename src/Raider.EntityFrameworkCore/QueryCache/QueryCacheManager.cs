using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Raider.Extensions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Raider.EntityFrameworkCore.QueryCache
{
	public class QueryCacheManager: IDisposable
	{
		/// <summary>Gets the dictionary cache tags used to store tags and corresponding cached keys.</summary>
		/// <value>The cache tags used to store tags and corresponding cached keys.</value>
		private readonly ConcurrentDictionary<string, List<string>> _cacheTags; //ConcurrentDictionary<tag, List<cacheKey>>
		private readonly ConcurrentDictionary<string, List<string>> _cacheKeys; //ConcurrentDictionary<cacheKey, List<tag>>

		private MemoryCacheEntryOptions? _defaultMemoryCacheEntryOptions;
		private Func<MemoryCacheEntryOptions>? _memoryCacheEntryOptionsFactory;

		private IMemoryCache? _cache;
		private bool disposedValue;

		/// <summary>Gets or sets the cache to use for the QueryCacheExtensions extension methods.</summary>
		/// <value>The cache to use for the QueryCacheExtensions extension methods.</value>
		public IMemoryCache? Cache
		{
			get => _cache;
			set
			{
				if (value == null)
					throw new ArgumentNullException(nameof(value), $"{nameof(Cache)} cannot be null.");

				try
				{
					_cache?.Dispose();
				}
				catch { }
				_cache = value;
			}
		}

		internal QueryCacheManager(bool isEnabled)
		{
			if (isEnabled)
				_cache = new MemoryCache(new MemoryCacheOptions());

			IsEnabled = isEnabled;
			DefaultMemoryCacheEntryOptions = new MemoryCacheEntryOptions();
			CachePrefix = "Raider.EntityFrameworkCore.QueryCache.QueryCacheManager;";
			CacheTypePostfix = "_Raider_QueryCacheManager_CacheType";
			_cacheTags = new ConcurrentDictionary<string, List<string>>();
			_cacheKeys = new ConcurrentDictionary<string, List<string>>();
			IncludeConnectionInCacheKey = true;
		}

		/// <summary>Gets or sets the cache prefix to use to create the cache key.</summary>
		/// <value>The cache prefix to use to create the cache key.</value>
		public string CachePrefix { get; set; }

		/// <summary>Gets or sets the cache type suffix.</summary>
		/// <value>The cache type suffix.</value>
		public string CacheTypePostfix { get; set; }

		/// <summary>Gets or sets the cache key factory.</summary>
		/// <value>The cache key factory.</value>
		public Func<IQueryable, string[], string>? CacheKeyFactory { get; set; }

		private bool _isEnabled;
		/// <summary>
		///     Gets or sets a value indicating whether the Query Cache is enabled
		/// </summary>
		/// <value>true if the Query Cache is enabled.</value>
		public bool IsEnabled
		{
			get => _isEnabled;
			set
			{
				var enabled = value;
				if (enabled && _cache == null)
				{
					_cache = new MemoryCache(new MemoryCacheOptions());
				}
				_isEnabled = enabled;
			}
		}

		/// <summary>
		///     Gets or sets a value indicating whether the connection in cache key should be included.
		/// </summary>
		/// <value>true if include connection in cache key, false if not.</value>
		public bool IncludeConnectionInCacheKey { get; set; }

		/// <summary>
		///     Gets or sets a value indicating whether this object use first tag as cache key.
		/// </summary>
		/// <value>true if use first tag as cache key, false if not.</value>
		public bool UseFirstTagAsCacheKey { get; set; }

		/// <summary>
		///     Gets or sets a value indicating whether this object use tag as cache key.
		/// </summary>
		/// <value>true if use tag as cache key, false if not.</value>
		public bool UseTagsAsCacheKey { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether this object is command information optional for cache
		/// key.
		/// </summary>
		/// <value>
		/// True if this object is command information optional for cache key, false if not.
		/// </value>
		public bool IsCommandInfoOptionalForCacheKey { get; set; }

		/// <summary>Gets or sets the default memory cache entry options to use when no policy is specified.</summary>
		/// <value>The default memory cache entry options to use when no policy is specified.</value>
		public MemoryCacheEntryOptions? DefaultMemoryCacheEntryOptions
		{
			get
			{
				if (_defaultMemoryCacheEntryOptions == null && MemoryCacheEntryOptionsFactory != null)
				{
					return MemoryCacheEntryOptionsFactory();
				}

				return _defaultMemoryCacheEntryOptions;
			}
			set
			{
				_defaultMemoryCacheEntryOptions = value;
				_memoryCacheEntryOptionsFactory = null;
			}
		}

		/// <summary>Gets or sets the memory cache entry options factory.</summary>
		/// <value>The memory cache entry options factory.</value>
		public Func<MemoryCacheEntryOptions>? MemoryCacheEntryOptionsFactory
		{
			get { return _memoryCacheEntryOptionsFactory; }
			set
			{
				_memoryCacheEntryOptionsFactory = value;
				_defaultMemoryCacheEntryOptions = null;
			}
		}

		/// <summary>Adds cache tags corresponding to a cached key in the CacheTags dictionary.</summary>
		/// <param name="cacheKey">The cache key.</param>
		/// <param name="tags">A variable-length parameters list containing tags corresponding to the <paramref name="cacheKey" />.</param>
		internal void AddCacheTag(string cacheKey, params string[] tags)
		{
			var newTags = tags.Select(tag => $"{CachePrefix}{tag}").ToList();
			_cacheKeys.AddOrUpdate(cacheKey, x => newTags, (x, list) =>
			{
				lock (list)
				{
					foreach (var tag in newTags)
					{
						if (!list.Contains(tag))
							list.Add(tag);
					}
				}

				return list;
			});

			foreach (var tag in newTags)
			{
				_cacheTags.AddOrUpdate($"{CachePrefix}{tag}", x => new List<string> { cacheKey }, (x, list) =>
				{
					lock (list)
					{
						// never lock something related to this list elsewhere or ensure we don't create a deadlock
						if (!list.Contains(cacheKey))
							list.Add(cacheKey);
					}

					return list;
				});
			}
		}

		/// <summary>Expire all cached objects && tag.</summary>
		public void ExpireAll()
		{
			if (!IsEnabled)
				return;

			var keys = _cacheKeys.Keys.ToList();
			foreach (var key in keys)
			{
				_cacheKeys.TryRemove(key, out _);
				Cache!.Remove(key);
			}

			var tags = _cacheTags.Keys.ToList();

			// We do not use ExpireTag because type doesn't have CachePrefix
			foreach (var tag in tags)
			{
				if (_cacheTags.TryRemove(tag, out List<string>? keysList))
				{
					// never lock something related to this list elsewhere or ensure we don't create a deadlock
					lock (keysList)
					{
						foreach (var key in keysList)
						{
							Cache!.Remove(key);
						}
					}
				}
			}
		}

		/// <summary>Expire type.</summary>
		public void ExpireType<T>()
		{
			ExpireType(typeof(T));
		}

		/// <summary>Expire type.</summary>
		/// <param name="type">The type.</param>
		public void ExpireType(Type type)
		{
			ExpireTag($"{type.Name}{CacheTypePostfix}");
		}

		/// <summary>Expire all cached keys linked to specified tags.</summary>
		/// <param name="tags">
		///     A variable-length parameters list containing tag to expire linked cache
		///     key.
		/// </param>
		public void ExpireTag(params string[] tags)
		{
			if (!IsEnabled)
				return;

			foreach (var tag in tags)
			{
				if (_cacheTags.TryRemove($"{CachePrefix}{tag}", out List<string>? keysList))
				{
					// never lock something related to this list elsewhere or ensure we don't create a deadlock
					lock (keysList)
					{
						foreach (var key in keysList)
						{
							Cache!.Remove(key);
							_cacheKeys.TryRemove(key, out _);
						}
					}
				}
			}
		}

		/// <summary>Gets cached keys used to cache or retrieve a query from the QueryCacheManager.</summary>
		/// <param name="query">The query to cache or retrieve from the QueryCacheManager.</param>
		/// <param name="tags">A variable-length parameters list containing tags to create the cache key.</param>
		/// <returns>The cache key used to cache or retrieve a query from the QueryCacheManager.</returns>
		internal string GetCacheKey<T>(IQueryable<T> query, string methodName, string[] tags)
			where T : class
		{
			if (CacheKeyFactory != null)
			{
				var cacheKey = CacheKeyFactory(query, tags);

				if (!string.IsNullOrEmpty(cacheKey))
				{
					return cacheKey;
				}
			}

			var sb = new StringBuilder();

			sb.AppendLine(CachePrefix);
			sb.AppendLine(methodName);

			if (IncludeConnectionInCacheKey)
			{
				var queryContext = query.GetRelationalQueryContext();
				if (queryContext != null)
					sb.AppendLine(GlobalQueryCacheManager.GetConnectionStringForCacheKey(queryContext));
			}

			if (UseFirstTagAsCacheKey)
			{
				if (tags == null || tags.Length == 0 || string.IsNullOrEmpty(tags[0]))
				{
					throw new Exception("The option 'UseFirstTagAsCacheKey' is enabled but we found no tag, an empty tag string, or a null tag string instead. Make sure a tag is provided, and it's not null or empty.");
				}

				sb.AppendLine(tags[0]);
				return sb.ToString();
			}

			if (UseTagsAsCacheKey)
			{
				if (tags == null || tags.Length == 0 || tags.Any(string.IsNullOrEmpty))
				{
					throw new Exception("The option 'UseTagsAsCacheKey' is enabled but we found no tag, an empty tag string, or a null tag string instead. Make sure a tag is provided, and it's not null or empty.");
				}

				sb.AppendLine(string.Join(";", tags));
				return sb.ToString();
			}

			sb.AppendLine(string.Join(";", tags));

			sb.AppendLine(query.Expression.ToString());
			sb.AppendLine(query.ToQueryString());

			return sb.ToString();
		}

		internal void RemoveCallback(object key, object value, EvictionReason reason, object state)
		{
			var cacheKey = key.ToString();
			if (cacheKey == null)
				return;

			if (_cacheKeys.TryRemove(cacheKey, out List<string>? tags))
			{
				foreach (var tag in tags)
					_cacheTags.TryRemove(tag, out _);
			}
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					try
					{
						Cache?.Dispose();
					}
					catch { }
				}

				disposedValue = true;
			}
		}

		public void Dispose()
		{
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}
	}
}

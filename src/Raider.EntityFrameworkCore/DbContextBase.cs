using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Raider.EntityFrameworkCore.Concurrence;
using Raider.EntityFrameworkCore.Database;
using Raider.EntityFrameworkCore.QueryCache;
using Raider.EntityFrameworkCore.Synchronyzation;
using Raider.Logging.SerilogEx;
using Raider.Trace;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Raider.EntityFrameworkCore
{
	public abstract class DbContextBase : Microsoft.EntityFrameworkCore.DbContext
	{
		protected readonly IApplicationContext _applicationContext;
		protected readonly ILogger _logger;

		protected DbConnection? ExternalDbConnection { get; private set; }
		protected string? ExternalConnectionString { get; private set; }
		protected internal QueryCacheManager QueryCacheManager { get; private set; }

		private DbConnection? dbConnection;
		public DbConnection DbConnection
		{
			get
			{
				if (dbConnection == null)
					dbConnection = this.Database.GetDbConnection();

				return dbConnection;
			}
		}

		public IDbContextTransaction? DbContextTransaction => Database?.CurrentTransaction;
		public IDbTransaction? DbTransaction => Database?.CurrentTransaction?.GetDbTransaction();

		private string? _dbConnectionString;
		public string DBConnectionString
		{
			get
			{

				if (_dbConnectionString == null)
					_dbConnectionString = DbConnection.ConnectionString;

				return _dbConnectionString;
			}
		}

		public DbContextBase(DbContextOptions options, ILogger logger, IApplicationContext appContext/*, disabledEtitiesFromAudit, disabledEtityPropertiesFromAudit*/)
			: base(options)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_applicationContext = appContext ?? throw new ArgumentNullException(nameof(appContext));
			QueryCacheManager = new QueryCacheManager(false);
		}

		protected DbContextBase(ILogger logger, IApplicationContext appContext)
			: base()
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_applicationContext = appContext ?? throw new ArgumentNullException(nameof(appContext));
			QueryCacheManager = new QueryCacheManager(false);
		}

		private bool initialized = false;
		private readonly object _initLock = new();
		internal void Initialize(DbConnection? externalDbConnection, string? externalConnectionString)
		{
			if (initialized)
				return;

			lock (_initLock)
			{
				if (initialized)
					return;

				ExternalDbConnection = externalDbConnection;
				ExternalConnectionString = externalConnectionString;

				initialized = true;
			}
		}
		
#pragma warning disable CS0809 // Obsolete member overrides non-obsolete member
		[Obsolete("Use Save() method instead.", true)]
		public override int SaveChanges()
		{
			throw new NotSupportedException($"Use {nameof(Save)}() method instead.");
		}

		[Obsolete("Use Save(bool acceptAllChangesOnSuccess) method instead.", true)]
		public override int SaveChanges(bool acceptAllChangesOnSuccess)
		{
			throw new NotSupportedException($"Use {nameof(Save)}(bool {nameof(acceptAllChangesOnSuccess)}) method instead.");
		}

		[Obsolete("Use SaveAsync(CancellationToken cancellationToken) method instead.", true)]
		public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
		{
			throw new NotSupportedException($"Use {nameof(SaveAsync)}({nameof(CancellationToken)} {nameof(cancellationToken)}) method instead.");
		}

		[Obsolete("Use SaveAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken) method instead.", true)]
		public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
		{
			throw new NotSupportedException($"Use {nameof(SaveAsync)}(bool {nameof(acceptAllChangesOnSuccess)}, {nameof(CancellationToken)} {nameof(cancellationToken)}) method instead.");
		}
#pragma warning restore CS0809 // Obsolete member overrides non-obsolete member

		public virtual int Save(
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
			=> Save(true, null, memberName, sourceFilePath, sourceLineNumber);

		public virtual int Save(
			SaveOptions? options,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
			=> Save(true, options, memberName, sourceFilePath, sourceLineNumber);

		public virtual int Save(
			bool acceptAllChangesOnSuccess,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
			=> Save(acceptAllChangesOnSuccess, null, memberName, sourceFilePath, sourceLineNumber);

		public virtual int Save(
			bool acceptAllChangesOnSuccess,
			SaveOptions? options,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
		{
			using var disposable = CreateDbLogScope(memberName, sourceFilePath, sourceLineNumber);
			SetEntities(options);

			return base.SaveChanges(acceptAllChangesOnSuccess);
		}

		public virtual Task<int> SaveAsync(
			CancellationToken cancellationToken = default,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
			=> SaveAsync(true, null, cancellationToken, memberName, sourceFilePath, sourceLineNumber);

		public virtual Task<int> SaveAsync(
			SaveOptions? options,
			CancellationToken cancellationToken = default,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
			=> SaveAsync(true, options, cancellationToken, memberName, sourceFilePath, sourceLineNumber);

		public virtual Task<int> SaveAsync(
			bool acceptAllChangesOnSuccess,
			CancellationToken cancellationToken = default,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
			=> SaveAsync(acceptAllChangesOnSuccess, null, cancellationToken, memberName, sourceFilePath, sourceLineNumber);

		public virtual async Task<int> SaveAsync(
			bool acceptAllChangesOnSuccess,
			SaveOptions? options,
			CancellationToken cancellationToken = default,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
		{
			using var disposable = CreateDbLogScope(memberName, sourceFilePath, sourceLineNumber);
			SetEntities(options);

			return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
		}

		protected int SaveWithoutScope(bool acceptAllChangesOnSuccess)
			=> SaveWithoutScope(acceptAllChangesOnSuccess, null);

		protected int SaveWithoutScope(bool acceptAllChangesOnSuccess, SaveOptions? options)
		{
			SetEntities(options);
			return base.SaveChanges(acceptAllChangesOnSuccess);
		}

		protected Task<int> SaveWithoutScopeAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
			=> SaveWithoutScopeAsync(acceptAllChangesOnSuccess, null, cancellationToken);

		protected async Task<int> SaveWithoutScopeAsync(bool acceptAllChangesOnSuccess, SaveOptions? options, CancellationToken cancellationToken = default)
		{
			SetEntities(options);
			return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
		}

		protected IDisposable CreateDbLogScope(
			string memberName,
			string sourceFilePath,
			int sourceLineNumber)
		{
			var traceFrame =
				new TraceFrameBuilder(_applicationContext.TraceInfo.TraceFrame)
					.CallerMemberName(memberName)
					.CallerFilePath(sourceFilePath)
					.CallerLineNumber(sourceLineNumber == 0 ? (int?)null : sourceLineNumber)
					.Build();

			var disposable = _logger.BeginScope(new Dictionary<string, object?>
			{
				[nameof(_applicationContext.TraceInfo.TraceFrame)] = traceFrame.ToString(),
				[nameof(_applicationContext.TraceInfo.CorrelationId)] = _applicationContext.TraceInfo.CorrelationId,
				[LogEventHelper.IS_DB_LOG] = true
			});

			return disposable;
		}

		protected static void RegisterUnaccentFunction(ModelBuilder modelBuilder)
		{
			modelBuilder
				.HasDbFunction(() => DbFunc.Unaccent(default))
				.HasName("unaccent");
		}

		protected virtual void SetEntities(SaveOptions? options)
		{
			if (options != null && !options.SetConcurrencyToken && !options.SetSyncToken)
				return;

			ChangeTracker.DetectChanges();

			foreach (var entry in ChangeTracker.Entries())
			{
				if (entry.State == EntityState.Detached || entry.State == EntityState.Unchanged)
					continue;

				if ((options == null || options.SetConcurrencyToken) && entry.Entity is IConcurrent concurrent)
				{
					switch (entry.State)
					{
						case EntityState.Added:
						case EntityState.Modified:
							concurrent.ConcurrencyToken = Guid.NewGuid();
							break;

						default:
							break;
					}
				}

				if ((options == null || options.SetSyncToken) && entry.Entity is ISynchronizable synchronizable)
				{
					switch (entry.State)
					{
						case EntityState.Added:
							synchronizable.SyncToken = Guid.NewGuid();
							break;
						case EntityState.Modified:
							if (WasModifiedNotIgnorredProperty(entry, synchronizable))
								synchronizable.SyncToken = Guid.NewGuid();
							break;

						default:
							break;
					}
				}
			}
		}

		protected bool WasModifiedNotIgnorredProperty(EntityEntry entry, ISynchronizable synchronizable)
		{
			if (entry == null || synchronizable == null)
				return false;

			var ignoredProperties = synchronizable.GetIgnoredSynchronizationProperties();
			if (ignoredProperties == null || ignoredProperties.Count == 0)
				return true;

			return entry.Properties.Any(prop => prop.IsModified && !ignoredProperties.Contains(prop.Metadata.Name));
		}

		private readonly object _configureDbContextCacheLock = new();
		public void ConfigureDbContextCache(Action<QueryCacheManager> configure, bool force)
		{
			if (configure == null || (!force && QueryCacheManager.IsEnabled))
				return;

			lock (_configureDbContextCacheLock)
			{
				if (force || !QueryCacheManager.IsEnabled)
					configure(QueryCacheManager);
			}
		}

		public void EnableDbContextCache()
			=> ConfigureDbContextCache(c => c.IsEnabled = true, false);

		public override void Dispose()
		{
			QueryCacheManager.Dispose();
			base.Dispose();
		}

		public override ValueTask DisposeAsync()
		{
			QueryCacheManager.Dispose();
			return base.DisposeAsync();
		}
	}
}

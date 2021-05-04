using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Raider.EntityFrameworkCore.Database;
using Raider.Logging.SerilogEx;
using Raider.Trace;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;


namespace Raider.EntityFrameworkCore
{
	public abstract class DbContextBase : Microsoft.EntityFrameworkCore.DbContext
	{
		protected readonly IApplicationContext _applicationContext;
		protected readonly ILogger _logger;

		protected string? connectionString;

		private DbConnection dbConnection;
		public DbConnection DbConnection
		{
			get
			{
				if (dbConnection == null)
					dbConnection = this.Database.GetDbConnection();

				return dbConnection;
			}
		}

		public IDbTransaction? DbTransaction => Database?.CurrentTransaction?.GetDbTransaction();

		private string _dbConnectionString;
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
		}

		protected DbContextBase(ILogger logger, IApplicationContext appContext)
			: base()
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_applicationContext = appContext ?? throw new ArgumentNullException(nameof(appContext));
		}

		public virtual int SaveChanges(
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
		{
			return SaveChanges(true, memberName, sourceFilePath, sourceLineNumber);
		}

		public virtual Task<int> SaveChangesAsync(CancellationToken cancellationToken = default,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
		{
			return SaveChangesAsync(true, cancellationToken, memberName, sourceFilePath, sourceLineNumber);
		}

		public virtual int SaveChanges(bool acceptAllChangesOnSuccess,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
		{
			using var disposable = CreateDbLogScope(memberName, sourceFilePath, sourceLineNumber);
			return base.SaveChanges(acceptAllChangesOnSuccess);
		}

		public virtual async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
		{
			using var disposable = CreateDbLogScope(memberName, sourceFilePath, sourceLineNumber);
			return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
		}

		protected int SaveChangesWithoutScope(bool acceptAllChangesOnSuccess)
		{
			return base.SaveChanges(acceptAllChangesOnSuccess);
		}

		protected async Task<int> SaveChangesWithoutScopeAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
		{
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

		protected void RegisterUnaccentFunction(ModelBuilder modelBuilder)
		{
			modelBuilder
				.HasDbFunction(() => DbFunc.Unaccent(default));
		}
	}
}

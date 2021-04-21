﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Raider.EntityFrameworkCore.Database;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;


namespace Raider.EntityFrameworkCore
{
	public abstract class DbContextBase : Microsoft.EntityFrameworkCore.DbContext
	{
		protected readonly IApplicationContext _applicationContext;
		protected readonly ILogger _logger;
		protected readonly int? _userId;

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
			_userId = _applicationContext.TraceInfo.IdUser;
		}

		protected DbContextBase(ILogger logger, IApplicationContext appContext)
			: base()
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_applicationContext = appContext ?? throw new ArgumentNullException(nameof(appContext));
			_userId = _applicationContext.TraceInfo.IdUser;
		}

		public override int SaveChanges(bool acceptAllChangesOnSuccess)
		{
			using (_logger.BeginScope(new Dictionary<string, object> { { "EF LOGGER SCOPE BY TOM", "VALUE TOM TOM" } })) //TODO
			{
				return base.SaveChanges(acceptAllChangesOnSuccess);
			}
		}

		public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
		{
			using (_logger.BeginScope(new Dictionary<string, object> { { "EF LOGGER SCOPE BY TOM", "VALUE TOM TOM ASYNC" } })) //TODO
			{
				return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
			}
		}

		protected void RegisterUnaccentFunction(ModelBuilder modelBuilder)
		{
			modelBuilder
				.HasDbFunction(() => DbFunc.Unaccent(default));
		}
	}
}

﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Raider.EntityFrameworkCore;
using Raider.Services.EntityFramework.Commands;
using System;
using System.Data;

namespace Raider.Services.EntityFramework
{
	internal static class DbContextExtensions
	{
		public static DbContext CheckDbTransaction(this DbContext dbContext, TransactionUsage transactionUsage)
		{
			if (transactionUsage == TransactionUsage.NONE && dbContext.Database.CurrentTransaction != null)
				throw new InvalidOperationException($"DbContext has transaction, but expected {nameof(TransactionUsage)} is {transactionUsage}");

			if (transactionUsage == TransactionUsage.ReuseOrCreateNew && dbContext.Database.CurrentTransaction == null)
				throw new InvalidOperationException($"DbContext has no transaction, but expected {nameof(TransactionUsage)} is {transactionUsage}");

			return dbContext;
		}
	}

	public class DbServiceContext : Raider.Services.ServiceContext, IServiceContext, IDbCommandServiceContext
	{
		private DbCommandHandlerContext? _commandHandlerContext;

		public IDbContextTransaction? DbContextTransaction => _commandHandlerContext?.DbContextTransaction;

		public DbServiceContext()
			: base()
		{
		}

		protected override void OnSetCommandHandlerContext(Services.Commands.CommandHandlerContext commandHandlerContext)
		{
			_commandHandlerContext = (DbCommandHandlerContext)commandHandlerContext;
		}

		public TContext CreateNewDbContext<TContext>(
			TransactionUsage transactionUsage = TransactionUsage.ReuseOrCreateNew,
			IsolationLevel? transactionIsolationLevel = null)
			where TContext : DbContext
			=> _commandHandlerContext == null
				? throw new InvalidOperationException($"{nameof(_commandHandlerContext)} == null")
				: _commandHandlerContext.CreateNewDbContext<TContext>(transactionUsage, transactionIsolationLevel);

		public TContext GetOrCreateDbContext<TContext>(
			TransactionUsage transactionUsage = TransactionUsage.ReuseOrCreateNew,
			IsolationLevel? transactionIsolationLevel = null) 
			where TContext : DbContext
			=> _commandHandlerContext == null
				? throw new InvalidOperationException($"{nameof(_commandHandlerContext)} == null")
				: _commandHandlerContext.GetOrCreateDbContext<TContext>(transactionUsage, transactionIsolationLevel);
	}
}

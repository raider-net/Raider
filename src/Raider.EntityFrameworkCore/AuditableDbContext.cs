using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Raider.EntityFrameworkCore.Audit;
using Raider.EntityFrameworkCore.Concurrence;
using Raider.Trace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

#nullable disable

namespace Raider.EntityFrameworkCore
{
	public abstract class AuditableDbContext<TAuditEntry> : DbContextBase
			where TAuditEntry : class, IAuditEntry, new()
	{
		public DbSet<TAuditEntry> AuditEntry { get; set; }

		public AuditableDbContext(DbContextOptions options, ILogger logger, IApplicationContext appContext/*, disabledEtitiesFromAudit, disabledEtityPropertiesFromAudit*/)
			: base(options, logger, appContext)
		{
		}

		protected AuditableDbContext(ILogger logger, IApplicationContext appContext)
			: base(logger, appContext)
		{
		}

		public override int SaveChanges(
			bool writeConcurrency = true,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
		{
			return SaveChanges(true, writeConcurrency, memberName, sourceFilePath, sourceLineNumber);
		}

		public override Task<int> SaveChangesAsync(
			bool writeConcurrency = true,
			CancellationToken cancellationToken = default,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
		{
			return SaveChangesAsync(true, writeConcurrency, cancellationToken, memberName, sourceFilePath, sourceLineNumber);
		}

		public override int SaveChanges(
			bool acceptAllChangesOnSuccess,
			bool writeConcurrency = true,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
		{
			using var disposable = CreateDbLogScope(memberName, sourceFilePath, sourceLineNumber);

			var auditCorrelationId = Guid.NewGuid();
			var auditEntriesWithTempProperty = OnBeforeSaveChanges(auditCorrelationId);

			var result = base.SaveChangesWithoutScope(acceptAllChangesOnSuccess, writeConcurrency);

			if (0 < auditEntriesWithTempProperty.Count)
			{
				OnAfterSaveChanges(auditCorrelationId, auditEntriesWithTempProperty);
				var tmpResult = base.SaveChangesWithoutScope(acceptAllChangesOnSuccess, false /*writeConcurrency*/);
				result += tmpResult;
			}

			return result;
		}

		public override async Task<int> SaveChangesAsync(
			bool acceptAllChangesOnSuccess,
			bool writeConcurrency = true,
			CancellationToken cancellationToken = default,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
		{
			using var disposable = CreateDbLogScope(memberName, sourceFilePath, sourceLineNumber);

			var auditCorrelationId = Guid.NewGuid();
			var auditEntriesWithTempProperty = OnBeforeSaveChanges(auditCorrelationId);

			var result = await base.SaveChangesWithoutScopeAsync(acceptAllChangesOnSuccess, writeConcurrency, cancellationToken);

			if (0 < auditEntriesWithTempProperty.Count)
			{
				OnAfterSaveChanges(auditCorrelationId, auditEntriesWithTempProperty);
				var tmpResult = await base.SaveChangesWithoutScopeAsync(acceptAllChangesOnSuccess, false /*writeConcurrency*/, cancellationToken);
				result += tmpResult;
			}

			return result;
		}

		private List<AuditEntryInternal> OnBeforeSaveChanges(Guid auditCorrelationId)
		{
			ChangeTracker.DetectChanges();

			var auditEntries = new List<AuditEntryInternal>();

			var _userId = _applicationContext.TraceInfo.IdUser;
			var now = DateTime.Now;
			foreach (var entry in ChangeTracker.Entries())
			{
				if (entry.Entity is IAuditEntry || entry.State == EntityState.Detached || entry.State == EntityState.Unchanged)
					continue;

				if (entry.Entity is IAuditable auditable)
				{
					switch (entry.State)
					{
						case EntityState.Added:
							auditable.AuditCreatedTime = now;
							auditable.IdAuditCreatedBy = _userId;
							break;

						case EntityState.Modified:
							if (entry.Properties.Any(x => x.IsModified))
							{
								auditable.AuditModifiedTime = now;
								auditable.IdAuditModifiedBy = _userId;
							}
							break;

						default:
							break;
					}
				}

				if (entry.Entity is IConcurrent concurrent)
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

				var auditEntry = new AuditEntryInternal(entry)
				{
					IdUser = _userId,
					Created = now,
					CorrelationId = _applicationContext.TraceInfo.CorrelationId
				};

				auditEntries.Add(auditEntry);
				foreach (var property in entry.Properties)
				{
					if (property.IsTemporary)
					{
						auditEntry.TemporaryProperties.Add(property);
						continue;
					}

					string propertyName = property.Metadata.Name;
					if (property.Metadata.IsPrimaryKey())
					{
						auditEntry.KeyValues[propertyName] = property.CurrentValue;
						continue;
					}

					switch (entry.State)
					{
						case EntityState.Added:
							auditEntry.DbOperation = DbOperation.Create;
							auditEntry.NewValues[propertyName] = property.CurrentValue;
							break;

						case EntityState.Deleted:
							auditEntry.DbOperation = DbOperation.Delete;
							auditEntry.OldValues[propertyName] = property.OriginalValue;
							break;

						case EntityState.Modified:
							if (property.IsModified)
							{
								auditEntry.ChangedColumns.Add(propertyName);
								auditEntry.DbOperation = DbOperation.Update;
								auditEntry.OldValues[propertyName] = property.OriginalValue;
								auditEntry.NewValues[propertyName] = property.CurrentValue;
							}
							break;
					}
				}
			}

			//insert entries without TemporaryProperties
			foreach (var auditEntry in auditEntries.Where(ae => !ae.HasTemporaryProperties))
			{
				AuditEntry.Add(auditEntry.ToAudit<TAuditEntry>(auditCorrelationId));
			}

			return auditEntries.Where(ae => ae.HasTemporaryProperties).ToList();
		}

		private void OnAfterSaveChanges(Guid auditCorrelationId, List<AuditEntryInternal> auditEntriesWithTempProperty)
		{
			foreach (var auditEntry in auditEntriesWithTempProperty)
			{
				foreach (var prop in auditEntry.TemporaryProperties)
				{
					if (prop.Metadata.IsPrimaryKey())
					{
						auditEntry.KeyValues[prop.Metadata.Name] = prop.CurrentValue;
					}
					else
					{
						auditEntry.NewValues[prop.Metadata.Name] = prop.CurrentValue;
					}
				}

				AuditEntry.Add(auditEntry.ToAudit<TAuditEntry>(auditCorrelationId));
			}
		}

		protected override void WriteConcurrency()
		{
			//do nothing
		}
	}
}

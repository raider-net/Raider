using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Raider.EntityFrameworkCore.Audit;
using System;
using System.Collections.Generic;
using System.Linq;
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

		public override int SaveChanges(bool acceptAllChangesOnSuccess)
		{
			using (_logger.BeginScope(new Dictionary<string, object> { { "EF LOGGER SCOPE BY TOM", "VALUE TOM TOM" } })) //TODO
			{
				var auditCorrelationId = Guid.NewGuid();
				var auditEntriesWithTempProperty = OnBeforeSaveChanges(auditCorrelationId);

				var result = base.SaveChanges(acceptAllChangesOnSuccess);

				if (0 < auditEntriesWithTempProperty.Count)
				{
					OnAfterSaveChanges(auditCorrelationId, auditEntriesWithTempProperty);
					var tmpResult = base.SaveChanges(acceptAllChangesOnSuccess);
					result += tmpResult;
				}

				return result;
			}
		}

		public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
		{
			using (_logger.BeginScope(new Dictionary<string, object> { { "EF LOGGER SCOPE BY TOM", "VALUE TOM TOM ASYNC" } })) //TODO
			{
				var auditCorrelationId = Guid.NewGuid();
				var auditEntriesWithTempProperty = OnBeforeSaveChanges(auditCorrelationId);

				var result = await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);

				if (0 < auditEntriesWithTempProperty.Count)
				{
					OnAfterSaveChanges(auditCorrelationId, auditEntriesWithTempProperty);
					var tmpResult = await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
					result += tmpResult;
				}

				return result;
			}
		}

		private List<AuditEntryInternal> OnBeforeSaveChanges(Guid auditCorrelationId)
		{
			ChangeTracker.DetectChanges();

			var auditEntries = new List<AuditEntryInternal>();

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
							auditEntry.AuditType = AuditType.Create;
							auditEntry.NewValues[propertyName] = property.CurrentValue;
							break;

						case EntityState.Deleted:
							auditEntry.AuditType = AuditType.Delete;
							auditEntry.OldValues[propertyName] = property.OriginalValue;
							break;

						case EntityState.Modified:
							if (property.IsModified)
							{
								auditEntry.ChangedColumns.Add(propertyName);
								auditEntry.AuditType = AuditType.Update;
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
	}
}

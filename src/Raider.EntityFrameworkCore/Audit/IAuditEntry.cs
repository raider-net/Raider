using System;

namespace Raider.EntityFrameworkCore.Audit
{
	public interface IAuditEntry
	{
		DateTime Created { get; set; }
		int? IdUser { get; set; }
		int IdAuditType { get; set; }
		string TableName { get; set; }
		string PrimaryKey { get; set; }
		string? OldValues { get; set; }
		string? NewValues { get; set; }
		string? AffectedColumns { get; set; }
		Guid AuditCorrelationId { get; set; }
		string? CommandQueryName { get; set; }
		long? IdCommandQuery { get; set; }
		Guid? CorrelationId { get; set; }
	}
}

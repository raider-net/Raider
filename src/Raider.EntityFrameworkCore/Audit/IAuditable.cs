using System;

namespace Raider.EntityFrameworkCore.Audit
{
	public interface IAuditable
	{
		DateTime AuditCreatedTime { get; set; }
		int? IdAuditCreatedBy { get; set; }
		DateTime? AuditModifiedTime { get; set; }
		int? IdAuditModifiedBy { get; set; }
	}
}

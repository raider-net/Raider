using System;

namespace Raider.EntityFrameworkCore.Correlation
{
	public interface ICorrelable
	{
		Guid CorrelationId { get; set; }
	}
}

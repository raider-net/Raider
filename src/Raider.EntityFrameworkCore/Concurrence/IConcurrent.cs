using System;

namespace Raider.EntityFrameworkCore.Concurrence
{
	public interface IConcurrent
	{
		Guid ConcurrencyToken { get; set; }
	}
}

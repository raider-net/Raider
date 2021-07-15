using System;

namespace Raider.EntityFrameworkCore.Synchronyzation
{
	public interface ISynchronizable
	{
		Guid SyncToken { get; set; }
	}
}

using System;
using System.Collections.Generic;

namespace Raider.EntityFrameworkCore.Synchronyzation
{
	public interface ISynchronizable
	{
		Guid SyncToken { get; set; }

		List<string>? GetIgnoredSynchronizationProperties();
	}
}

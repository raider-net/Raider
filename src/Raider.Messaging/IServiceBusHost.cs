﻿using Raider.Identity;
using System;

namespace Raider.Messaging
{
	public interface IServiceBusHost
	{
		string? ConnectionString { get; }
		Guid IdServiceBusHost { get; }
		Guid IdServiceBusHostRuntime { get; }
		string Name { get; }
		string? Description { get; }
		DateTime StartedUtc { get; }
		int? IdUser { get; }
		RaiderIdentity<int>? User { get; }
		RaiderPrincipal<int>? Principal { get; }
	}
}

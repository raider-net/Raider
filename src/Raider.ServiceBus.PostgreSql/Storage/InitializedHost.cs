using Raider.ServiceBus.Config.Components;
using Raider.ServiceBus.Model;
using System;
using System.Collections.Generic;

namespace Raider.ServiceBus.PostgreSql.Storage
{
	internal class InitializedHost
	{
		public IHost Host { get; set; }
		public IReadOnlyList<IScenario> Scenarios { get; set; }
		public IReadOnlyDictionary<Type, IMessageType> MessageTypes { get; set; }
	}
}

﻿namespace Raider.Messaging
{
	public interface IScenario
	{
		int IdScenario { get; }
		string Name { get; }
		public string? Description { get; }
	}
}

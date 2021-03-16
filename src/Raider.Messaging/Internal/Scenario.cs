using System;

namespace Raider.Messaging
{
	internal struct Scenario : IScenario
	{
		public int IdScenario { get; }
		public string Name { get; }
		public string? Description { get; }

		public Scenario(int idScenario, string name)
		{
			IdScenario = idScenario;
			Name = string.IsNullOrWhiteSpace(name)
				? throw new ArgumentNullException(nameof(name))
				: name;
			Description = null;
		}

		public Scenario(int idScenario, string name, string? description)
		{
			IdScenario = idScenario;
			Name = string.IsNullOrWhiteSpace(name)
				? throw new ArgumentNullException(nameof(name))
				: name;
			Description = description;
		}
	}
}

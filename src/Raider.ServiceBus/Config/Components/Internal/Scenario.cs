using Raider.Converters;
using System;
using System.Collections.Generic;

namespace Raider.ServiceBus.Config.Components.Internal
{
	internal class Scenario : IScenario
	{
		private readonly ScenarioOptions _options;
		private readonly IServiceProvider _serviceProvider;

		public Guid IdScenario { get; }
		public string Name { get; }
		public string? Description { get; set; }
		public bool Disabled { get; set; }
		public DateTime? LastStartTimeUtc { get; set; }
		public List<IComponent> InboundComponents { get; }
		public List<IComponent> BusinessProcesses { get; }
		public List<IComponent> OutboundComponents { get; }

		public Scenario(ScenarioOptions options, IServiceProvider serviceProvider)
		{
			_options = options ?? throw new ArgumentNullException(nameof(options));
			_serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

			IdScenario = GuidConverter.ToGuid(_options.Name);
			Name = _options.Name;
			Description = _options.Description;
			Disabled = _options.Disabled;
			InboundComponents = new List<IComponent>();
			BusinessProcesses = new List<IComponent>();
			OutboundComponents = new List<IComponent>();
		}
	}
}

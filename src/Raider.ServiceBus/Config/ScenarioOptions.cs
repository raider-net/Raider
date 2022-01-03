using Raider.ServiceBus.Config.Fluent;
using Raider.Text;
using Raider.Validation;
using System.Collections.Generic;
using System.Text;

namespace Raider.ServiceBus.Config
{
	public class ScenarioOptions : IValidable
	{
		public string Name { get; set; }
		public string? Description { get; set; }
		public bool Disabled { get; set; }
		public List<InboundComponentBuilder> InboundComponents { get; }
		public List<BusinessProcessBuilder> BusinessProcesses { get; }
		public List<OutboundComponentBuilder> OutboundComponents { get; }

		public ScenarioOptions()
		{
			InboundComponents = new List<InboundComponentBuilder>();
			BusinessProcesses = new List<BusinessProcessBuilder>();
			OutboundComponents = new List<OutboundComponentBuilder>();
		}

		public StringBuilder? Validate(string? propertyPrefix = null, StringBuilder? parentErrorBuffer = null, Dictionary<string, object>? validationContext = null)
		{
			if (string.IsNullOrWhiteSpace(Name))
			{
				if (parentErrorBuffer == null)
					parentErrorBuffer = new StringBuilder();

				parentErrorBuffer.AppendLine($"{StringHelper.ConcatIfNotNullOrEmpty(propertyPrefix, ".", nameof(Name))} == null");
			}

			if (InboundComponents.Count == 0 && BusinessProcesses.Count == 0 && OutboundComponents.Count == 0)
			{
				if (parentErrorBuffer == null)
					parentErrorBuffer = new StringBuilder();

				parentErrorBuffer.AppendLine($"{StringHelper.ConcatIfNotNullOrEmpty(propertyPrefix, ".", "Components")} is empty.");
			}

			return parentErrorBuffer;
		}
	}
}

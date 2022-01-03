using Raider.ServiceBus.Config.Fluent;
using Raider.Text;
using Raider.Validation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Raider.ServiceBus.Config
{
	public class ComponentOptions : IValidable
	{
		public string Name { get; set; }
		public Type CrlType { get; set; }
		public string? Description { get; set; }
		public int ThrottleDelayInMilliseconds { get; set; }
		public int InactivityTimeoutInSeconds { get; set; }
		public int ShutdownTimeoutInSeconds { get; set; }
		public List<ComponentQueueBuilder> ComponentQueues { get; }

		public ComponentOptions()
		{
			ComponentQueues = new List<ComponentQueueBuilder>();
		}

		public StringBuilder? Validate(string? propertyPrefix = null, StringBuilder? parentErrorBuffer = null, Dictionary<string, object>? validationContext = null)
		{
			if (string.IsNullOrWhiteSpace(Name))
			{
				if (parentErrorBuffer == null)
					parentErrorBuffer = new StringBuilder();

				parentErrorBuffer.AppendLine($"{StringHelper.ConcatIfNotNullOrEmpty(propertyPrefix, ".", nameof(Name))} == null");
			}

			if (CrlType == null)
			{
				if (parentErrorBuffer == null)
					parentErrorBuffer = new StringBuilder();

				parentErrorBuffer.AppendLine($"{StringHelper.ConcatIfNotNullOrEmpty(propertyPrefix, ".", nameof(CrlType))} == null");
			}

			if (ComponentQueues.Count == 0)
			{
				if (parentErrorBuffer == null)
					parentErrorBuffer = new StringBuilder();

				parentErrorBuffer.AppendLine($"{StringHelper.ConcatIfNotNullOrEmpty(propertyPrefix, ".", nameof(ComponentQueues))} is empty");
			}

			return parentErrorBuffer;
		}
	}
}

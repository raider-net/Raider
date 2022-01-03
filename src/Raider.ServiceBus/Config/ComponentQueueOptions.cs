using Raider.Text;
using Raider.Validation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Raider.ServiceBus.Config
{
	public class ComponentQueueOptions : IValidable
	{
		public Type MessageType { get; set; }
		public string Name { get; set; }
		//public Type CrlType { get; set; }
		public string? Description { get; set; }
		public bool IsFIFO { get; set; }
		public int ProcessingTimeoutInSeconds { get; set; }
		public int MaxRetryCount { get; set; }

		public StringBuilder? Validate(string? propertyPrefix = null, StringBuilder? parentErrorBuffer = null, Dictionary<string, object>? validationContext = null)
		{
			if (MessageType == null)
			{
				if (parentErrorBuffer == null)
					parentErrorBuffer = new StringBuilder();

				parentErrorBuffer.AppendLine($"{StringHelper.ConcatIfNotNullOrEmpty(propertyPrefix, ".", nameof(MessageType))} == null");
			}

			if (string.IsNullOrWhiteSpace(Name))
			{
				if (parentErrorBuffer == null)
					parentErrorBuffer = new StringBuilder();

				parentErrorBuffer.AppendLine($"{StringHelper.ConcatIfNotNullOrEmpty(propertyPrefix, ".", nameof(Name))} == null");
			}

			//if (CrlType == null)
			//{
			//	if (parentErrorBuffer == null)
			//		parentErrorBuffer = new StringBuilder();

			//	parentErrorBuffer.AppendLine($"{StringHelper.ConcatIfNotNullOrEmpty(propertyPrefix, ".", nameof(CrlType))} == null");
			//}

			return parentErrorBuffer;
		}
	}
}

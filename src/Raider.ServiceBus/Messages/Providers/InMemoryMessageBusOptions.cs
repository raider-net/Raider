using Raider.Serializer;
using Raider.ServiceBus.BusLogger;
using Raider.ServiceBus.Messages.Config;
using Raider.ServiceBus.Resolver;
using Raider.Text;
using System;
using System.Collections.Generic;
using System.Text;

namespace Raider.ServiceBus.Messages.Providers
{
	public class InMemoryMessageBusOptions : IInMemoryMessageBusOptions, IMessageBusOptions
	{
		public string Name { get; set; }
		public Type MessageHandlerContextType { get; set; }
		public Func<IServiceProvider, MessageHandlerContext> MessageHandlerContextFactory { get; set; }
		public ITypeResolver TypeResolver { get; set; }
		public Func<IServiceProvider, ISerializer> MessageSerializer { get; set; }
		public Func<IServiceProvider, IHostLogger> HostLogger { get; set; }
		public Func<IServiceProvider, IHandlerMessageLogger> MessageLogger { get; set; }
		public bool EnableMessageSerialization { get; set; }

		public StringBuilder? Validate(string? propertyPrefix = null, StringBuilder? parentErrorBuffer = null, Dictionary<string, object>? validationContext = null)
		{
			if (string.IsNullOrWhiteSpace(Name))
			{
				if (parentErrorBuffer == null)
					parentErrorBuffer = new StringBuilder();

				parentErrorBuffer.AppendLine($"{StringHelper.ConcatIfNotNullOrEmpty(propertyPrefix, ".", nameof(Name))} == null");
			}

			if (MessageHandlerContextType == null)
			{
				if (parentErrorBuffer == null)
					parentErrorBuffer = new StringBuilder();

				parentErrorBuffer.AppendLine($"{StringHelper.ConcatIfNotNullOrEmpty(propertyPrefix, ".", nameof(MessageHandlerContextType))} == null");
			}

			if (MessageHandlerContextFactory == null)
			{
				if (parentErrorBuffer == null)
					parentErrorBuffer = new StringBuilder();

				parentErrorBuffer.AppendLine($"{StringHelper.ConcatIfNotNullOrEmpty(propertyPrefix, ".", nameof(MessageHandlerContextFactory))} == null");
			}

			if (HostLogger == null)
			{
				if (parentErrorBuffer == null)
					parentErrorBuffer = new StringBuilder();

				parentErrorBuffer.AppendLine($"{StringHelper.ConcatIfNotNullOrEmpty(propertyPrefix, ".", nameof(HostLogger))} == null");
			}

			if (MessageLogger == null)
			{
				if (parentErrorBuffer == null)
					parentErrorBuffer = new StringBuilder();

				parentErrorBuffer.AppendLine($"{StringHelper.ConcatIfNotNullOrEmpty(propertyPrefix, ".", nameof(MessageLogger))} == null");
			}

			return parentErrorBuffer;
		}
	}
}

using Raider.Serializer;
using Raider.ServiceBus.BusLogger;
using Raider.ServiceBus.Events.Config;
using Raider.ServiceBus.Resolver;
using Raider.Text;
using System;
using System.Collections.Generic;
using System.Text;

namespace Raider.ServiceBus.Events.Providers
{
	public class InMemoryEventBusOptions : IInMemoryEventBusOptions, IEventBusOptions
	{
		public string Name { get; set; }
		public Type EventHandlerContextType { get; set; }
		public Func<IServiceProvider, EventHandlerContext> EventHandlerContextFactory { get; set; }
		public ITypeResolver TypeResolver { get; set; }
		public Func<IServiceProvider, ISerializer> MessageSerializer { get; set; }
		public Func<IServiceProvider, IHostLogger> HostLogger { get; set; }
		public Func<IServiceProvider, IHandlerMessageLogger> EventLogger { get; set; }
		public bool EnableMessageSerialization { get; set; }

		public StringBuilder? Validate(string? propertyPrefix = null, StringBuilder? parentErrorBuffer = null, Dictionary<string, object>? validationContext = null)
		{
			if (string.IsNullOrWhiteSpace(Name))
			{
				if (parentErrorBuffer == null)
					parentErrorBuffer = new StringBuilder();

				parentErrorBuffer.AppendLine($"{StringHelper.ConcatIfNotNullOrEmpty(propertyPrefix, ".", nameof(Name))} == null");
			}

			if (EventHandlerContextType == null)
			{
				if (parentErrorBuffer == null)
					parentErrorBuffer = new StringBuilder();

				parentErrorBuffer.AppendLine($"{StringHelper.ConcatIfNotNullOrEmpty(propertyPrefix, ".", nameof(EventHandlerContextType))} == null");
			}

			if (EventHandlerContextFactory == null)
			{
				if (parentErrorBuffer == null)
					parentErrorBuffer = new StringBuilder();

				parentErrorBuffer.AppendLine($"{StringHelper.ConcatIfNotNullOrEmpty(propertyPrefix, ".", nameof(EventHandlerContextFactory))} == null");
			}

			if (HostLogger == null)
			{
				if (parentErrorBuffer == null)
					parentErrorBuffer = new StringBuilder();

				parentErrorBuffer.AppendLine($"{StringHelper.ConcatIfNotNullOrEmpty(propertyPrefix, ".", nameof(HostLogger))} == null");
			}

			if (EventLogger == null)
			{
				if (parentErrorBuffer == null)
					parentErrorBuffer = new StringBuilder();

				parentErrorBuffer.AppendLine($"{StringHelper.ConcatIfNotNullOrEmpty(propertyPrefix, ".", nameof(EventLogger))} == null");
			}

			return parentErrorBuffer;
		}
	}
}

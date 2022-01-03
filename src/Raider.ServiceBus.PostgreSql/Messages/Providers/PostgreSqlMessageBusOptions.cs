using Raider.Serializer;
using Raider.ServiceBus.BusLogger;
using Raider.ServiceBus.Messages;
using Raider.ServiceBus.Messages.Config;
using Raider.ServiceBus.Resolver;
using Raider.Text;
using Raider.Validation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Raider.ServiceBus.PostgreSql.Messages.Providers
{
	public class PostgreSqlMessageBusOptions : IPostgreSqlMessageBusOptions, IMessageBusOptions, IValidable
	{
		public string Name { get; set; }
		public Type MessageHandlerContextType { get; set; }
		public Func<IServiceProvider, MessageHandlerContext> MessageHandlerContextFactory { get; set; }
		public ITypeResolver TypeResolver { get; set; }
		public Func<IServiceProvider, ISerializer> MessageSerializer { get; set; }
		public Func<IServiceProvider, IHostLogger> HostLogger { get; set; }
		public Func<IServiceProvider, IHandlerMessageLogger> MessageLogger { get; set; }
		public bool EnableMessageSerialization { get; set; }
		public string ConnectionString { get; set; }
		public string HostTypeDbSchemaName { get; set; }
		public string HostTypeDbTableName { get; set; }
		public string HostDbSchemaName { get; set; }
		public string HostDbTableName { get; set; }
		public string HostLogDbSchemaName { get; set; }
		public string HostLogDbTableName { get; set; }
		public string MessageTypeDbSchemaName { get; set; }
		public string MessageTypeDbTableName { get; set; }
		public string HandlerMessageDbSchemaName { get; set; }
		public string HandlerMessageDbTableName { get; set; }
		public string MessageBodyDbSchemaName { get; set; }
		public string MessageBodyDbTableName { get; set; }
		public string HandlerMessageLogDbSchemaName { get; set; }
		public string HandlerMessageLogDbTableName { get; set; }

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

			if (TypeResolver == null)
			{
				if (parentErrorBuffer == null)
					parentErrorBuffer = new StringBuilder();

				parentErrorBuffer.AppendLine($"{StringHelper.ConcatIfNotNullOrEmpty(propertyPrefix, ".", nameof(TypeResolver))} == null");
			}

			if (MessageSerializer == null)
			{
				if (parentErrorBuffer == null)
					parentErrorBuffer = new StringBuilder();

				parentErrorBuffer.AppendLine($"{StringHelper.ConcatIfNotNullOrEmpty(propertyPrefix, ".", nameof(MessageSerializer))} == null");
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

			if (string.IsNullOrWhiteSpace(ConnectionString))
			{
				if (parentErrorBuffer == null)
					parentErrorBuffer = new StringBuilder();

				parentErrorBuffer.AppendLine($"{StringHelper.ConcatIfNotNullOrEmpty(propertyPrefix, ".", nameof(ConnectionString))} == null");
			}

			if (string.IsNullOrWhiteSpace(HostTypeDbSchemaName))
			{
				if (parentErrorBuffer == null)
					parentErrorBuffer = new StringBuilder();

				parentErrorBuffer.AppendLine($"{StringHelper.ConcatIfNotNullOrEmpty(propertyPrefix, ".", nameof(HostTypeDbSchemaName))} == null");
			}

			if (string.IsNullOrWhiteSpace(HostTypeDbTableName))
			{
				if (parentErrorBuffer == null)
					parentErrorBuffer = new StringBuilder();

				parentErrorBuffer.AppendLine($"{StringHelper.ConcatIfNotNullOrEmpty(propertyPrefix, ".", nameof(HostTypeDbTableName))} == null");
			}

			if (string.IsNullOrWhiteSpace(HostDbSchemaName))
			{
				if (parentErrorBuffer == null)
					parentErrorBuffer = new StringBuilder();

				parentErrorBuffer.AppendLine($"{StringHelper.ConcatIfNotNullOrEmpty(propertyPrefix, ".", nameof(HostDbSchemaName))} == null");
			}

			if (string.IsNullOrWhiteSpace(HostDbTableName))
			{
				if (parentErrorBuffer == null)
					parentErrorBuffer = new StringBuilder();

				parentErrorBuffer.AppendLine($"{StringHelper.ConcatIfNotNullOrEmpty(propertyPrefix, ".", nameof(HostDbTableName))} == null");
			}

			if (string.IsNullOrWhiteSpace(HostLogDbSchemaName))
			{
				if (parentErrorBuffer == null)
					parentErrorBuffer = new StringBuilder();

				parentErrorBuffer.AppendLine($"{StringHelper.ConcatIfNotNullOrEmpty(propertyPrefix, ".", nameof(HostLogDbSchemaName))} == null");
			}

			if (string.IsNullOrWhiteSpace(HostLogDbTableName))
			{
				if (parentErrorBuffer == null)
					parentErrorBuffer = new StringBuilder();

				parentErrorBuffer.AppendLine($"{StringHelper.ConcatIfNotNullOrEmpty(propertyPrefix, ".", nameof(HostLogDbTableName))} == null");
			}

			if (string.IsNullOrWhiteSpace(MessageTypeDbSchemaName))
			{
				if (parentErrorBuffer == null)
					parentErrorBuffer = new StringBuilder();

				parentErrorBuffer.AppendLine($"{StringHelper.ConcatIfNotNullOrEmpty(propertyPrefix, ".", nameof(MessageTypeDbSchemaName))} == null");
			}

			if (string.IsNullOrWhiteSpace(MessageTypeDbTableName))
			{
				if (parentErrorBuffer == null)
					parentErrorBuffer = new StringBuilder();

				parentErrorBuffer.AppendLine($"{StringHelper.ConcatIfNotNullOrEmpty(propertyPrefix, ".", nameof(MessageTypeDbTableName))} == null");
			}

			if (string.IsNullOrWhiteSpace(HandlerMessageDbSchemaName))
			{
				if (parentErrorBuffer == null)
					parentErrorBuffer = new StringBuilder();

				parentErrorBuffer.AppendLine($"{StringHelper.ConcatIfNotNullOrEmpty(propertyPrefix, ".", nameof(HandlerMessageDbSchemaName))} == null");
			}

			if (string.IsNullOrWhiteSpace(HandlerMessageDbTableName))
			{
				if (parentErrorBuffer == null)
					parentErrorBuffer = new StringBuilder();

				parentErrorBuffer.AppendLine($"{StringHelper.ConcatIfNotNullOrEmpty(propertyPrefix, ".", nameof(HandlerMessageDbTableName))} == null");
			}

			if (string.IsNullOrWhiteSpace(MessageBodyDbSchemaName))
			{
				if (parentErrorBuffer == null)
					parentErrorBuffer = new StringBuilder();

				parentErrorBuffer.AppendLine($"{StringHelper.ConcatIfNotNullOrEmpty(propertyPrefix, ".", nameof(MessageBodyDbSchemaName))} == null");
			}

			if (string.IsNullOrWhiteSpace(MessageBodyDbTableName))
			{
				if (parentErrorBuffer == null)
					parentErrorBuffer = new StringBuilder();

				parentErrorBuffer.AppendLine($"{StringHelper.ConcatIfNotNullOrEmpty(propertyPrefix, ".", nameof(MessageBodyDbTableName))} == null");
			}

			if (string.IsNullOrWhiteSpace(HandlerMessageLogDbSchemaName))
			{
				if (parentErrorBuffer == null)
					parentErrorBuffer = new StringBuilder();

				parentErrorBuffer.AppendLine($"{StringHelper.ConcatIfNotNullOrEmpty(propertyPrefix, ".", nameof(HandlerMessageLogDbSchemaName))} == null");
			}

			if (string.IsNullOrWhiteSpace(HandlerMessageLogDbTableName))
			{
				if (parentErrorBuffer == null)
					parentErrorBuffer = new StringBuilder();

				parentErrorBuffer.AppendLine($"{StringHelper.ConcatIfNotNullOrEmpty(propertyPrefix, ".", nameof(HandlerMessageLogDbTableName))} == null");
			}

			return parentErrorBuffer;
		}
	}
}

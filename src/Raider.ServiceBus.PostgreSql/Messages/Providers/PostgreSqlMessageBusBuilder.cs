using Raider.Exceptions;
using Raider.ServiceBus.Messages;
using Raider.ServiceBus.Messages.Config.Fluent;
using System;

namespace Raider.ServiceBus.PostgreSql.Messages.Providers
{
	public interface IPostgreSqlMessageBusBuilder<TBuilder, TObject> : IMessageBusBuilder<TBuilder, TObject>
		where TBuilder : IPostgreSqlMessageBusBuilder<TBuilder, TObject>
		where TObject : PostgreSqlMessageBusOptions
	{
		TBuilder ConnectionString(string connectionString, bool force = true);

		TBuilder HostTypeDbSchemaName(string hostTypeDbSchemaName, bool force = true);

		TBuilder HostTypeDbTableName(string hostTypeDbTableName, bool force = true);

		TBuilder HostDbSchemaName(string hostDbSchemaName, bool force = true);

		TBuilder HostDbTableName(string hostDbTableName, bool force = true);

		TBuilder HostLogDbSchemaName(string hostLogDbSchemaName, bool force = true);

		TBuilder HostLogDbTableName(string hostLogDbTableName, bool force = true);

		TBuilder MessageTypeDbSchemaName(string messageTypeDbSchemaName, bool force = true);

		TBuilder MessageTypeDbTableName(string messageTypeDbTableName, bool force = true);

		TBuilder HandlerMessageDbSchemaName(string handlerMessageDbSchemaName, bool force = true);

		TBuilder HandlerMessageDbTableName(string handlerMessageDbTableName, bool force = true);

		TBuilder MessageBodyDbSchemaName(string messageBodyDbSchemaName, bool force = true);

		TBuilder MessageBodyDbTableName(string messageBodyDbTableName, bool force = true);

		TBuilder HandlerMessageLogDbSchemaName(string handlerMessageLogDbSchemaName, bool force = true);

		TBuilder HandlerMessageLogDbTableName(string handlerMessageLogDbTableName, bool force = true);
	}

	public abstract class PostgreSqlMessageBusBuilderBase<TBuilder, TObject> : MessageBusBuilderBase<TBuilder, TObject>, IPostgreSqlMessageBusBuilder<TBuilder, TObject>
		where TBuilder : PostgreSqlMessageBusBuilderBase<TBuilder, TObject>
		where TObject : PostgreSqlMessageBusOptions
	{
		protected PostgreSqlMessageBusBuilderBase(TObject options)
			: base(options)
		{
		}

		public virtual TBuilder ConnectionString(string connectionString, bool force = true)
		{
			if (force || string.IsNullOrWhiteSpace(_options.ConnectionString))
				_options.ConnectionString = connectionString;

			return _builder;
		}

		public virtual TBuilder HostTypeDbSchemaName(string hostTypeDbSchemaName, bool force = true)
		{
			if (force || string.IsNullOrWhiteSpace(_options.HostTypeDbSchemaName))
				_options.HostTypeDbSchemaName = hostTypeDbSchemaName;

			return _builder;
		}

		public virtual TBuilder HostTypeDbTableName(string hostTypeDbTableName, bool force = true)
		{
			if (force || string.IsNullOrWhiteSpace(_options.HostTypeDbTableName))
				_options.HostTypeDbTableName = hostTypeDbTableName;

			return _builder;
		}

		public virtual TBuilder HostDbSchemaName(string hostDbSchemaName, bool force = true)
		{
			if (force || string.IsNullOrWhiteSpace(_options.HostDbSchemaName))
				_options.HostDbSchemaName = hostDbSchemaName;

			return _builder;
		}

		public virtual TBuilder HostDbTableName(string hostDbTableName, bool force = true)
		{
			if (force || string.IsNullOrWhiteSpace(_options.HostDbTableName))
				_options.HostDbTableName = hostDbTableName;

			return _builder;
		}

		public virtual TBuilder HostLogDbSchemaName(string hostLogDbSchemaName, bool force = true)
		{
			if (force || string.IsNullOrWhiteSpace(_options.HostLogDbSchemaName))
				_options.HostLogDbSchemaName = hostLogDbSchemaName;

			return _builder;
		}

		public virtual TBuilder HostLogDbTableName(string hostLogDbTableName, bool force = true)
		{
			if (force || string.IsNullOrWhiteSpace(_options.HostLogDbTableName))
				_options.HostLogDbTableName = hostLogDbTableName;

			return _builder;
		}

		public virtual TBuilder MessageTypeDbSchemaName(string messageTypeDbSchemaName, bool force = true)
		{
			if (force || string.IsNullOrWhiteSpace(_options.MessageTypeDbSchemaName))
				_options.MessageTypeDbSchemaName = messageTypeDbSchemaName;

			return _builder;
		}

		public virtual TBuilder MessageTypeDbTableName(string messageTypeDbTableName, bool force = true)
		{
			if (force || string.IsNullOrWhiteSpace(_options.MessageTypeDbTableName))
				_options.MessageTypeDbTableName = messageTypeDbTableName;

			return _builder;
		}

		public virtual TBuilder HandlerMessageDbSchemaName(string handlerMessageDbSchemaName, bool force = true)
		{
			if (force || string.IsNullOrWhiteSpace(_options.HandlerMessageDbSchemaName))
				_options.HandlerMessageDbSchemaName = handlerMessageDbSchemaName;

			return _builder;
		}

		public virtual TBuilder HandlerMessageDbTableName(string HandlerMessageDbTableNameh, bool force = true)
		{
			if (force || string.IsNullOrWhiteSpace(_options.HandlerMessageDbTableName))
				_options.HandlerMessageDbTableName = HandlerMessageDbTableNameh;

			return _builder;
		}

		public virtual TBuilder MessageBodyDbSchemaName(string messageBodyDbSchemaName, bool force = true)
		{
			if (force || string.IsNullOrWhiteSpace(_options.MessageBodyDbSchemaName))
				_options.MessageBodyDbSchemaName = messageBodyDbSchemaName;

			return _builder;
		}

		public virtual TBuilder MessageBodyDbTableName(string messageBodyDbTableName, bool force = true)
		{
			if (force || string.IsNullOrWhiteSpace(_options.MessageBodyDbTableName))
				_options.MessageBodyDbTableName = messageBodyDbTableName;

			return _builder;
		}

		public virtual TBuilder HandlerMessageLogDbSchemaName(string handlerMessageLogDbSchemaName, bool force = true)
		{
			if (force || string.IsNullOrWhiteSpace(_options.HandlerMessageLogDbSchemaName))
				_options.HandlerMessageLogDbSchemaName = handlerMessageLogDbSchemaName;

			return _builder;
		}

		public virtual TBuilder HandlerMessageLogDbTableName(string handlerMessageLogDbTableName, bool force = true)
		{
			if (force || string.IsNullOrWhiteSpace(_options.HandlerMessageLogDbTableName))
				_options.HandlerMessageLogDbTableName = handlerMessageLogDbTableName;

			return _builder;
		}
	}

	public class PostgreSqlMessageBusBuilder : PostgreSqlMessageBusBuilderBase<PostgreSqlMessageBusBuilder, PostgreSqlMessageBusOptions>
	{
		public PostgreSqlMessageBusBuilder()
			: base(new PostgreSqlMessageBusOptions())
		{
		}

		public override IMessageBus Build(IServiceProvider serviceProvider)
		{
			return new PostgreSqlMessageBus(GetOptions(), serviceProvider);
		}

		internal PostgreSqlMessageBusOptions GetOptions()
		{
			var sb = _options.Validate();
			var error = sb?.ToString();
			if (!string.IsNullOrWhiteSpace(error))
				throw new ConfigurationException(error);

			return _options;
		}
	}
}

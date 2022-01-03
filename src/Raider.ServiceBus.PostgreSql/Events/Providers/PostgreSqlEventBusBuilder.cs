using Raider.Exceptions;
using Raider.ServiceBus.Events;
using Raider.ServiceBus.Events.Config.Fluent;
using System;

namespace Raider.ServiceBus.PostgreSql.Events.Providers
{
	public interface IPostgreSqlEventBusBuilder<TBuilder, TObject> : IEventBusBuilder<TBuilder, TObject>
		where TBuilder : IPostgreSqlEventBusBuilder<TBuilder, TObject>
		where TObject : PostgreSqlEventBusOptions
	{
		TBuilder ConnectionString(string connectionString, bool force = true);

		TBuilder HostTypeDbSchemaName(string hostTypeDbSchemaName, bool force = true);

		TBuilder HostTypeDbTableName(string hostTypeDbTableName, bool force = true);

		TBuilder HostDbSchemaName(string hostDbSchemaName, bool force = true);

		TBuilder HostDbTableName(string hostDbTableName, bool force = true);

		TBuilder HostLogDbSchemaName(string hostLogDbSchemaName, bool force = true);

		TBuilder HostLogDbTableName(string hostLogDbTableName, bool force = true);

		TBuilder EventTypeDbSchemaName(string eventTypeDbSchemaName, bool force = true);

		TBuilder EventTypeDbTableName(string eventTypeDbTableName, bool force = true);

		TBuilder EventDbSchemaName(string eventDbSchemaName, bool force = true);

		TBuilder EventDbTableName(string eventDbTableName, bool force = true);

		TBuilder EventBodyDbSchemaName(string eventBodyDbSchemaName, bool force = true);

		TBuilder EventBodyDbTableName(string eventBodyDbTableName, bool force = true);

		TBuilder EventLogDbSchemaName(string eventLogDbSchemaName, bool force = true);

		TBuilder EventLogDbTableName(string eventLogDbTableName, bool force = true);
	}

	public abstract class PostgreSqlEventBusBuilderBase<TBuilder, TObject> : EventBusBuilderBase<TBuilder, TObject>, IPostgreSqlEventBusBuilder<TBuilder, TObject>
		where TBuilder : PostgreSqlEventBusBuilderBase<TBuilder, TObject>
		where TObject : PostgreSqlEventBusOptions
	{
		protected PostgreSqlEventBusBuilderBase(TObject options)
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

		public virtual TBuilder EventTypeDbSchemaName(string eventTypeDbSchemaName, bool force = true)
		{
			if (force || string.IsNullOrWhiteSpace(_options.MessageTypeDbSchemaName))
				_options.MessageTypeDbSchemaName = eventTypeDbSchemaName;

			return _builder;
		}

		public virtual TBuilder EventTypeDbTableName(string eventTypeDbTableName, bool force = true)
		{
			if (force || string.IsNullOrWhiteSpace(_options.MessageTypeDbTableName))
				_options.MessageTypeDbTableName = eventTypeDbTableName;

			return _builder;
		}

		public virtual TBuilder EventDbSchemaName(string eventDbSchemaName, bool force = true)
		{
			if (force || string.IsNullOrWhiteSpace(_options.HandlerMessageDbSchemaName))
				_options.HandlerMessageDbSchemaName = eventDbSchemaName;

			return _builder;
		}

		public virtual TBuilder EventDbTableName(string eventDbTableName, bool force = true)
		{
			if (force || string.IsNullOrWhiteSpace(_options.HandlerMessageDbTableName))
				_options.HandlerMessageDbTableName = eventDbTableName;

			return _builder;
		}

		public virtual TBuilder EventBodyDbSchemaName(string eventBodyDbSchemaName, bool force = true)
		{
			if (force || string.IsNullOrWhiteSpace(_options.MessageBodyDbSchemaName))
				_options.MessageBodyDbSchemaName = eventBodyDbSchemaName;

			return _builder;
		}

		public virtual TBuilder EventBodyDbTableName(string eventBodyDbTableName, bool force = true)
		{
			if (force || string.IsNullOrWhiteSpace(_options.MessageBodyDbTableName))
				_options.MessageBodyDbTableName = eventBodyDbTableName;

			return _builder;
		}

		public virtual TBuilder EventLogDbSchemaName(string eventLogDbSchemaName, bool force = true)
		{
			if (force || string.IsNullOrWhiteSpace(_options.HandlerMessageLogDbSchemaName))
				_options.HandlerMessageLogDbSchemaName = eventLogDbSchemaName;

			return _builder;
		}

		public virtual TBuilder EventLogDbTableName(string eventLogDbTableName, bool force = true)
		{
			if (force || string.IsNullOrWhiteSpace(_options.HandlerMessageLogDbTableName))
				_options.HandlerMessageLogDbTableName = eventLogDbTableName;

			return _builder;
		}
	}

	public class PostgreSqlEventBusBuilder : PostgreSqlEventBusBuilderBase<PostgreSqlEventBusBuilder, PostgreSqlEventBusOptions>
	{
		public PostgreSqlEventBusBuilder()
			: base(new PostgreSqlEventBusOptions())
		{
		}

		public override IEventBus Build(IServiceProvider serviceProvider)
		{
			return new PostgreSqlEventBus(GetOptions(), serviceProvider);
		}

		internal PostgreSqlEventBusOptions GetOptions()
		{
			var sb = _options.Validate();
			var error = sb?.ToString();
			if (!string.IsNullOrWhiteSpace(error))
				throw new ConfigurationException(error);

			return _options;
		}
	}
}

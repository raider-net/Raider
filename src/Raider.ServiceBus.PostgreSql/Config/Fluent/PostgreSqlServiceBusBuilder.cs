using Raider.Exceptions;
using Raider.ServiceBus.Config.Fluent;
using System;

namespace Raider.ServiceBus.PostgreSql.Config.Fluent
{
	public interface IPostgreSqlServiceBusBuilder<TBuilder, TObject> : IServiceBusBuilder<TBuilder, TObject>
		where TBuilder : IPostgreSqlServiceBusBuilder<TBuilder, TObject>
		where TObject : PostgreSqlServiceBusOptions
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

		TBuilder ScenarioDbSchemaName(string scenarioDbSchemaName, bool force = true);

		TBuilder ScenarioDbTableName(string scenarioDbTableName, bool force = true);

		TBuilder ComponentDbSchemaName(string componentDbSchemaName, bool force = true);

		TBuilder ComponentDbTableName(string componentDbTableName, bool force = true);

		TBuilder ComponentLogDbSchemaName(string componentLogDbSchemaName, bool force = true);

		TBuilder ComponentLogDbTableName(string componentLogDbTableName, bool force = true);

		TBuilder ComponentQueueDbSchemaName(string componentQueueDbSchemaName, bool force = true);

		TBuilder ComponentQueueDbTableName(string componentQueueDbTableName, bool force = true);

		TBuilder MessageSessionDbSchemaName(string messageSessionDbSchemaName, bool force = true);

		TBuilder MessageSessionDbTableName(string messageSessionDbTableName, bool force = true);

		TBuilder MessageHeaderDbSchemaName(string messageHeaderDbSchemaName, bool force = true);

		TBuilder MessageHeaderDbTableName(string messageHeaderDbTableName, bool force = true);

		TBuilder MessageLogDbSchemaName(string messageLogDbSchemaName, bool force = true);

		TBuilder MessageLogDbTableName(string messageLogDbTableName, bool force = true);
	}

	public abstract class PostgreSqlServiceBusBuilderBase<TBuilder, TObject> : ServiceBusBuilderBase<TBuilder, TObject>, IPostgreSqlServiceBusBuilder<TBuilder, TObject>
		where TBuilder : PostgreSqlServiceBusBuilderBase<TBuilder, TObject>
		where TObject : PostgreSqlServiceBusOptions
	{
		protected PostgreSqlServiceBusBuilderBase(TObject options)
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

		public virtual TBuilder HandlerMessageDbTableName(string handlerMessageDbTableName, bool force = true)
		{
			if (force || string.IsNullOrWhiteSpace(_options.HandlerMessageDbTableName))
				_options.HandlerMessageDbTableName = handlerMessageDbTableName;

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

		public virtual TBuilder HandlerMessageLogDbTableName(string handlerMessageLogDbTableNameh, bool force = true)
		{
			if (force || string.IsNullOrWhiteSpace(_options.HandlerMessageLogDbTableName))
				_options.HandlerMessageLogDbTableName = handlerMessageLogDbTableNameh;

			return _builder;
		}

		public virtual TBuilder ScenarioDbSchemaName(string scenarioDbSchemaName, bool force = true)
		{
			if (force || string.IsNullOrWhiteSpace(_options.ScenarioDbSchemaName))
				_options.ScenarioDbSchemaName = scenarioDbSchemaName;

			return _builder;
		}

		public virtual TBuilder ScenarioDbTableName(string scenarioDbTableName, bool force = true)
		{
			if (force || string.IsNullOrWhiteSpace(_options.ScenarioDbTableName))
				_options.ScenarioDbTableName = scenarioDbTableName;

			return _builder;
		}

		public virtual TBuilder ComponentDbSchemaName(string componentDbSchemaName, bool force = true)
		{
			if (force || string.IsNullOrWhiteSpace(_options.ComponentDbSchemaName))
				_options.ComponentDbSchemaName = componentDbSchemaName;

			return _builder;
		}

		public virtual TBuilder ComponentDbTableName(string componentDbTableName, bool force = true)
		{
			if (force || string.IsNullOrWhiteSpace(_options.ComponentDbTableName))
				_options.ComponentDbTableName = componentDbTableName;

			return _builder;
		}

		public virtual TBuilder ComponentLogDbSchemaName(string componentLogDbSchemaName, bool force = true)
		{
			if (force || string.IsNullOrWhiteSpace(_options.ComponentLogDbSchemaName))
				_options.ComponentLogDbSchemaName = componentLogDbSchemaName;

			return _builder;
		}

		public virtual TBuilder ComponentLogDbTableName(string componentLogDbTableName, bool force = true)
		{
			if (force || string.IsNullOrWhiteSpace(_options.ComponentLogDbTableName))
				_options.ComponentLogDbTableName = componentLogDbTableName;

			return _builder;
		}

		public virtual TBuilder ComponentQueueDbSchemaName(string componentQueueDbSchemaName, bool force = true)
		{
			if (force || string.IsNullOrWhiteSpace(_options.ComponentQueueDbSchemaName))
				_options.ComponentQueueDbSchemaName = componentQueueDbSchemaName;

			return _builder;
		}

		public virtual TBuilder ComponentQueueDbTableName(string componentQueueDbTableName, bool force = true)
		{
			if (force || string.IsNullOrWhiteSpace(_options.ComponentQueueDbTableName))
				_options.ComponentQueueDbTableName = componentQueueDbTableName;

			return _builder;
		}

		public virtual TBuilder MessageSessionDbSchemaName(string messageSessionDbSchemaName, bool force = true)
		{
			if (force || string.IsNullOrWhiteSpace(_options.MessageSessionDbSchemaName))
				_options.MessageSessionDbSchemaName = messageSessionDbSchemaName;

			return _builder;
		}

		public virtual TBuilder MessageSessionDbTableName(string messageSessionDbTableName, bool force = true)
		{
			if (force || string.IsNullOrWhiteSpace(_options.MessageSessionDbTableName))
				_options.MessageSessionDbTableName = messageSessionDbTableName;

			return _builder;
		}

		public virtual TBuilder MessageHeaderDbSchemaName(string messageHeaderDbSchemaName, bool force = true)
		{
			if (force || string.IsNullOrWhiteSpace(_options.MessageHeaderDbSchemaName))
				_options.MessageHeaderDbSchemaName = messageHeaderDbSchemaName;

			return _builder;
		}

		public virtual TBuilder MessageHeaderDbTableName(string messageHeaderDbTableName, bool force = true)
		{
			if (force || string.IsNullOrWhiteSpace(_options.MessageHeaderDbTableName))
				_options.MessageHeaderDbTableName = messageHeaderDbTableName;

			return _builder;
		}

		public virtual TBuilder MessageLogDbSchemaName(string messageLogDbSchemaName, bool force = true)
		{
			if (force || string.IsNullOrWhiteSpace(_options.MessageLogDbSchemaName))
				_options.MessageLogDbSchemaName = messageLogDbSchemaName;

			return _builder;
		}

		public virtual TBuilder MessageLogDbTableName(string messageLogDbTableName, bool force = true)
		{
			if (force || string.IsNullOrWhiteSpace(_options.MessageLogDbTableName))
				_options.MessageLogDbTableName = messageLogDbTableName;

			return _builder;
		}
	}

	public class PostgreSqlServiceBusBuilder : PostgreSqlServiceBusBuilderBase<PostgreSqlServiceBusBuilder, PostgreSqlServiceBusOptions>
	{
		public PostgreSqlServiceBusBuilder()
			: base(new PostgreSqlServiceBusOptions())
		{
		}

		public override IServiceBus Build(IServiceProvider serviceProvider)
			=> new PostgreSqlServiceBus(GetOptions(), serviceProvider);

		public override void Validate()
		{
			var sb = _options.Validate();
			var error = sb?.ToString();
			if (!string.IsNullOrWhiteSpace(error))
				throw new ConfigurationException(error);
		}

		internal PostgreSqlServiceBusOptions GetOptions()
		{
			Validate();
			return _options;
		}
	}
}

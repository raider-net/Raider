using Raider.Serializer;
using Raider.ServiceBus.BusLogger;
using Raider.ServiceBus.Resolver;
using System;

namespace Raider.ServiceBus.Config.Fluent
{
	public interface IServiceBusBuilder<TBuilder, TObject>
		where TBuilder : IServiceBusBuilder<TBuilder, TObject>
		where TObject : IServiceBusOptions
	{
		IServiceBus Build(IServiceProvider serviceProvider);

		TBuilder Name(string name, bool force = true);
		TBuilder TypeResolver(ITypeResolver typeResolver, bool force = true);
		TBuilder MessageSerializer(Func<IServiceProvider, ISerializer> serializer, bool force = true);
		TBuilder HostLogger(Func<IServiceProvider, IHostLogger> logger, bool force = true);
		TBuilder EnableMessageSerialization(bool enable, bool force = true);
		TBuilder AddScenario(Action<ScenarioBuilder> scenarioBuilder);
	}

	public abstract class ServiceBusBuilderBase<TBuilder, TObject> : IServiceBusBuilder<TBuilder, TObject>
		where TBuilder : ServiceBusBuilderBase<TBuilder, TObject>
		where TObject : IServiceBusOptions
	{
		protected readonly TBuilder _builder;
		protected TObject _options;

		protected ServiceBusBuilderBase(TObject options)
		{
			_options = options;
			_builder = (TBuilder)this;
		}

		public abstract IServiceBus Build(IServiceProvider serviceProvider);

		public abstract void Validate();

		public virtual TBuilder Name(string name, bool force = true)
		{
			if (force || string.IsNullOrWhiteSpace(_options.Name))
				_options.Name = name;

			return _builder;
		}

		public virtual TBuilder TypeResolver(ITypeResolver typeResolver, bool force = true)
		{
			if (force || _options.TypeResolver == null)
				_options.TypeResolver = typeResolver;

			return _builder;
		}

		public virtual TBuilder MessageSerializer(Func<IServiceProvider, ISerializer> serializer, bool force = true)
		{
			if (force || _options.MessageSerializer == null)
				_options.MessageSerializer = serializer;

			return _builder;
		}

		public virtual TBuilder HostLogger(Func<IServiceProvider, IHostLogger> logger, bool force = true)
		{
			if (force || _options.HostLogger == null)
				_options.HostLogger = logger;

			return _builder;
		}

		public virtual TBuilder EnableMessageSerialization(bool enable, bool force = true)
		{
			_options.EnableMessageSerialization = enable;
			return _builder;
		}

		public virtual TBuilder AddScenario(Action<ScenarioBuilder> scenarioBuilder)
		{
			var cBuilder = new ScenarioBuilder();
			scenarioBuilder?.Invoke(cBuilder);
			cBuilder.Validate();
			_options.Scenarios.Add(cBuilder);

			return _builder;
		}
	}
}

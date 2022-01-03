using Raider.Serializer;
using Raider.ServiceBus.BusLogger;
using Raider.ServiceBus.Resolver;
using System;

namespace Raider.ServiceBus.Events.Config.Fluent
{
	public interface IEventBusBuilder<TBuilder, TObject>
		where TBuilder : IEventBusBuilder<TBuilder, TObject>
		where TObject : IEventBusOptions
	{
		IEventBus Build(IServiceProvider serviceProvider);

		TBuilder Name(string name, bool force = true);
		TBuilder EventHandlerContextType(Type eventHandlerContextType, bool force = true);
		TBuilder EventHandlerContextFactory(Func<IServiceProvider, EventHandlerContext> serializer, bool force = true);
		TBuilder TypeResolver(ITypeResolver typeResolver, bool force = true);
		TBuilder EventSerializer(Func<IServiceProvider, ISerializer> serializer, bool force = true);
		TBuilder HostLogger(Func<IServiceProvider, IHostLogger> logger, bool force = true);
		TBuilder EventLogger(Func<IServiceProvider, IHandlerMessageLogger> logger, bool force = true);
		TBuilder EnableMessageSerialization(bool enable, bool force = true);
	}

	public abstract class EventBusBuilderBase<TBuilder, TObject> : IEventBusBuilder<TBuilder, TObject>
		where TBuilder : EventBusBuilderBase<TBuilder, TObject>
		where TObject : IEventBusOptions
	{
		protected readonly TBuilder _builder;
		protected TObject _options;

		protected EventBusBuilderBase(TObject options)
		{
			_options = options;
			_builder = (TBuilder)this;
		}

		public abstract IEventBus Build(IServiceProvider serviceProvider);

		public virtual TBuilder Name(string name, bool force = true)
		{
			if (force || string.IsNullOrWhiteSpace(_options.Name))
				_options.Name = name;

			return _builder;
		}

		public virtual TBuilder EventHandlerContextType(Type eventHandlerContextType, bool force = true)
		{
			if (force || _options.EventHandlerContextType == null)
				_options.EventHandlerContextType = eventHandlerContextType;

			return _builder;
		}

		public virtual TBuilder EventHandlerContextFactory(Func<IServiceProvider, EventHandlerContext> factory, bool force = true)
		{
			if (force || _options.MessageSerializer == null)
				_options.EventHandlerContextFactory = factory;

			return _builder;
		}

		public virtual TBuilder TypeResolver(ITypeResolver typeResolver, bool force = true)
		{
			if (force || _options.TypeResolver == null)
				_options.TypeResolver = typeResolver;

			return _builder;
		}

		public virtual TBuilder EventSerializer(Func<IServiceProvider, ISerializer> serializer, bool force = true)
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

		public virtual TBuilder EventLogger(Func<IServiceProvider, IHandlerMessageLogger> logger, bool force = true)
		{
			if (force || _options.EventLogger == null)
				_options.EventLogger = logger;

			return _builder;
		}

		public virtual TBuilder EnableMessageSerialization(bool enable, bool force = true)
		{
			_options.EnableMessageSerialization = enable;
			return _builder;
		}
	}
}

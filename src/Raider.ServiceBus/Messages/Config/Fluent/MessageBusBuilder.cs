using Raider.Serializer;
using Raider.ServiceBus.BusLogger;
using Raider.ServiceBus.Resolver;
using System;

namespace Raider.ServiceBus.Messages.Config.Fluent
{
	public interface IMessageBusBuilder<TBuilder, TObject>
		where TBuilder : IMessageBusBuilder<TBuilder, TObject>
		where TObject : IMessageBusOptions
	{
		IMessageBus Build(IServiceProvider serviceProvider);

		TBuilder Name(string name, bool force = true);
		TBuilder MessageHandlerContextType(Type messageHandlerContextType, bool force = true);
		TBuilder MessageHandlerContextFactory(Func<IServiceProvider, MessageHandlerContext> serializer, bool force = true);
		TBuilder TypeResolver(ITypeResolver typeResolver, bool force = true);
		TBuilder MessageSerializer(Func<IServiceProvider, ISerializer> serializer, bool force = true);
		TBuilder HostLogger(Func<IServiceProvider, IHostLogger> logger, bool force = true);
		TBuilder MessageLogger(Func<IServiceProvider, IHandlerMessageLogger> logger, bool force = true);
		TBuilder EnableMessageSerialization(bool enable, bool force = true);
	}

	public abstract class MessageBusBuilderBase<TBuilder, TObject> : IMessageBusBuilder<TBuilder, TObject>
		where TBuilder : MessageBusBuilderBase<TBuilder, TObject>
		where TObject : IMessageBusOptions
	{
		protected readonly TBuilder _builder;
		protected TObject _options;

		protected MessageBusBuilderBase(TObject options)
		{
			_options = options;
			_builder = (TBuilder)this;
		}

		public abstract IMessageBus Build(IServiceProvider serviceProvider);

		public virtual TBuilder Name(string name, bool force = true)
		{
			if (force || string.IsNullOrWhiteSpace(_options.Name))
				_options.Name = name;

			return _builder;
		}

		public virtual TBuilder MessageHandlerContextType(Type messageHandlerContextType, bool force = true)
		{
			if (force || _options.MessageHandlerContextType == null)
				_options.MessageHandlerContextType = messageHandlerContextType;

			return _builder;
		}

		public virtual TBuilder MessageHandlerContextFactory(Func<IServiceProvider, MessageHandlerContext> factory, bool force = true)
		{
			if (force || _options.MessageSerializer == null)
				_options.MessageHandlerContextFactory = factory;

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

		public virtual TBuilder MessageLogger(Func<IServiceProvider, IHandlerMessageLogger> logger, bool force = true)
		{
			if (force || _options.MessageLogger == null)
				_options.MessageLogger = logger;

			return _builder;
		}

		public virtual TBuilder EnableMessageSerialization(bool enable, bool force = true)
		{
			_options.EnableMessageSerialization = enable;
			return _builder;
		}
	}
}

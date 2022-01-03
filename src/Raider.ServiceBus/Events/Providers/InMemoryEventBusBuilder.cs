using Raider.Exceptions;
using Raider.ServiceBus.Events.Config.Fluent;
using System;

namespace Raider.ServiceBus.Events.Providers
{
	public interface IInMemoryMessageBusBuilder<TBuilder, TObject> : IEventBusBuilder<TBuilder, TObject>
		where TBuilder : IInMemoryMessageBusBuilder<TBuilder, TObject>
		where TObject : InMemoryEventBusOptions
	{
	}

	public abstract class InMemoryMessageBusBuilderBase<TBuilder, TObject> : EventBusBuilderBase<TBuilder, TObject>, IInMemoryMessageBusBuilder<TBuilder, TObject>
		where TBuilder : InMemoryMessageBusBuilderBase<TBuilder, TObject>
		where TObject : InMemoryEventBusOptions
	{
		protected InMemoryMessageBusBuilderBase(TObject options)
			: base(options)
		{
		}
	}

	public class InMemoryEventBusBuilder : InMemoryMessageBusBuilderBase<InMemoryEventBusBuilder, InMemoryEventBusOptions>
	{
		public InMemoryEventBusBuilder()
			: base(new InMemoryEventBusOptions())
		{
		}

		public override IEventBus Build(IServiceProvider serviceProvider)
		{
			return new InMemoryEventBus(GetOptions(), serviceProvider);
		}

		internal InMemoryEventBusOptions GetOptions()
		{
			var sb = _options.Validate();
			var error = sb?.ToString();
			if (!string.IsNullOrWhiteSpace(error))
				throw new ConfigurationException(error);

			return _options;
		}
	}
}

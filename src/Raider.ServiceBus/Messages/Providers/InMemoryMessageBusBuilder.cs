using Raider.Exceptions;
using Raider.ServiceBus.Messages.Config.Fluent;
using System;

namespace Raider.ServiceBus.Messages.Providers
{
	public interface IInMemoryMessageBusBuilder<TBuilder, TObject> : IMessageBusBuilder<TBuilder, TObject>
		where TBuilder : IInMemoryMessageBusBuilder<TBuilder, TObject>
		where TObject : InMemoryMessageBusOptions
	{
	}

	public abstract class InMemoryMessageBusBuilderBase<TBuilder, TObject> : MessageBusBuilderBase<TBuilder, TObject>, IInMemoryMessageBusBuilder<TBuilder, TObject>
		where TBuilder : InMemoryMessageBusBuilderBase<TBuilder, TObject>
		where TObject : InMemoryMessageBusOptions
	{
		protected InMemoryMessageBusBuilderBase(TObject options)
			: base(options)
		{
		}
	}

	public class InMemoryMessageBusBuilder : InMemoryMessageBusBuilderBase<InMemoryMessageBusBuilder, InMemoryMessageBusOptions>
	{
		public InMemoryMessageBusBuilder()
			: base(new InMemoryMessageBusOptions())
		{
		}

		public override IMessageBus Build(IServiceProvider serviceProvider)
		{
			return new InMemoryMessageBus(GetOptions(), serviceProvider);
		}

		internal InMemoryMessageBusOptions GetOptions()
		{
			var sb = _options.Validate();
			var error = sb?.ToString();
			if (!string.IsNullOrWhiteSpace(error))
				throw new ConfigurationException(error);

			return _options;
		}
	}
}

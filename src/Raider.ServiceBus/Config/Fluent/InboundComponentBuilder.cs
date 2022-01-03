using Raider.Exceptions;
using Raider.ServiceBus.Components;
using Raider.ServiceBus.Config.Components;
using Raider.ServiceBus.Config.Components.Internal;
using Raider.ServiceBus.Resolver;
using System;

namespace Raider.ServiceBus.Config.Fluent
{
	public interface IInboundComponentBuilder<TBuilder, TObject> : IComponentBuilder<TBuilder, TObject>
		where TBuilder : IInboundComponentBuilder<TBuilder, TObject>
		where TObject : ComponentOptions
	{		
		TBuilder CrlType<TInboundComponent>(bool force = true)
			where TInboundComponent : IInboundComponent;
	}

	public abstract class InboundComponentBuilderBase<TBuilder, TObject> : ComponentBuilderBase<TBuilder, TObject>, IInboundComponentBuilder<TBuilder, TObject>
		where TBuilder : InboundComponentBuilderBase<TBuilder, TObject>
		where TObject : ComponentOptions
	{
		protected InboundComponentBuilderBase(TObject options)
			: base(options)
		{
		}

		public virtual TBuilder CrlType<TInboundComponent>(bool force = true)
			where TInboundComponent : IInboundComponent
		{
			if (force || _options.CrlType == null)
				_options.CrlType = typeof(TInboundComponent);

			return _builder;
		}
	}

	public class InboundComponentBuilder : InboundComponentBuilderBase<InboundComponentBuilder, ComponentOptions>
	{
		public InboundComponentBuilder()
			: base(new ComponentOptions())
		{
		}

		public override IComponent Build(ITypeResolver typeResolver, IScenario scenario, IServiceProvider serviceProvider)
		{
			var component = new Component(typeResolver, scenario, GetOptions(), serviceProvider);

			foreach (var componentQueueBuilder in _options.ComponentQueues)
			{
				var componentQueue = componentQueueBuilder.Build(component, serviceProvider);
				component.ComponentQueues.Add(componentQueue);
			}

			return component;
		}

		public override void Validate()
		{
			var sb = _options.Validate();
			var error = sb?.ToString();
			if (!string.IsNullOrWhiteSpace(error))
				throw new ConfigurationException(error);
		}

		internal ComponentOptions GetOptions()
		{
			Validate();
			return _options;
		}
	}
}

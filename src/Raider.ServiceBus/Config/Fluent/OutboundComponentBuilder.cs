using Raider.Exceptions;
using Raider.ServiceBus.Components;
using Raider.ServiceBus.Config.Components;
using Raider.ServiceBus.Config.Components.Internal;
using Raider.ServiceBus.Resolver;
using System;

namespace Raider.ServiceBus.Config.Fluent
{
	public interface IOutboundComponentBuilder<TBuilder, TObject> : IComponentBuilder<TBuilder, TObject>
		where TBuilder : IOutboundComponentBuilder<TBuilder, TObject>
		where TObject : ComponentOptions
	{		
		TBuilder CrlType<TOutboundComponent>(bool force = true)
			where TOutboundComponent : IOutboundComponent;
	}

	public abstract class OutboundComponentBuilderBase<TBuilder, TObject> : ComponentBuilderBase<TBuilder, TObject>, IOutboundComponentBuilder<TBuilder, TObject>
		where TBuilder : OutboundComponentBuilderBase<TBuilder, TObject>
		where TObject : ComponentOptions
	{
		protected OutboundComponentBuilderBase(TObject options)
			: base(options)
		{
		}

		public virtual TBuilder CrlType<TOutboundComponent>(bool force = true)
			where TOutboundComponent : IOutboundComponent
		{
			if (force || _options.CrlType == null)
				_options.CrlType = typeof(TOutboundComponent);

			return _builder;
		}
	}

	public class OutboundComponentBuilder : OutboundComponentBuilderBase<OutboundComponentBuilder, ComponentOptions>
	{
		public OutboundComponentBuilder()
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

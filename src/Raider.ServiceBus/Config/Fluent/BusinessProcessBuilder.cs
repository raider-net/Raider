using Raider.Exceptions;
using Raider.ServiceBus.Components;
using Raider.ServiceBus.Config.Components;
using Raider.ServiceBus.Config.Components.Internal;
using Raider.ServiceBus.Resolver;
using System;

namespace Raider.ServiceBus.Config.Fluent
{
	public interface IBusinessProcessBuilder<TBuilder, TObject> : IComponentBuilder<TBuilder, TObject>
		where TBuilder : IBusinessProcessBuilder<TBuilder, TObject>
		where TObject : ComponentOptions
	{		
		TBuilder CrlType<TBusinessProcess>(bool force = true)
			where TBusinessProcess : IBaseBusinessProcess;
	}

	public abstract class BusinessProcessBuilderBase<TBuilder, TObject> : ComponentBuilderBase<TBuilder, TObject>, IBusinessProcessBuilder<TBuilder, TObject>
		where TBuilder : BusinessProcessBuilderBase<TBuilder, TObject>
		where TObject : ComponentOptions
	{
		protected BusinessProcessBuilderBase(TObject options)
			: base(options)
		{
		}

		public virtual TBuilder CrlType<TBusinessProcess>(bool force = true)
			where TBusinessProcess : IBaseBusinessProcess
		{
			if (force || _options.CrlType == null)
				_options.CrlType = typeof(TBusinessProcess);

			return _builder;
		}
	}

	public class BusinessProcessBuilder : BusinessProcessBuilderBase<BusinessProcessBuilder, ComponentOptions>
	{
		public BusinessProcessBuilder()
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

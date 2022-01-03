using Raider.ServiceBus.Config.Components;
using Raider.ServiceBus.Resolver;
using System;

namespace Raider.ServiceBus.Config.Fluent
{
	public interface IComponentBuilder<TBuilder, TObject>
		where TBuilder : IComponentBuilder<TBuilder, TObject>
		where TObject : ComponentOptions
	{
		IComponent Build(ITypeResolver typeResolver, IScenario scenario, IServiceProvider serviceProvider);

		TBuilder Name(string name, bool force = true);

		TBuilder Description(string description, bool force = true);
		TBuilder ThrottleDelayInMilliseconds(int throttleDelayInMilliseconds);
		TBuilder InactivityTimeoutInSeconds(int inactivityTimeoutInSeconds);
		TBuilder ShutdownTimeoutInSeconds(int shutdownTimeoutInSeconds);
		TBuilder AddQueue(Action<ComponentQueueBuilder> componentBuilder);
	}

	public abstract class ComponentBuilderBase<TBuilder, TObject> : IComponentBuilder<TBuilder, TObject>
		where TBuilder : ComponentBuilderBase<TBuilder, TObject>
		where TObject : ComponentOptions
	{
		protected readonly TBuilder _builder;
		protected TObject _options;

		protected ComponentBuilderBase(TObject options)
		{
			_options = options;
			_builder = (TBuilder)this;
		}

		public abstract IComponent Build(ITypeResolver typeResolver, IScenario scenario, IServiceProvider serviceProvider);

		public abstract void Validate();

		public virtual TBuilder Name(string name, bool force = true)
		{
			if (force || string.IsNullOrWhiteSpace(_options.Name))
				_options.Name = name;

			return _builder;
		}

		public virtual TBuilder Description(string description, bool force = true)
		{
			if (force || string.IsNullOrWhiteSpace(_options.Description))
				_options.Description = description;

			return _builder;
		}

		public virtual TBuilder ThrottleDelayInMilliseconds(int throttleDelayInMilliseconds)
		{
			_options.ThrottleDelayInMilliseconds = throttleDelayInMilliseconds;
			return _builder;
		}

		public virtual TBuilder InactivityTimeoutInSeconds(int inactivityTimeoutInSeconds)
		{
			_options.InactivityTimeoutInSeconds = inactivityTimeoutInSeconds;
			return _builder;
		}

		public virtual TBuilder ShutdownTimeoutInSeconds(int shutdownTimeoutInSeconds)
		{
			_options.ShutdownTimeoutInSeconds = shutdownTimeoutInSeconds;
			return _builder;
		}

		public virtual TBuilder AddQueue(Action<ComponentQueueBuilder> componentQueueBuilder)
		{
			var cqBuilder = new ComponentQueueBuilder();
			componentQueueBuilder?.Invoke(cqBuilder);
			cqBuilder.Validate();
			_options.ComponentQueues.Add(cqBuilder);

			return _builder;
		}
	}
}

using Raider.Converters;
using Raider.Exceptions;
using Raider.ServiceBus.Config.Components;
using Raider.ServiceBus.Config.Components.Internal;
using Raider.ServiceBus.Internal.Model;
using Raider.ServiceBus.Resolver;
using System;
using System.Collections.Generic;

namespace Raider.ServiceBus.Config.Fluent
{
	public interface IScenarioBuilder<TBuilder, TObject>
		where TBuilder : IScenarioBuilder<TBuilder, TObject>
		where TObject : ScenarioOptions
	{
		IScenario Build(ITypeResolver typeResolver, IServiceProvider serviceProvider);

		TBuilder Name(string name, bool force = true);
		TBuilder Description(string description, bool force = true);
		TBuilder Disabled(bool disabled);
		TBuilder AddInboundComponent(Action<InboundComponentBuilder> componentBuilder);
		TBuilder AddBusinessProcess(Action<BusinessProcessBuilder> componentBuilder);
		TBuilder AddOutboundComponent(Action<OutboundComponentBuilder> componentBuilder);
	}

	public abstract class ScenarioBuilderBase<TBuilder, TObject> : IScenarioBuilder<TBuilder, TObject>
		where TBuilder : ScenarioBuilderBase<TBuilder, TObject>
		where TObject : ScenarioOptions
	{
		protected readonly TBuilder _builder;
		protected TObject _options;

		protected ScenarioBuilderBase(TObject options)
		{
			_options = options;
			_builder = (TBuilder)this;
		}

		public abstract IScenario Build(ITypeResolver typeResolver, IServiceProvider serviceProvider);

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

		public virtual TBuilder Disabled(bool disabled)
		{
			_options.Disabled = disabled;
			return _builder;
		}

		public virtual TBuilder AddInboundComponent(Action<InboundComponentBuilder> componentBuilder)
		{
			var cBuilder = new InboundComponentBuilder();
			componentBuilder?.Invoke(cBuilder);
			cBuilder.Validate();
			_options.InboundComponents.Add(cBuilder);

			return _builder;
		}

		public virtual TBuilder AddBusinessProcess(Action<BusinessProcessBuilder> componentBuilder)
		{
			var cBuilder = new BusinessProcessBuilder();
			componentBuilder?.Invoke(cBuilder);
			cBuilder.Validate();
			_options.BusinessProcesses.Add(cBuilder);

			return _builder;
		}

		public virtual TBuilder AddOutboundComponent(Action<OutboundComponentBuilder> componentBuilder)
		{
			var cBuilder = new OutboundComponentBuilder();
			componentBuilder?.Invoke(cBuilder);
			cBuilder.Validate();
			_options.OutboundComponents.Add(cBuilder);

			return _builder;
		}
	}

	public class ScenarioBuilder : ScenarioBuilderBase<ScenarioBuilder, ScenarioOptions>
	{
		public ScenarioBuilder()
			: base(new ScenarioOptions())
		{
		}

		public override IScenario Build(ITypeResolver typeResolver, IServiceProvider serviceProvider)
		{
			var scenario = new Scenario(GetOptions(), serviceProvider);
			var messageTypes = new Dictionary<Type, DbMessageType>();

			foreach (var inboundComponentBuilder in _options.InboundComponents)
			{
				var inboundComponent = inboundComponentBuilder.Build(typeResolver, scenario, serviceProvider);
				scenario.InboundComponents.Add(inboundComponent);

				foreach (var componentQueue in inboundComponent.ComponentQueues)
				{
					if (!messageTypes.TryGetValue(componentQueue.MessageType, out var iMessageType))
					{
						iMessageType = ComponentQueueMessageTypeToDB(typeResolver, componentQueue.MessageType, scenario, inboundComponent);
						messageTypes.Add(componentQueue.MessageType, iMessageType);
					}
					((ComponentQueue)componentQueue).MessageTypeModel = iMessageType;
				}
			}

			foreach (var businessProcessBuilder in _options.BusinessProcesses)
			{
				var businessProcess = businessProcessBuilder.Build(typeResolver, scenario, serviceProvider);
				scenario.BusinessProcesses.Add(businessProcess);

				foreach (var componentQueue in businessProcess.ComponentQueues)
				{
					if (!messageTypes.TryGetValue(componentQueue.MessageType, out var iMessageType))
					{
						iMessageType = ComponentQueueMessageTypeToDB(typeResolver, componentQueue.MessageType, scenario, businessProcess);
						messageTypes.Add(componentQueue.MessageType, iMessageType);
					}
					((ComponentQueue)componentQueue).MessageTypeModel = iMessageType;
				}
			}

			foreach (var outboundComponentBuilder in _options.OutboundComponents)
			{
				var outboundComponent = outboundComponentBuilder.Build(typeResolver, scenario, serviceProvider);
				scenario.OutboundComponents.Add(outboundComponent);

				foreach (var componentQueue in outboundComponent.ComponentQueues)
				{
					if (!messageTypes.TryGetValue(componentQueue.MessageType, out var iMessageType))
					{
						iMessageType = ComponentQueueMessageTypeToDB(typeResolver, componentQueue.MessageType, scenario, outboundComponent);
						messageTypes.Add(componentQueue.MessageType, iMessageType);
					}
					((ComponentQueue)componentQueue).MessageTypeModel = iMessageType;
				}
			}

			return scenario;
		}

		private DbMessageType ComponentQueueMessageTypeToDB(ITypeResolver typeResolver, Type type, IScenario scenario, IComponent component)
		{
			if (typeResolver == null)
				throw new ArgumentNullException(nameof(typeResolver));

			var resolvedTypeString = typeResolver.ToName(type);
			if (string.IsNullOrWhiteSpace(resolvedTypeString))
				throw new InvalidOperationException($"Message type {type} {nameof(resolvedTypeString)} == NULL | {nameof(scenario)} = {scenario.Name} | {nameof(component)} = {component.CrlType.FullName}");

			return
				new DbMessageType
				(
					GuidConverter.ToGuid(resolvedTypeString),
					type.FullName ?? type.Name,
					(int)MessageMetaType.ServiceBusMessage,
					resolvedTypeString
				);
		}

		public override void Validate()
		{
			var sb = _options.Validate();
			var error = sb?.ToString();
			if (!string.IsNullOrWhiteSpace(error))
				throw new ConfigurationException(error);
		}

		internal ScenarioOptions GetOptions()
		{
			Validate();
			return _options;
		}
	}
}

using Raider.Exceptions;
using Raider.ServiceBus.Config.Components;
using Raider.ServiceBus.Config.Components.Internal;
using System;

namespace Raider.ServiceBus.Config.Fluent
{
	public interface IComponentQueueBuilder<TBuilder, TObject>
		where TBuilder : IComponentQueueBuilder<TBuilder, TObject>
		where TObject : ComponentQueueOptions
	{
		IComponentQueue Build(IComponent component, IServiceProvider serviceProvider);

		TBuilder MessageType<TMessage>(bool force = true)
			where TMessage : IBaseRequestMessage;

		TBuilder Name(string name, bool force = true);
		
		//TBuilder CrlType<TQueue>(bool force = true)
		//	where TQueue : IQueue;

		TBuilder Description(string description, bool force = true);
		TBuilder IsFIFO(bool isFIFO);
		TBuilder ProcessingTimeoutInSeconds(int processingTimeoutInSeconds);
		TBuilder MaxRetryCount(int maxRetryCount);
	}

	public abstract class ComponentQueueBuilderBase<TBuilder, TObject> : IComponentQueueBuilder<TBuilder, TObject>
		where TBuilder : ComponentQueueBuilderBase<TBuilder, TObject>
		where TObject : ComponentQueueOptions
	{
		protected readonly TBuilder _builder;
		protected TObject _options;

		protected ComponentQueueBuilderBase(TObject options)
		{
			_options = options;
			_builder = (TBuilder)this;
		}

		public abstract IComponentQueue Build(IComponent component, IServiceProvider serviceProvider);

		public abstract void Validate();

		public TBuilder MessageType<TMessage>(bool force = true)
			where TMessage : IBaseRequestMessage
		{
			if (force || _options.MessageType == null)
				_options.MessageType = typeof(TMessage);

			return _builder;
		}

		public virtual TBuilder Name(string name, bool force = true)
		{
			if (force || string.IsNullOrWhiteSpace(_options.Name))
				_options.Name = name;

			return _builder;
		}

		//public virtual TBuilder CrlType<TQueue>(bool force = true)
		//	where TQueue : IQueue
		//{
		//	if (force || _options.CrlType == null)
		//		_options.CrlType = typeof(TQueue);

		//	return _builder;
		//}

		public virtual TBuilder Description(string description, bool force = true)
		{
			if (force || string.IsNullOrWhiteSpace(_options.Description))
				_options.Description = description;

			return _builder;
		}

		public virtual TBuilder IsFIFO(bool isFIFO)
		{
			_options.IsFIFO = isFIFO;
			return _builder;
		}

		public virtual TBuilder ProcessingTimeoutInSeconds(int processingTimeoutInSeconds)
		{
			_options.ProcessingTimeoutInSeconds = processingTimeoutInSeconds;
			return _builder;
		}

		public virtual TBuilder MaxRetryCount(int maxRetryCount)
		{
			_options.MaxRetryCount = maxRetryCount;
			return _builder;
		}
	}

	public class ComponentQueueBuilder : ComponentQueueBuilderBase<ComponentQueueBuilder, ComponentQueueOptions>
	{
		public ComponentQueueBuilder()
			: base(new ComponentQueueOptions())
		{
		}

		public override IComponentQueue Build(IComponent component, IServiceProvider serviceProvider)
		{
			return new ComponentQueue(component, GetOptions(), serviceProvider);
		}

		public override void Validate()
		{
			var sb = _options.Validate();
			var error = sb?.ToString();
			if (!string.IsNullOrWhiteSpace(error))
				throw new ConfigurationException(error);
		}

		internal ComponentQueueOptions GetOptions()
		{
			Validate();
			return _options;
		}
	}
}

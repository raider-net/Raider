using Raider.Converters;
using Raider.ServiceBus.Model;
using System;

namespace Raider.ServiceBus.Config.Components.Internal
{
	internal class ComponentQueue : IComponentQueue
	{
		private readonly ComponentQueueOptions _options;
		private readonly IServiceProvider _serviceProvider;

		public IComponent Component { get; }
		public Guid IdComponentQueue { get; }
		public Type MessageType { get; }
		public IMessageType MessageTypeModel { get; set; }
		public string Name { get; }
		//public Type CrlType { get; }
		public string? Description { get; set; }
		public DateTime? LastMessageDeliveryUtc { get; set; }
		public bool IsFIFO { get; set; }
		public int ProcessingTimeoutInSeconds { get; set; }
		public int MaxRetryCount { get; set; }

		public ComponentQueue(IComponent component, ComponentQueueOptions options, IServiceProvider serviceProvider)
		{
			Component = component ?? throw new ArgumentNullException(nameof(component));
			_options = options ?? throw new ArgumentNullException(nameof(options));
			_serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

			IdComponentQueue = GuidConverter.ToGuid(_options.Name);
			Component = component;
			MessageType = _options.MessageType;
			Name = _options.Name;
			//CrlType = _options.CrlType;
			Description = _options.Description;
			IsFIFO = _options.IsFIFO;
			ProcessingTimeoutInSeconds = _options.ProcessingTimeoutInSeconds;
			MaxRetryCount = _options.MaxRetryCount;
		}
	}
}

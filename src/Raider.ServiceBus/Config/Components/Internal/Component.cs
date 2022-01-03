using Raider.Converters;
using Raider.ServiceBus.Components;
using Raider.ServiceBus.Resolver;
using System;
using System.Collections.Generic;

namespace Raider.ServiceBus.Config.Components.Internal
{
	internal class Component : IComponent
	{
		private readonly ComponentOptions _options;
		private readonly IServiceProvider _serviceProvider;

		public IScenario Scenario { get; }
		public Guid IdComponent { get; }
		public string Name { get; }
		public Type CrlType { get; }
		public string ResolvedCrlType { get; }
		public string? Description { get; set; }
		public int ThrottleDelayInMilliseconds { get; set; }
		public int InactivityTimeoutInSeconds { get; set; }
		public int ShutdownTimeoutInSeconds { get; set; }
		public Guid? IdCurrentSession { get; set; }
		public ComponentStatus ComponentStatus { get; set; }
		int IComponent.IdComponentStatus => (int)ComponentStatus;
		public DateTime? LastStartTimeUtc { get; set; }
		public List<IComponentQueue> ComponentQueues { get; }

		public Component(ITypeResolver typeResolver, IScenario scenario, ComponentOptions options, IServiceProvider serviceProvider)
		{
			if (typeResolver == null)
				throw new ArgumentNullException(nameof(typeResolver));

			Scenario = scenario ?? throw new ArgumentNullException(nameof(scenario));
			_options = options ?? throw new ArgumentNullException(nameof(options));
			_serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

			ResolvedCrlType = typeResolver.ToName(_options.CrlType);
			if (string.IsNullOrWhiteSpace(ResolvedCrlType))
				throw new InvalidOperationException($"Message type {_options.CrlType} {nameof(ResolvedCrlType)} == NULL | {nameof(scenario)} = {scenario?.Name} | {nameof(Component)} = {CrlType?.FullName}");

			IdComponent = GuidConverter.ToGuid(ResolvedCrlType);
			Scenario = scenario;
			Name = _options.Name;
			CrlType = _options.CrlType;
			Description = _options.Description;
			ThrottleDelayInMilliseconds = _options.ThrottleDelayInMilliseconds;
			InactivityTimeoutInSeconds = _options.InactivityTimeoutInSeconds;
			ShutdownTimeoutInSeconds = _options.ShutdownTimeoutInSeconds;
			ComponentStatus = ComponentStatus.Idle;
			ComponentQueues = new List<IComponentQueue>();
		}
	}
}

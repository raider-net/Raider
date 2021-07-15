using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace Raider.Messaging
{
	public abstract class ComponentBase : IComponent
	{
		public abstract int IdComponent { get; }
		public abstract Guid IdInstance { get; }
		public abstract bool Initialized { get; protected set; }
		public abstract bool Started { get; protected set; }
		public abstract string Name { get; }
		public abstract string? Description { get; set; }
		public abstract int IdScenario { get; }
		public abstract DateTime LastActivityUtc { get; protected set; }
		public abstract ComponentState State { get; protected set; }
		public abstract IReadOnlyDictionary<object, object> ServiceBusHostProperties { get; }

		protected internal abstract Task StartAsync(IServiceBusStorageContext context, CancellationToken cancellationToken);

		Task IComponent.StartAsync(IServiceBusStorageContext context, CancellationToken cancellationToken)
			=> StartAsync(context, cancellationToken);

		public bool TryGetProperty<T>(object key, [NotNullWhen(true)] out T? value)
		{
			if (ServiceBusHostProperties.TryGetValue(key, out object? val) && val is T result)
			{
				value = result;
				return true;
			}

			value = default;
			return false;
		}
	}
}

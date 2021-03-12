using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Raider.DependencyInjection;
using Raider.Messaging.Internal;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Raider.Messaging.Extensions
{
	public static class ServiceCollectionExtensions
	{
		public static IServiceCollection AddRaiderMessaging<TMessageBox>(this IServiceCollection services, ServiceBusMode mode, bool allowJobs, Action<IServiceBusRegister> registerConfiguration, bool throwIfNotSubscribedMessageFound, bool throwIfNotPublishedMessageFound)
			where TMessageBox : class, IMessageBox
		{
			if (registerConfiguration == null)
				throw new ArgumentNullException(nameof(registerConfiguration));

			AddServices(services, mode, allowJobs, registerConfiguration, out List<Type> notSubscribed, out List<Type> notPublished);

			if (throwIfNotSubscribedMessageFound && 0 < notSubscribed.Count)
				throw new InvalidOperationException($"Not subscribed message data types:{Environment.NewLine}{string.Join(Environment.NewLine, notSubscribed.Select(x => x.FullName))}");

			if (throwIfNotPublishedMessageFound && 0 < notPublished.Count)
				throw new InvalidOperationException($"Not published message data types:{Environment.NewLine}{string.Join(Environment.NewLine, notPublished.Select(x => x.FullName))}");

			services.TryAddSingleton<IMessageBox, TMessageBox>();

			return services;
		}

		public static IServiceCollection AddRaiderMessaging(this IServiceCollection services, ServiceBusMode mode, bool allowJobs, Action<IServiceBusRegister> registerConfiguration, bool throwIfNotSubscribedMessageFound, bool throwIfNotPublishedMessageFound)
		{
			if (registerConfiguration == null)
				throw new ArgumentNullException(nameof(registerConfiguration));

			AddServices(services, mode, allowJobs, registerConfiguration, out List<Type> notSubscribed, out List<Type> notPublished);

			if (throwIfNotSubscribedMessageFound && 0 < notSubscribed.Count)
				throw new InvalidOperationException($"Not subscribed message data types:{Environment.NewLine}{string.Join(Environment.NewLine, notSubscribed.Select(x => x.FullName))}");

			if (throwIfNotPublishedMessageFound && 0 < notPublished.Count)
				throw new InvalidOperationException($"Not published message data types:{Environment.NewLine}{string.Join(Environment.NewLine, notPublished.Select(x => x.FullName))}");

			services.TryAddSingleton<IMessageBox, InMemoryMessageBox>();

			return services;
		}

		private static IServiceCollection AddServices(IServiceCollection services, ServiceBusMode mode, bool allowJobs, Action<IServiceBusRegister> registerConfiguration, out List<Type> notSubscribed, out List<Type> notPublished)
		{
			services.TryAddTransient<ServiceFactory>(p => p.GetService);
			services.AddTransient<SubscriberContext>();

			var register = new ServiceBusRegister(mode, allowJobs);
			registerConfiguration?.Invoke(register);
			register.FinalizeRegistration(out notSubscribed, out notPublished);

			services.TryAddSingleton<IServiceBusRegister>(register);
			services.TryAddSingleton<IServiceBus, ServiceBusPublisher>();

			if (mode == ServiceBusMode.PublishingAndSubscribing
				|| mode == ServiceBusMode.OnlyMessageSubscribing
				|| allowJobs)
			{
				services.AddHostedService<ServiceBusHost>();
			}

			return services;
		}
	}
}

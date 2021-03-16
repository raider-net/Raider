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
		public static IServiceCollection AddRaiderMessaging<TStorage>(this IServiceCollection services, Action<ServiceBusConfig> configuration)
			where TStorage : class, IServiceBusStorage
		{
			if (configuration == null)
				throw new ArgumentNullException(nameof(configuration));

			var cfg = new ServiceBusConfig();
			configuration?.Invoke(cfg);

			if (cfg.ServiceBusHostOptions == null)
				throw new InvalidOperationException($"{nameof(configuration)}.{nameof(cfg.ServiceBusHostOptions)} == null");

			if (cfg.RegisterConfiguration == null)
				throw new InvalidOperationException($"{nameof(configuration)}.{nameof(cfg.RegisterConfiguration)} == null");

			AddServices(services, cfg.Mode, cfg.AllowJobs, cfg.RegisterConfiguration, out List<Type> notSubscribed, out List<Type> notPublished);

			if (cfg.ThrowIfNotSubscribedMessageFound && 0 < notSubscribed.Count)
				throw new InvalidOperationException($"Not subscribed message data types:{Environment.NewLine}{string.Join(Environment.NewLine, notSubscribed.Select(x => x.FullName))}");

			if (cfg.ThrowIfNotPublishedMessageFound && 0 < notPublished.Count)
				throw new InvalidOperationException($"Not published message data types:{Environment.NewLine}{string.Join(Environment.NewLine, notPublished.Select(x => x.FullName))}");

			services.Configure<ServiceBusHostOptions>(o =>
			{
				o.ConncetionString = cfg.ServiceBusHostOptions.ConncetionString;
				o.ServiceHostStartMaxRetryCount = cfg.ServiceBusHostOptions.ServiceHostStartMaxRetryCount;
				o.IdServiceBusHost = cfg.ServiceBusHostOptions.IdServiceBusHost;
				o.Name = cfg.ServiceBusHostOptions.Name;
				o.Description = cfg.ServiceBusHostOptions.Description;
				o.IdUser = cfg.ServiceBusHostOptions.IdUser;
			});

			services.TryAddSingleton<IServiceBusStorage, TStorage>();
			services.TryAddSingleton<IMessageBox>(sp => sp.GetRequiredService<IServiceBusStorage>());

			return services;
		}

		//[Obsolete("Use AddRaiderMessaging<TMessageBox, TStorage> instead")]
		//public static IServiceCollection AddRaiderMessaging(this IServiceCollection services, ServiceBusMode mode, bool allowJobs, Action<IServiceBusRegister> registerConfiguration, bool throwIfNotSubscribedMessageFound, bool throwIfNotPublishedMessageFound)
		//{
		//	if (registerConfiguration == null)
		//		throw new ArgumentNullException(nameof(registerConfiguration));

		//	AddServices(services, mode, allowJobs, registerConfiguration, out List<Type> notSubscribed, out List<Type> notPublished);

		//	if (throwIfNotSubscribedMessageFound && 0 < notSubscribed.Count)
		//		throw new InvalidOperationException($"Not subscribed message data types:{Environment.NewLine}{string.Join(Environment.NewLine, notSubscribed.Select(x => x.FullName))}");

		//	if (throwIfNotPublishedMessageFound && 0 < notPublished.Count)
		//		throw new InvalidOperationException($"Not published message data types:{Environment.NewLine}{string.Join(Environment.NewLine, notPublished.Select(x => x.FullName))}");

		//	services.TryAddSingleton<IMessageBox, InMemoryMessageBox>();

		//	return services;
		//}

		private static IServiceCollection AddServices(IServiceCollection services, ServiceBusMode mode, bool allowJobs, Action<IServiceBusRegister> registerConfiguration, out List<Type> notSubscribed, out List<Type> notPublished)
		{
			services.TryAddTransient<ServiceFactory>(p => p.GetService);
			services.AddTransient<SubscriberContext>();

			var register = new ServiceBusRegister(mode, allowJobs);
			registerConfiguration?.Invoke(register);
			register.FinalizeRegistration(out notSubscribed, out notPublished);

			services.TryAddSingleton<IServiceBusRegister>(register);
			services.TryAddSingleton<IServiceBus, ServiceBus>();

			services.AddHostedService<ServiceBusHostService>();

			return services;
		}
	}
}

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Raider.DependencyInjection;
using Raider.Messaging.Internal;
using System;

namespace Raider.Messaging.Extensions
{
	public static class ServiceCollectionExtensions
	{
		public static IServiceCollection AddRaiderMessaging<TStorage, TAuthMngr>(this IServiceCollection services, Action<ServiceBusConfig> configuration)
			where TStorage : class, IServiceBusStorage
			where TAuthMngr : class, IServiceBusAuthenticationManager
		{
			if (configuration == null)
				throw new ArgumentNullException(nameof(configuration));

			var cfg = new ServiceBusConfig();
			configuration?.Invoke(cfg);

			if (cfg.ServiceBusHostOptions == null)
				throw new InvalidOperationException($"{nameof(configuration)}.{nameof(cfg.ServiceBusHostOptions)} == null");

			if (cfg.RegisterConfiguration == null)
				throw new InvalidOperationException($"{nameof(configuration)}.{nameof(cfg.RegisterConfiguration)} == null");

			AddServices(services, cfg.Mode, cfg.AllowJobs, cfg.RegisterConfiguration);

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
			services.TryAddSingleton<IServiceBusAuthenticationManager, TAuthMngr>();
			services.TryAddSingleton<IMessageBox>(sp => sp.GetRequiredService<IServiceBusStorage>());

			return services;
		}

		private static IServiceCollection AddServices(IServiceCollection services, ServiceBusMode mode, bool allowJobs, Action<IServiceBusRegister> registerConfiguration)
		{
			services.TryAddTransient<ServiceFactory>(p => p.GetService);
			services.AddTransient<SubscriberContext>();

			var register = new ServiceBusRegister(mode, allowJobs);
			registerConfiguration?.Invoke(register);
			register.FinalizeRegistration();

			services.TryAddSingleton<IServiceBusRegister>(register);
			services.TryAddSingleton<IServiceBus, ServiceBus>();

			services.AddHostedService<ServiceBusHostService>();

			return services;
		}
	}
}

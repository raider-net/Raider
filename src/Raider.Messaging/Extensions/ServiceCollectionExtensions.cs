using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
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

			if (cfg.ServiceBusOptions == null)
				throw new InvalidOperationException($"{nameof(configuration)}.{nameof(cfg.ServiceBusOptions)} == null");

			if (cfg.RegisterConfiguration == null)
				throw new InvalidOperationException($"{nameof(configuration)}.{nameof(cfg.RegisterConfiguration)} == null");

			AddServices(services, cfg.Mode, cfg.AllowJobs, cfg.RegisterConfiguration);

			services.Configure<ServiceBusOptions>(o =>
			{
				o.ConncetionString = cfg.ServiceBusOptions.ConncetionString;
				o.ServiceHostStartMaxRetryCount = cfg.ServiceBusOptions.ServiceHostStartMaxRetryCount;
				o.IdServiceBusHost = cfg.ServiceBusOptions.IdServiceBusHost;
				o.Name = cfg.ServiceBusOptions.Name;
				o.Description = cfg.ServiceBusOptions.Description;
				o.IdUser = cfg.ServiceBusOptions.IdUser;
				o.AddProperties(cfg.ServiceBusOptions.Properties);
			});

			services.TryAddSingleton<IServiceBusStorage, TStorage>();
			services.TryAddSingleton<IServiceBusAuthenticationManager, TAuthMngr>();
			services.TryAddSingleton<IMessageBox>(sp => sp.GetRequiredService<IServiceBusStorage>());

			return services;
		}

		private static IServiceCollection AddServices(IServiceCollection services, ServiceBusMode mode, bool allowJobs, Action<IServiceBusRegister> registerConfiguration)
		{
			services.AddTransient<SubscriberContext>();
			
			if (allowJobs)
				services.AddTransient<JobContext>();

			var register = new ServiceBusRegister(mode, allowJobs);
			registerConfiguration?.Invoke(register);
			register.FinalizeRegistration();

			services.TryAddSingleton<IServiceBusRegister>(register);
			services.TryAddSingleton<IServiceBusPublisher, ServiceBusPublisher>();

			services.AddHostedService<ServiceBus>();

			return services;
		}
	}
}

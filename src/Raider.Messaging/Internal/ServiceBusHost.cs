using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Raider.Threading;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Raider.Messaging
{
	internal class ServiceBusHost : BackgroundService
	{
		private readonly IMessageBox _messageBox;
		private readonly IServiceBusRegister _register;
		private readonly ILoggerFactory _loggerFactory;
		private readonly ILogger _logger;

		private bool _initialized;

		public ServiceBusHost(IMessageBox messageBox, IServiceBusRegister register, ILoggerFactory loggerFactory)
		{
			_messageBox = messageBox ?? throw new ArgumentNullException(nameof(messageBox));
			_register = register ?? throw new ArgumentNullException(nameof(register));
			_loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
			_logger = _loggerFactory.CreateLogger<ServiceBusHost>();

			if (!_register.RegistrationFinished)
				throw new InvalidOperationException("Component registration was not finished.");
		}

		private readonly AsyncLock _initLock = new AsyncLock();
		private async Task InitializeAsync(CancellationToken stoppingToken)
		{
			if (_initialized)
				return;

			using (await _initLock.LockAsync())
			{
				if (_initialized)
					return;

				_logger.LogInformation($"{nameof(ServiceBusHost)} initialization started.");

				await _register.InitializeComponentsAsync(_messageBox, _loggerFactory, stoppingToken);

				_logger.LogInformation($"{nameof(ServiceBusHost)} initialization finished.");

				_initialized = true;
			}
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			await InitializeAsync(stoppingToken);
		}
	}
}

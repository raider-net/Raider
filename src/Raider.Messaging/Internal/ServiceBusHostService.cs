using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Raider.Extensions;
using Raider.Threading;
using Raider.Trace;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Raider.Messaging
{
	internal class ServiceBusHostService : BackgroundService
	{
		private readonly int _startMaxRetryCount;
		private readonly ServiceBusHost _serviceBusHost;
		private readonly IServiceScope _serviceScope;
		private readonly IServiceProvider _serviceProvider;
		private readonly IServiceBusStorage _storage;
		private readonly IMessageBox _messageBox;
		private readonly IServiceBusRegister _register;
		private readonly ILoggerFactory _loggerFactory;
		private readonly ILogger _fallbackLogger;

		private bool _initialized;
		private bool _started;

		public ServiceBusHostService(
			IOptions<ServiceBusHostOptions> options,
			IServiceProvider serviceProvider,
			IServiceBusStorage storage,
			IMessageBox messageBox,
			IServiceBusRegister register,
			ILoggerFactory loggerFactory)
		{
			var traceInfo = TraceInfo.Create(storage?.ServiceBusHost?.ApplicationContext.TraceInfo.IdUser, storage?.ServiceBusHost?.ApplicationContext.TraceInfo.RuntimeUniqueKey);

			if (loggerFactory == null)
			{
				Serilog.Log.Logger.Error($"{traceInfo}: {nameof(loggerFactory)} == null");
				throw new ArgumentNullException(nameof(loggerFactory));
			}
			_loggerFactory = loggerFactory;

			try
			{
				_fallbackLogger = _loggerFactory.CreateLogger<ServiceBusHostService>();
			}
			catch (Exception ex)
			{
				Serilog.Log.Logger.Error(ex, $"{traceInfo}: {nameof(_loggerFactory.CreateLogger)}");
				throw;
			}

			if (options?.Value == null)
			{
				ServiceBusLogger.SeriLogError(_fallbackLogger, traceInfo, $"{nameof(options)}?.Value == null", null);
				throw new ArgumentNullException(nameof(options));
			}

			var opt = options.Value;
			_startMaxRetryCount = opt.ServiceHostStartMaxRetryCount;
			if (_startMaxRetryCount < 0)
				_startMaxRetryCount = 0;

			if (serviceProvider == null)
			{
				ServiceBusLogger.SeriLogError(_fallbackLogger, traceInfo, $"{nameof(serviceProvider)} == null", null);
				throw new ArgumentNullException(nameof(serviceProvider));
			}
			_serviceScope = serviceProvider.CreateScope();
			_serviceProvider = _serviceScope.ServiceProvider;

			try
			{
				_serviceBusHost = new ServiceBusHost(opt, _serviceProvider)
				{
					Description = opt.Description
				};
			}
			catch (Exception ex)
			{
				ServiceBusLogger.SeriLogError(_fallbackLogger, traceInfo, $"new {nameof(ServiceBusHost)}.ctor(...)", ex);
				throw;
			}

			if (storage == null)
			{
				ServiceBusLogger.SeriLogError(_fallbackLogger, traceInfo, $"{nameof(storage)} == null", null);
				throw new ArgumentNullException(nameof(storage));
			}
			_storage = storage;

			if (messageBox == null)
			{
				ServiceBusLogger.SeriLogError(_fallbackLogger, traceInfo, $"{nameof(messageBox)} == null", null);
				throw new ArgumentNullException(nameof(messageBox));
			}
			_messageBox = messageBox;

			if (register == null)
			{
				ServiceBusLogger.SeriLogError(_fallbackLogger, traceInfo, $"{nameof(register)} == null", null);
				throw new ArgumentNullException(nameof(register));
			}
			_register = register;

			if (!_register.RegistrationFinished)
			{
				var error = "Component registration was not finished.";
				ServiceBusLogger.SeriLogError(_fallbackLogger, traceInfo, error, null);
				throw new InvalidOperationException(error);
			}
		}

		private readonly AsyncLock _initLock = new AsyncLock();
		private async Task InitializeAsync(int startRetryCount, CancellationToken cancellationToken)
		{
			var traceInfo = TraceInfo.Create(_serviceBusHost.Principal, _serviceBusHost.ApplicationContext.TraceInfo.RuntimeUniqueKey);

			if (_initialized)
			{
				var error = "Already initialized.";

				await LogErrorAsync(
					traceInfo,
					nameof(ServiceBusDefaults.LogMessageType.Init),
					error, null, cancellationToken);

				throw new InvalidOperationException(error);
			}

			using (await _initLock.LockAsync())
			{
				if (_initialized)
				{
					var error = "Already initialized.";

					await LogErrorAsync(
						traceInfo,
						nameof(ServiceBusDefaults.LogMessageType.Init),
						error, null, cancellationToken);

					throw new InvalidOperationException(error);
				}

				using var ctx = await _storage.CreateServiceBusStorageContextAsync(_serviceBusHost, cancellationToken);

				try
				{
					await _storage.SetServiceBusHost(ctx, _serviceBusHost, cancellationToken);
					await _storage.WriteServiceBusHostStartAsync(ctx, cancellationToken);
				}
				catch (Exception ex)
				{
					ServiceBusLogger.SeriLogError(_fallbackLogger, traceInfo, $"{nameof(IServiceBusStorage)}.{nameof(_storage.SetServiceBusHost)}", ex);
					throw;
				}

				await _register.InitializeComponentsAsync(_serviceProvider, _storage, ctx, _messageBox, _loggerFactory, cancellationToken);
				_initialized = true;

				try
				{
					await _register.StartComponentsAsync(ctx, cancellationToken);
					_started = true;
				}
				catch (Exception ex)
				{
					ServiceBusLogger.SeriLogError(_fallbackLogger, traceInfo, $"{nameof(_storage)}.{nameof(_storage.WriteServiceBusHostStartAsync)}", ex);
					throw;
				}
			}
		}

		private async Task LogErrorAsync(ITraceInfo traceInfo, string logMessageType, string message, Exception? ex, CancellationToken cancellationToken = default)
		{
			if (!_started || _storage == null)
			{
				ServiceBusLogger.SeriLogError(_fallbackLogger, traceInfo, message, ex);
			}
			else
			{
				try
				{
					if (traceInfo == null)
						traceInfo = TraceInfo.Create();

					await _storage.WriteServiceBusLogAsync(
						new LogError(traceInfo, logMessageType, message, ex?.ToStringTrace()),
						cancellationToken);
				}
				catch (Exception storageEx)
				{
					ServiceBusLogger.SeriLogError(_fallbackLogger, traceInfo, message, ex);
					ServiceBusLogger.SeriLogError(_fallbackLogger, traceInfo, $"{nameof(IServiceBusStorage)}.{nameof(_storage.WriteServiceBusLogAsync)}", storageEx);
					throw;
				}
			}
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			var startRetryCount = 0;
			stoppingToken.Register(ShutDown);

			// Without this line we can encounter a blocking issue such as: https://github.com/dotnet/extensions/issues/2816
			await Task.Yield();

			while (!_started)
			{
				try
				{
					await _serviceBusHost.Login(_serviceProvider);
					await InitializeAsync(startRetryCount, stoppingToken);
				}
				catch
				{
					if (_startMaxRetryCount <= startRetryCount)
						throw;
				}
				startRetryCount++;
			}
		}

		private void ShutDown()
		{
			_serviceScope?.Dispose();
		}
	}
}

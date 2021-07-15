using Microsoft.Extensions.Logging;
using Raider.Messaging.Messages;
using Raider.Trace;
using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace Raider.Messaging
{
	internal class ServiceBusPublisher : IServiceBusPublisher
	{
		private readonly IServiceBusRegister _register;
		private readonly ILoggerFactory _loggerFactory;
		private readonly ILogger _fallbackLogger;

		public ServiceBusPublisher(IServiceBusRegister register, ILoggerFactory loggerFactory)
		{
			var traceInfo = TraceInfo.Create();

			if (loggerFactory == null)
			{
				Serilog.Log.Logger.Error($"{traceInfo}: {nameof(loggerFactory)} == null");
				throw new ArgumentNullException(nameof(loggerFactory));
			}
			_loggerFactory = loggerFactory;

			try
			{
				_fallbackLogger = _loggerFactory.CreateLogger<ServiceBus>();
			}
			catch (Exception ex)
			{
				Serilog.Log.Logger.Error(ex, $"{traceInfo}: {nameof(_loggerFactory.CreateLogger)}");
				throw;
			}

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

		public Task<IMessage<TData>> PublishMessageAsync<TData>(int idPublisher, TData mesage, IMessage? previousMessage = null, DateTimeOffset? validToUtc = null, bool isRecovery = false, IDbTransaction? dbTransaction = null, CancellationToken token = default) where TData : IMessageData
		{
			var publisher = _register.TryGetPublisher<TData>(idPublisher);
			if (publisher == null)
				throw new ArgumentException($"Not registered {nameof(idPublisher)}: {idPublisher}", nameof(idPublisher));

			return publisher.PublishMessageAsync(mesage, previousMessage, validToUtc, isRecovery, dbTransaction, token);
		}
	}
}

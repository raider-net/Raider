using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Raider.Identity;
using Raider.Localization;
using Raider.Logging;
using Raider.Services.Commands;
using Raider.Trace;
using Raider.Web;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Raider.Services
{
	public class ServiceContext : IServiceContext, ICommandServiceContext
	{
		private CommandHandlerContext _commandHandlerContext;

		public IServiceProvider ServiceProvider => _commandHandlerContext.ServiceProvider;

		public ITraceInfo TraceInfo { get; private set; }
		public IApplicationContext ApplicationContext => _commandHandlerContext.ApplicationContext;
		public IApplicationResources ApplicationResources => _commandHandlerContext.ApplicationResources;
		public IRequestMetadata? RequestMetadata => _commandHandlerContext.RequestMetadata;
		public RaiderIdentity<int>? User => _commandHandlerContext.User;

		public string? CommandName => _commandHandlerContext.CommandName;

		public Guid? IdCommandEntry => _commandHandlerContext.IdCommandEntry;

		public ILogger Logger { get; private set; }

		public Dictionary<object, object?> CommandHandlerItems => _commandHandlerContext.CommandHandlerItems;

		public Type ForServiceType { get; private set; }
		public bool AllowCommit { get; set; }
		public Dictionary<object, object?> LocalItems { get; } = new Dictionary<object, object?>();

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
		public ServiceContext() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

		//public ServiceContext(ITraceFrame currentTraceFrame, CommandHandlerContext commandHandlerContext, Type serviceType)
		//{
		//	_commandHandlerContext = commandHandlerContext ?? throw new ArgumentNullException(nameof(commandHandlerContext));
		//	OnSetCommandHandlerContext(_commandHandlerContext);
		//	TraceInfo = new TraceInfoBuilder(currentTraceFrame, commandHandlerContext.TraceInfo).Build();
		//	ForServiceType = serviceType;

		//	var loggerFactory = ServiceProvider.GetRequiredInstance<ILoggerFactory>();
		//	var serviceLogger = loggerFactory.CreateLogger(serviceType);
		//	Logger = serviceLogger;
		//}

		//internal ServiceContext(ITraceInfo traceInfo, CommandHandlerContext commandHandlerContext, Type serviceType)
		//{
		//	_commandHandlerContext = commandHandlerContext ?? throw new ArgumentNullException(nameof(commandHandlerContext));
		//	OnSetCommandHandlerContext(_commandHandlerContext);
		//	TraceInfo = traceInfo ?? throw new ArgumentNullException(nameof(traceInfo));
		//	ForServiceType = serviceType;

		//	var loggerFactory = ServiceProvider.GetRequiredInstance<ILoggerFactory>();
		//	var serviceLogger = loggerFactory.CreateLogger(serviceType);
		//	Logger = serviceLogger;
		//}

		internal void Init(ITraceFrame currentTraceFrame, CommandHandlerContext commandHandlerContext, Type serviceType)
		{
			_commandHandlerContext = commandHandlerContext ?? throw new ArgumentNullException(nameof(commandHandlerContext));
			OnSetCommandHandlerContext(_commandHandlerContext);
			TraceInfo = new TraceInfoBuilder(currentTraceFrame, commandHandlerContext.TraceInfo).Build();
			ForServiceType = serviceType;

			var loggerFactory = ServiceProvider.GetRequiredService<ILoggerFactory>();
			var serviceLogger = loggerFactory.CreateLogger(serviceType);
			Logger = serviceLogger;
		}

		internal void Init(ITraceInfo traceInfo, CommandHandlerContext commandHandlerContext, Type serviceType)
		{
			_commandHandlerContext = commandHandlerContext ?? throw new ArgumentNullException(nameof(commandHandlerContext));
			OnSetCommandHandlerContext(_commandHandlerContext);
			TraceInfo = traceInfo ?? throw new ArgumentNullException(nameof(traceInfo));
			ForServiceType = serviceType;

			var loggerFactory = ServiceProvider.GetRequiredService<ILoggerFactory>();
			var serviceLogger = loggerFactory.CreateLogger(serviceType);
			Logger = serviceLogger;
		}

		protected virtual void OnSetCommandHandlerContext(CommandHandlerContext commandHandlerContext)
		{
		}

		public TService GetService<TService, TServiceContext>(
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
			where TServiceContext : ServiceContext, new()
			where TService : ServiceBase<TServiceContext>
			=> _commandHandlerContext.GetService<TService, TServiceContext>(memberName, sourceFilePath, sourceLineNumber);

		public Task<TService> GetServiceAsync<TService, TServiceContext>(
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0,
			CancellationToken cancellationToken = default)
			where TServiceContext : ServiceContext, new()
			where TService : ServiceBase<TServiceContext>
			=> _commandHandlerContext.GetServiceAsync<TService, TServiceContext>(memberName, sourceFilePath, sourceLineNumber, cancellationToken);

		public MethodLogScope CreateScope(
			IEnumerable<MethodParameter>? methodParameters = null,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
			=> _commandHandlerContext.CreateScope(methodParameters, memberName, sourceFilePath, sourceLineNumber);

		public bool TryGetCommandHandlerItem<TKey, TValue>(TKey key, out TValue? value)
			=> _commandHandlerContext.TryGetCommandHandlerItem(key, out value);

		public void LogTraceMessage(ILogMessage message)
			=> _commandHandlerContext.LogTraceMessage(message);

		public ILogMessage? LogTraceMessage(MethodLogScope scope, Action<LogMessageBuilder> messageBuilder)
			=> _commandHandlerContext.LogTraceMessage(scope, messageBuilder);

		public void LogDebugMessage(ILogMessage message)
			=> _commandHandlerContext.LogDebugMessage(message);

		public ILogMessage? LogDebugMessage(MethodLogScope scope, Action<LogMessageBuilder> messageBuilder)
			=> _commandHandlerContext.LogDebugMessage(scope, messageBuilder);

		public void LogInformationMessage(ILogMessage message)
			=> _commandHandlerContext.LogInformationMessage(message);

		public ILogMessage? LogInformationMessage(MethodLogScope scope, Action<LogMessageBuilder> messageBuilder)
			=> _commandHandlerContext.LogInformationMessage(scope, messageBuilder);

		public void LogWarningMessage(ILogMessage message)
			=> _commandHandlerContext.LogWarningMessage(message);

		public ILogMessage? LogWarningMessage(MethodLogScope scope, Action<LogMessageBuilder> messageBuilder)
			=> _commandHandlerContext.LogWarningMessage(scope, messageBuilder);

		public void LogErrorMessage(IErrorMessage message)
			=> _commandHandlerContext.LogErrorMessage(message);

		public IErrorMessage LogErrorMessage(MethodLogScope scope, Action<ErrorMessageBuilder> messageBuilder)
			=> _commandHandlerContext.LogErrorMessage(scope, messageBuilder);

		public void LogCriticalMessage(IErrorMessage message)
			=> _commandHandlerContext.LogCriticalMessage(message);

		public IErrorMessage LogCriticalMessage(MethodLogScope scope, Action<ErrorMessageBuilder> messageBuilder)
			=> _commandHandlerContext.LogCriticalMessage(scope, messageBuilder);

		public bool TryGetLocalItem<TKey, TValue>(TKey key, out TValue? value)
		{
			value = default;

			if (key == null)
				return false;

			if (LocalItems.TryGetValue(key, out object? obj))
			{
				if (obj is TValue val)
				{
					value = val;
					return true;
				}
				else
				{
					throw new InvalidOperationException($"TryGetItem: Key = {typeof(TKey).FullName} && {obj?.GetType().FullName} != {typeof(TValue).FullName}");
				}
			}

			return false;
		}
	}
}

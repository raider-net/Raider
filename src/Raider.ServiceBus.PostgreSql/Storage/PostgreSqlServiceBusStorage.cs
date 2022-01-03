using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql;
using NpgsqlTypes;
using Raider.Infrastructure;
using Raider.Serializer;
using Raider.ServiceBus.BusLogger;
using Raider.ServiceBus.Components;
using Raider.ServiceBus.Config.Components;
using Raider.ServiceBus.Model;
using Raider.ServiceBus.PostgreSql.Messages.Storage.Model;
using Raider.ServiceBus.Resolver;
using Raider.Trace;
using Raider.Transactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Raider.ServiceBus.PostgreSql.Storage
{
	internal partial class PostgreSqlServiceBusStorage : PostgreSqlHostStorage
	{
		private readonly IPostgreSqlServiceBusOptions _options;
		private readonly ITypeResolver _typeResolver;
		private readonly ISerializer _serialzier;

		private readonly ILogger _logger;
		private readonly IComponentLogger _baseComponentLogger;
		private readonly IMessageLogger _baseMessageLogger;

		public PostgreSqlServiceBusStorage(IPostgreSqlServiceBusOptions options, IServiceProvider serviceProvider)
			: base(options, serviceProvider)
		{
			if (serviceProvider == null)
				throw new ArgumentNullException(nameof(serviceProvider));

			_options = options ?? throw new ArgumentNullException(nameof(options));
			_typeResolver = _options.TypeResolver;

			var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();

			_logger = loggerFactory.CreateLogger<ILogger<PostgreSqlServiceBusStorage>>();

			var msComponentLogger = loggerFactory.CreateLogger<BaseComponentLogger>();
			_baseComponentLogger = new BaseComponentLogger(msComponentLogger);

			var msMessageLogger = loggerFactory.CreateLogger<BaseMessageLogger>();
			_baseMessageLogger = new BaseMessageLogger(msMessageLogger);

			_serialzier = _options.MessageSerializer(serviceProvider);

			if (_serialzier == null)
				throw new InvalidOperationException($"{nameof(_serialzier)} == NULL");
		}

		public async Task<InitializedHost> InitializeHostAsync(CancellationToken cancellationToken = default)
		{
			var messageTypes = new Dictionary<Type, IMessageType>();
			var scenarios = new List<IScenario>();
			foreach (var scenarioBuilder in _options.Scenarios)
			{
				var scenario = scenarioBuilder.Build(_typeResolver, ServiceProvider);
				scenarios.Add(scenario);

				foreach (var inboundComponent in scenario.InboundComponents)
				{
					foreach (var componentQueue in inboundComponent.ComponentQueues)
					{
						messageTypes.TryAdd(componentQueue.MessageType, componentQueue.MessageTypeModel);
					}
				}

				foreach (var businessProcess in scenario.BusinessProcesses)
				{
					foreach (var componentQueue in businessProcess.ComponentQueues)
					{
						messageTypes.TryAdd(componentQueue.MessageType, componentQueue.MessageTypeModel);
					}
				}

				foreach (var outboundComponent in scenario.OutboundComponents)
				{
					foreach (var componentQueue in outboundComponent.ComponentQueues)
					{
						messageTypes.TryAdd(componentQueue.MessageType, componentQueue.MessageTypeModel);
					}
				}
			}

			var messageTypesDict = 0 < messageTypes.Count
				? messageTypes.Values.Select(x => x.ToDictionary()).ToList()
				: null;

			var host = await base.InitializeHostInternalAsync(HostType.ServiceBus, messageTypesDict, cancellationToken);

			var transactionContext = await CreateTransactionContextAsync(cancellationToken);

			try
			{
				foreach (var scenario in scenarios)
				{
					await SaveScenario(scenario, host.IdHost, transactionContext, cancellationToken);

					foreach (var inboundComponent in scenario.InboundComponents)
					{
						await SaveComponent(inboundComponent, scenario.IdScenario, transactionContext, cancellationToken);

						await LogInformationAsync(
							TraceInfo.Create(),
							inboundComponent.IdComponent,
							ComponentStatus.Idle,
							x => x.Detail("START"),
							"START",
							transactionContext,
							cancellationToken);

						foreach (var componentQueue in inboundComponent.ComponentQueues)
							await SaveComponentQueue(componentQueue, inboundComponent.IdComponent, transactionContext, cancellationToken);
					}

					foreach (var businessProcess in scenario.BusinessProcesses)
					{
						await SaveComponent(businessProcess, scenario.IdScenario, transactionContext, cancellationToken);

						await LogInformationAsync(
							TraceInfo.Create(),
							businessProcess.IdComponent,
							ComponentStatus.Idle,
							x => x.Detail("START"),
							"START",
							transactionContext,
							cancellationToken);

						foreach (var componentQueue in businessProcess.ComponentQueues)
							await SaveComponentQueue(componentQueue, businessProcess.IdComponent, transactionContext, cancellationToken);
					}

					foreach (var outboundComponent in scenario.OutboundComponents)
					{
						await SaveComponent(outboundComponent, scenario.IdScenario, transactionContext, cancellationToken);

						await LogInformationAsync(
							TraceInfo.Create(),
							outboundComponent.IdComponent,
							ComponentStatus.Idle,
							x => x.Detail("START"),
							"START",
							transactionContext,
							cancellationToken);

						foreach (var componentQueue in outboundComponent.ComponentQueues)
							await SaveComponentQueue(componentQueue, outboundComponent.IdComponent, transactionContext, cancellationToken);
					}
				}

				await transactionContext.CommitAsync(cancellationToken);
			}
			catch (Exception ex)
			{
				LogCritical(TraceInfo.Create(), host.IdHost, HostStatus.Error, x => x.ExceptionInfo(ex).Detail($"{nameof(PostgreSqlServiceBusStorage)}.{nameof(InitializeHostAsync)} error."));

				try
				{
					await transactionContext.RollbackAsync(cancellationToken);
				}
				catch (Exception exRolback)
				{
					LogCritical(TraceInfo.Create(), host.IdHost, HostStatus.Error, x => x.ExceptionInfo(exRolback).Detail($"{nameof(PostgreSqlServiceBusStorage)}.{nameof(InitializeHostAsync)} error."));
				}
				throw;
			}
			finally
			{
				try
				{
					await transactionContext.DisposeAsync();
				}
				catch (Exception ex)
				{
					LogCritical(TraceInfo.Create(), host.IdHost, HostStatus.Error, x => x.ExceptionInfo(ex).Detail($"{nameof(PostgreSqlServiceBusStorage)}.{nameof(InitializeHostAsync)} {nameof(transactionContext)}.{nameof(transactionContext.DisposeAsync)}"));
				}
			}

			return new InitializedHost
			{
				Host = host,
				Scenarios = scenarios,
				MessageTypes = messageTypes
			};
		}
	}
}

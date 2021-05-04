using Microsoft.Extensions.Logging;
using Raider.Commands;
using Raider.Commands.Aspects;
using Raider.DependencyInjection;
using Raider.Diagnostics;
using Raider.Extensions;
using Raider.Logging;
using Raider.Logging.Extensions;
using Raider.Services.Commands;
using Raider.Trace;
using System;
using System.Data;

namespace Raider.Services.Aspects
{
	public class CommandInterceptor<TCommand, TResult, TContext, TBuilder> : InterceptorBase<TContext, TBuilder>, ICommandInterceptor<TCommand, TResult>
		where TCommand : ICommand<TResult>
		where TContext : CommandHandlerContext
		where TBuilder : CommandHandlerContext.Builder<TContext>
	{
		private readonly ILoggerFactory _loggerFactory;
		private readonly ILogger _logger;

		public CommandInterceptor(ServiceFactory serviceFactory, ILoggerFactory loggerFactory, ILogger<CommandInterceptor<TCommand, TResult, TContext, TBuilder>> logger)
			: base(serviceFactory)
		{
			_loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		public ICommandResult<bool> InterceptCanExecute(ITraceInfo previousTraceInfo, ICommandHandler<TCommand, TResult> handler, TCommand command, ICommandInterceptorOptions? options)
		{
			throw new NotImplementedException();

			long callStartTicks = StaticWatch.CurrentTicks;
			long callEndTicks;
			decimal methodCallElapsedMilliseconds = -1;
			Type? commandType = command?.GetType();
			var traceInfo = new TraceInfoBuilder(TraceFrame.Create(), previousTraceInfo).Build();
			using var scope = _logger.BeginMethodCallScope(traceInfo);

			_logger.LogTraceMessage(
				traceInfo,
				x => x.LogCode(LogCode.Method_In)
						.CommandQueryName(commandType?.FullName));

			if (handler.GetOptions() is not CommandHandlerOptions handlerOptions)
				handlerOptions = new CommandHandlerOptions();

			var resultBuilder = new CommandResultBuilder<bool>();
			var result = resultBuilder.Build();

			try
			{
				var commandHandlerContextBuilder =
					CreateCommandHandlerContext(traceInfo, _loggerFactory.CreateLogger(handler.GetType()))
						.CommandName(commandType?.FullName);

				try
				{
					var handlerResult = handler.CanExecute(command, commandHandlerContextBuilder.Context);
					if (handlerResult == null)
						throw new InvalidOperationException($"Handler {handler.GetType().FullName}.{nameof(handler.CanExecute)} returned null. Expected {typeof(ICommandResult<TResult>).FullName}");

					callEndTicks = StaticWatch.CurrentTicks;
					methodCallElapsedMilliseconds = StaticWatch.ElapsedMilliseconds(callStartTicks, callEndTicks);
				}
				catch (Exception executeEx)
				{
					callEndTicks = StaticWatch.CurrentTicks;
					methodCallElapsedMilliseconds = StaticWatch.ElapsedMilliseconds(callStartTicks, callEndTicks);

					result = new CommandResultBuilder<bool>()
						.WithError(traceInfo,
							x => x.ExceptionInfo(executeEx)
									.Detail($"Unhandled handler ({handler.GetType().FullName}) exception."))
						.Build();
				}
			}
			catch (Exception interEx)
			{
				callEndTicks = StaticWatch.CurrentTicks;
				methodCallElapsedMilliseconds = StaticWatch.ElapsedMilliseconds(callStartTicks, callEndTicks);

				result = new CommandResultBuilder<bool>()
					.WithError(traceInfo,
						x => x.ExceptionInfo(interEx)
								.Detail($"Unhandled interceptor ({this.GetType().FullName}) exception."))
					.Build();
			}
			finally
			{
				_logger.LogDebugMessage(
					traceInfo,
					x => x.LogCode(LogCode.Method_Out)
						.CommandQueryName(commandType?.FullName)
						.MethodCallElapsedMilliseconds(methodCallElapsedMilliseconds));
			}

			return result;
		}

		public ICommandResult<TResult> InterceptExecute(ITraceInfo previousTraceInfo, ICommandHandler<TCommand, TResult> handler, TCommand command, ICommandInterceptorOptions? options)
		{
			long callStartTicks = StaticWatch.CurrentTicks;
			long callEndTicks;
			decimal methodCallElapsedMilliseconds = -1;
			Type? commandType = command?.GetType();
			var traceInfo = new TraceInfoBuilder(TraceFrame.Create(), previousTraceInfo).Build();
			using var scope = _logger.BeginMethodCallScope(traceInfo);

			_logger.LogTraceMessage(
				traceInfo,
				x => x.LogCode(LogCode.Method_In)
						.CommandQueryName(commandType?.FullName));

			if (handler.GetOptions() is not CommandHandlerOptions handlerOptions)
				handlerOptions = new CommandHandlerOptions();

			var resultBuilder = new CommandResultBuilder<TResult>();
			var result = resultBuilder.Build();
			Guid? idCommand = null;
			TContext? context = default;

			try
			{
				var commandHandlerContextBuilder =
					CreateCommandHandlerContext(traceInfo, _loggerFactory.CreateLogger(handler.GetType()))
						.CommandName(commandType?.FullName);

				context = commandHandlerContextBuilder.Context;

				if (command is not CommandBase<TResult> commandBase)
					throw new InvalidOperationException($"Command {commandType?.FullName} must implement {typeof(CommandBase<TResult>).FullName}");

				ICommandLogger? commandEntryLogger = null;
				CommandEntry? commandEntry = null;
				long? startTicks = null;

				if (handlerOptions.LogCommandEntry)
				{
					startTicks = StaticWatch.CurrentTicks;
					commandEntryLogger = ServiceFactory.GetInstance<ICommandLogger>();
					if (commandEntryLogger == null)
						throw new InvalidOperationException($"{nameof(ICommandLogger)} is not configured");

					commandEntry = new CommandEntry(typeof(TCommand).ToFriendlyFullName(), traceInfo, handlerOptions.SerializeCommand ? commandBase.Serialize() : null);
					commandEntryLogger.WriteCommandEntry(commandEntry);
					idCommand = commandEntry?.IdCommandQueryEntry;
				}

				commandHandlerContextBuilder.IdCommandEntry(idCommand);

				try
				{
					var canExecuteContextBuilder =
						CreateCommandHandlerContext(traceInfo, _loggerFactory.CreateLogger(handler.GetType()))
						.CommandName(commandType?.FullName);

					var canExecuteResult = handler.CanExecute(command, canExecuteContextBuilder.Context);
					if (canExecuteResult == null)
						throw new InvalidOperationException($"Handler {handler.GetType().FullName}.{nameof(handler.CanExecute)} returned null. Expected {typeof(ICommandResult<bool>).FullName}");

					if (!resultBuilder.MergeHasError(canExecuteResult))
					{
						var executeResult = handler.Execute(command, context);
						if (executeResult == null)
							throw new InvalidOperationException($"Handler {handler.GetType().FullName}.{nameof(handler.Execute)} returned null. Expected {typeof(ICommandResult<TResult>).FullName}");

						resultBuilder.CopyAllHasError(executeResult);
					}

					if (result.HasError)
					{
						foreach (var errMsg in result.ErrorMessages)
						{
							if (string.IsNullOrWhiteSpace(errMsg.ClientMessage))
								errMsg.ClientMessage = context.ApplicationResources?.GlobalExceptionMessage;

							if (!errMsg.IdCommandQuery.HasValue)
								errMsg.IdCommandQuery = idCommand;

							_logger.LogErrorMessage(errMsg);
						}

						context.Rollback();
					}
					else
					{
						context.Commit();
					}

					callEndTicks = StaticWatch.CurrentTicks;
					methodCallElapsedMilliseconds = StaticWatch.ElapsedMilliseconds(callStartTicks, callEndTicks);
				}
				catch (Exception executeEx)
				{
					callEndTicks = StaticWatch.CurrentTicks;
					methodCallElapsedMilliseconds = StaticWatch.ElapsedMilliseconds(callStartTicks, callEndTicks);

					var hasTrans = context.HasTransaction();
					context.Rollback();

					var clientErrorMessage = context.GetDefaultClientErrorMessage(executeEx);

					result = new CommandResultBuilder<TResult>()
						.WithError(traceInfo,
							x => x.ExceptionInfo(executeEx)
									.Detail(hasTrans ? $"Unhandled handler ({handler.GetType().FullName}) exception. DbTransaction.Rollback() succeeded." : $"Unhandled handler ({handler.GetType().FullName}) exception.")
									.ClientMessage(clientErrorMessage, force: false)
									.IdCommandQuery(idCommand))
						.Build();

					foreach (var errMsg in result.ErrorMessages)
					{
						if (string.IsNullOrWhiteSpace(errMsg.ClientMessage))
							errMsg.ClientMessage = context.ApplicationResources?.GlobalExceptionMessage;

						if (!errMsg.IdCommandQuery.HasValue)
							errMsg.IdCommandQuery = idCommand;

						_logger.LogErrorMessage(errMsg);
					}
				}
				finally
				{
					if (handlerOptions.LogCommandEntry && commandEntryLogger != null && commandEntry != null && startTicks.HasValue)
					{
						long endTicks = StaticWatch.CurrentTicks;
						var elapsedMilliseconds = StaticWatch.ElapsedMilliseconds(startTicks.Value, endTicks);
						commandEntryLogger.WriteCommandExit(commandEntry, elapsedMilliseconds);
					}
				}
			}
			catch (Exception interEx)
			{
				callEndTicks = StaticWatch.CurrentTicks;
				methodCallElapsedMilliseconds = StaticWatch.ElapsedMilliseconds(callStartTicks, callEndTicks);

				result = new CommandResultBuilder<TResult>()
					.WithError(traceInfo,
						x => x.ExceptionInfo(interEx)
								.Detail($"Unhandled interceptor ({this.GetType().FullName}) exception.")
								.IdCommandQuery(idCommand))
					.Build();

				foreach (var errMsg in result.ErrorMessages)
				{
					if (string.IsNullOrWhiteSpace(errMsg.ClientMessage))
						errMsg.ClientMessage = context?.ApplicationResources?.GlobalExceptionMessage;

					if (!errMsg.IdCommandQuery.HasValue)
						errMsg.IdCommandQuery = idCommand;

					_logger.LogErrorMessage(errMsg);
				}
			}
			finally
			{
				try
				{
					context?.DisposeTransaction();
				}
				catch { }

				_logger.LogDebugMessage(
					traceInfo,
					x => x.LogCode(LogCode.Method_Out)
						.CommandQueryName(commandType?.FullName)
						.MethodCallElapsedMilliseconds(methodCallElapsedMilliseconds));
			}

			return result;
		}
	}
}

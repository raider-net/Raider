using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Raider.Commands;
using Raider.Commands.Aspects;
using Raider.DependencyInjection;
using Raider.Diagnostics;
using Raider.Logging;
using Raider.Logging.Extensions;
using Raider.Services.Commands;
using Raider.Trace;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Raider.Services.Aspects
{
	public class AsyncCommandInterceptor<TCommand, TContext, TBuilder> : InterceptorBase<TContext, TBuilder>, IAsyncCommandInterceptor<TCommand>
		where TCommand : ICommand
		where TContext : CommandHandlerContext
		where TBuilder : CommandHandlerContext.Builder<TContext>
	{
		private readonly ILoggerFactory _loggerFactory;
		private readonly ILogger _logger;

		public AsyncCommandInterceptor(ServiceFactory serviceFactory, ILoggerFactory loggerFactory, ILogger<AsyncCommandInterceptor<TCommand, TContext, TBuilder>> logger)
			: base(serviceFactory)
		{
			_loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		public async Task<ICommandResult<bool>> InterceptCanExecuteAsync(ITraceInfo previousTraceInfo, IAsyncCommandHandler<TCommand> handler, TCommand command, ICommandInterceptorOptions? options, CancellationToken cancellationToken)
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
					var handlerResult = await handler.CanExecuteAsync(command, commandHandlerContextBuilder.Context, cancellationToken);
					if (handlerResult == null)
						throw new InvalidOperationException($"Handler {handler.GetType().FullName}.{nameof(handler.CanExecuteAsync)} returned null. Expected {typeof(ICommandResult).FullName}");

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

		public async Task<ICommandResult> InterceptExecuteAsync(ITraceInfo previousTraceInfo, IAsyncCommandHandler<TCommand> handler, TCommand command, ICommandInterceptorOptions? options, CancellationToken cancellationToken)
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

			var resultBuilder = new CommandResultBuilder();
			var result = resultBuilder.Build();
			long? idCommand = null;
			IDbContextTransaction? tran = null;

			try
			{
				var commandHandlerContextBuilder =
					CreateCommandHandlerContext(traceInfo, _loggerFactory.CreateLogger(handler.GetType()))
						.CommandName(commandType?.FullName);

				if (command is not CommandBase commandBase)
					throw new InvalidOperationException($"Command {commandType?.FullName} must implement {typeof(CommandBase).FullName}");

				if (handlerOptions.SerializeCommand)
				{
					var commandJson = commandBase.Serialize();
					//TODO save serialized command to DB in separated transaction (like logging)
					idCommand = 1; //TODO set inserted command id
				}

				commandHandlerContextBuilder.IdCommandEntry(idCommand);

				//TODO ako nastavit transaction id, ked transakcia ani dbcontext este nevznikli
				//traceInfoBuilder
				//	.TransactionId(tran?.TransactionId.ToString());

				try
				{
					var canExecuteContextBuilder =
						CreateCommandHandlerContext(traceInfo, _loggerFactory.CreateLogger(handler.GetType()))
						.CommandName(commandType?.FullName);

					var canExecuteResult = await handler.CanExecuteAsync(command, canExecuteContextBuilder.Context, cancellationToken);
					if (canExecuteResult == null)
						throw new InvalidOperationException($"Handler {handler.GetType().FullName}.{nameof(handler.CanExecuteAsync)} returned null. Expected {typeof(ICommandResult<bool>).FullName}");

					if (!resultBuilder.MergeHasError(canExecuteResult))
					{
						var executeResult = await handler.ExecuteAsync(command, commandHandlerContextBuilder.Context, cancellationToken);
						if (executeResult == null)
							throw new InvalidOperationException($"Handler {handler.GetType().FullName}.{nameof(handler.ExecuteAsync)} returned null. Expected {typeof(ICommandResult).FullName}");

						resultBuilder.MergeHasError(executeResult);
					}

					tran = commandHandlerContextBuilder.Context.DbContextTransaction;

					if (result.HasError)
					{
						if (tran != null)
						{
							tran.Rollback();
						}
					}
					else
					{
						if (tran != null)
						{
							tran.Commit();
						}
					}

					callEndTicks = StaticWatch.CurrentTicks;
					methodCallElapsedMilliseconds = StaticWatch.ElapsedMilliseconds(callStartTicks, callEndTicks);
				}
				catch (Exception executeEx)
				{
					callEndTicks = StaticWatch.CurrentTicks;
					methodCallElapsedMilliseconds = StaticWatch.ElapsedMilliseconds(callStartTicks, callEndTicks);

					if (tran != null)
					{
						tran.Rollback();
					}

					result = new CommandResultBuilder()
						.WithError(traceInfo,
							x => x.ExceptionInfo(executeEx)
									.Detail(tran == null ? $"Unhandled handler ({handler.GetType().FullName}) exception." : $"Unhandled handler ({handler.GetType().FullName}) exception. DbTransaction.Rollback() succeeded.")
									.IdCommandQuery(idCommand))
						.Build();
				}
			}
			catch (Exception interEx)
			{
				callEndTicks = StaticWatch.CurrentTicks;
				methodCallElapsedMilliseconds = StaticWatch.ElapsedMilliseconds(callStartTicks, callEndTicks);

				result = new CommandResultBuilder()
					.WithError(traceInfo,
						x => x.ExceptionInfo(interEx)
								.Detail($"Unhandled interceptor ({this.GetType().FullName}) exception.")
								.IdCommandQuery(idCommand))
					.Build();
			}
			finally
			{
				try
				{
					if (tran != null)
					{
						await tran.DisposeAsync();
					}
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

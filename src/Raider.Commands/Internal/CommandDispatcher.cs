using Microsoft.Extensions.Logging;
using Raider.Commands.Aspects;
using Raider.Commands.Logging;
using Raider.DependencyInjection;
using Raider.Diagnostics;
using Raider.Localization;
using Raider.Logging;
using Raider.Logging.Extensions;
using Raider.Trace;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Raider.Commands.Internal
{
	internal class CommandDispatcher : ICommandDispatcher
	{
		private readonly ServiceFactory _serviceFactory;
		private readonly IApplicationContext _applicationContext;
		private readonly IApplicationResources _applicationResources;
		private readonly ILogger<CommandDispatcher> _logger;
		private readonly ICommandHandlerRegistry _handlerRegistry;
		private readonly ICommandHandlerFactory _handlerFactory;

		private static readonly ConcurrentDictionary<Type, CommandProcessorBase> _voidCommandProcessors = new ConcurrentDictionary<Type, CommandProcessorBase>();
		private static readonly ConcurrentDictionary<Type, CommandProcessorBase> _asyncVoidCommandProcessors = new ConcurrentDictionary<Type, CommandProcessorBase>();
		private static readonly ConcurrentDictionary<Type, CommandProcessorBase> _commandProcessors = new ConcurrentDictionary<Type, CommandProcessorBase>();
		private static readonly ConcurrentDictionary<Type, CommandProcessorBase> _asyncCommandProcessors = new ConcurrentDictionary<Type, CommandProcessorBase>();

		public CommandDispatcher(
			ServiceFactory serviceFactory,
			ICommandHandlerRegistry handlerRegistry,
			ICommandHandlerFactory handlerFactory,
			ILogger<CommandDispatcher> logger)
		{
			_handlerRegistry = handlerRegistry ?? throw new ArgumentNullException(nameof(handlerRegistry));
			_handlerFactory = handlerFactory ?? throw new ArgumentNullException(nameof(handlerFactory));
			_logger = logger;
			_serviceFactory = serviceFactory ?? throw new ArgumentNullException(nameof(serviceFactory));
			_applicationContext = _serviceFactory.GetRequiredInstance<IApplicationContext>();
			_applicationResources = _serviceFactory.GetRequiredInstance<IApplicationResources>();
		}

		public ICommandResult<bool> CanExecute(ICommand command, ICommandInterceptorOptions? options = default)
		{
			long callStartTicks = StaticWatch.CurrentTicks;
			long callEndTicks;
			decimal methodCallElapsedMilliseconds = -1;

			Type? commandType = command?.GetType();
			VoidCommandProcessor? commandProcessor = null;
			ICommandHandler? handler = null;
			var traceInfo = new TraceInfoBuilder(TraceFrame.Create(), _applicationContext.TraceInfo).Build();
			using var scope = _logger.BeginMethodCallScope(traceInfo);

			_logger.LogTraceMessage(
				traceInfo,
				x => x.LogCode(LogCode.MethodEntry)
						.CommandQueryName(commandType?.FullName));

			try
			{
				if (command == null || commandType == null)
					throw new ArgumentNullException(nameof(command));

				commandProcessor = (VoidCommandProcessor)_voidCommandProcessors.GetOrAdd(commandType,
					t => CreateCommandProcessor(typeof(VoidCommandProcessor<>).MakeGenericType(commandType)));

				handler = commandProcessor.CreateHandler();
				handler.Dispatcher = this;
				var result = commandProcessor.CanExecute(traceInfo, handler, command, options);

				callEndTicks = StaticWatch.CurrentTicks;
				methodCallElapsedMilliseconds = StaticWatch.ElapsedMilliseconds(callStartTicks, callEndTicks);

				return result;
			}
			catch (Exception ex)
			{
				callEndTicks = StaticWatch.CurrentTicks;
				methodCallElapsedMilliseconds = StaticWatch.ElapsedMilliseconds(callStartTicks, callEndTicks);

				var errorMessage =
					_logger.LogErrorMessage(
						traceInfo,
						x => x.ExceptionInfo(ex)
							.Detail($"{nameof(CanExecute)} error - Command = {command?.GetType().FullName ?? "NULL"}")
							.LogCode((long)CommandLogCode.CommandDispatcherError)
							.ClientMessage(_applicationResources.GlobalExceptionMessage)
							.CommandQueryName(commandType?.FullName));

				var result = new CommandResultInternal<bool>();
				result.ErrorMessages.Add(errorMessage);
				return result;
			}
			finally
			{
				commandProcessor?.DisposeHandler(handler);

				_logger.LogDebugMessage(
					traceInfo,
					x => x.LogCode(LogCode.MethodExit)
						.MethodCallElapsedMilliseconds(methodCallElapsedMilliseconds)
						.CommandQueryName(commandType?.FullName));
			}
		}

		public async Task<ICommandResult<bool>> CanExecuteAsync(ICommand command, ICommandInterceptorOptions? options = default, CancellationToken cancellationToken = default)
		{
			long callStartTicks = StaticWatch.CurrentTicks;
			long callEndTicks;
			decimal methodCallElapsedMilliseconds = -1;

			Type? commandType = command?.GetType();
			AsyncVoidCommandProcessor? commandProcessor = null;
			ICommandHandler? handler = null;
			var traceInfo = new TraceInfoBuilder(TraceFrame.Create(), _applicationContext.TraceInfo).Build();
			using var scope = _logger.BeginMethodCallScope(traceInfo);

			_logger.LogTraceMessage(
				traceInfo,
				x => x.LogCode(LogCode.MethodEntry)
						.CommandQueryName(commandType?.FullName));

			try
			{
				if (command == null || commandType == null)
					throw new ArgumentNullException(nameof(command));

				commandProcessor = (AsyncVoidCommandProcessor)_asyncVoidCommandProcessors.GetOrAdd(commandType,
					t => CreateCommandProcessor(typeof(AsyncVoidCommandProcessor<>).MakeGenericType(commandType)));

				handler = commandProcessor.CreateHandler();
				handler.Dispatcher = this;
				var result = await commandProcessor.CanExecuteAsync(traceInfo, handler, command, options, cancellationToken);

				callEndTicks = StaticWatch.CurrentTicks;
				methodCallElapsedMilliseconds = StaticWatch.ElapsedMilliseconds(callStartTicks, callEndTicks);

				return result;
			}
			catch (Exception ex)
			{
				callEndTicks = StaticWatch.CurrentTicks;
				methodCallElapsedMilliseconds = StaticWatch.ElapsedMilliseconds(callStartTicks, callEndTicks);

				var errorMessage =
					_logger.LogErrorMessage(
						traceInfo,
						x => x.ExceptionInfo(ex)
							.Detail($"{nameof(CanExecuteAsync)} error - Command = {command?.GetType().FullName ?? "NULL"}")
							.LogCode((long)CommandLogCode.CommandDispatcherError)
							.ClientMessage(_applicationResources.GlobalExceptionMessage)
							.CommandQueryName(commandType?.FullName));

				var result = new CommandResultInternal<bool>();
				result.ErrorMessages.Add(errorMessage);
				return result;
			}
			finally
			{
				commandProcessor?.DisposeHandler(handler);

				_logger.LogDebugMessage(
					traceInfo,
					x => x.LogCode(LogCode.MethodExit)
						.MethodCallElapsedMilliseconds(methodCallElapsedMilliseconds)
						.CommandQueryName(commandType?.FullName));
			}
		}

		public ICommandResult<bool> CanExecute<TResult>(ICommand<TResult> command, ICommandInterceptorOptions? options = default)
		{
			long callStartTicks = StaticWatch.CurrentTicks;
			long callEndTicks;
			decimal methodCallElapsedMilliseconds = -1;

			Type? commandType = command?.GetType();
			CommandProcessor<TResult>? commandProcessor = null;
			ICommandHandler? handler = null;
			var traceInfo = new TraceInfoBuilder(TraceFrame.Create(), _applicationContext.TraceInfo).Build();
			using var scope = _logger.BeginMethodCallScope(traceInfo);

			_logger.LogTraceMessage(
				traceInfo,
				x => x.LogCode(LogCode.MethodEntry)
						.CommandQueryName(commandType?.FullName));

			try
			{
				if (command == null || commandType == null)
					throw new ArgumentNullException(nameof(command));

				commandProcessor = (CommandProcessor<TResult>)_commandProcessors.GetOrAdd(commandType,
					t => CreateCommandProcessor(typeof(CommandProcessor<,>).MakeGenericType(commandType, typeof(TResult))));

				handler = commandProcessor.CreateHandler();
				handler.Dispatcher = this;
				var result = commandProcessor.CanExecute(traceInfo, handler, command, options);

				callEndTicks = StaticWatch.CurrentTicks;
				methodCallElapsedMilliseconds = StaticWatch.ElapsedMilliseconds(callStartTicks, callEndTicks);

				return result;
			}
			catch (Exception ex)
			{
				callEndTicks = StaticWatch.CurrentTicks;
				methodCallElapsedMilliseconds = StaticWatch.ElapsedMilliseconds(callStartTicks, callEndTicks);

				var errorMessage =
					_logger.LogErrorMessage(
						traceInfo,
						x => x.ExceptionInfo(ex)
							.Detail($"{nameof(CanExecute)}<{nameof(TResult)}> error - Command = {command?.GetType().FullName ?? "NULL"}")
							.LogCode((long)CommandLogCode.CommandDispatcherError)
							.ClientMessage(_applicationResources.GlobalExceptionMessage)
							.CommandQueryName(commandType?.FullName));

				var result = new CommandResultInternal<bool>();
				result.ErrorMessages.Add(errorMessage);
				return result;
			}
			finally
			{
				commandProcessor?.DisposeHandler(handler);

				_logger.LogDebugMessage(
					traceInfo,
					x => x.LogCode(LogCode.MethodExit)
						.MethodCallElapsedMilliseconds(methodCallElapsedMilliseconds)
						.CommandQueryName(commandType?.FullName));
			}
		}

		public async Task<ICommandResult<bool>> CanExecuteAsync<TResult>(ICommand<TResult> command, ICommandInterceptorOptions? options = default, CancellationToken cancellationToken = default)
		{
			long callStartTicks = StaticWatch.CurrentTicks;
			long callEndTicks;
			decimal methodCallElapsedMilliseconds = -1;

			Type? commandType = command?.GetType();
			AsyncCommandProcessor<TResult>? commandProcessor = null;
			ICommandHandler? handler = null;
			var traceInfo = new TraceInfoBuilder(TraceFrame.Create(), _applicationContext.TraceInfo).Build();
			using var scope = _logger.BeginMethodCallScope(traceInfo);

			_logger.LogTraceMessage(
				traceInfo,
				x => x.LogCode(LogCode.MethodEntry)
						.CommandQueryName(commandType?.FullName));

			try
			{
				if (command == null || commandType == null)
					throw new ArgumentNullException(nameof(command));

				commandProcessor = (AsyncCommandProcessor<TResult>)_asyncCommandProcessors.GetOrAdd(commandType,
					t => CreateCommandProcessor(typeof(AsyncCommandProcessor<,>).MakeGenericType(commandType, typeof(TResult))));

				handler = commandProcessor.CreateHandler();
				handler.Dispatcher = this;
				var result = await commandProcessor.CanExecuteAsync(traceInfo, handler, command, options, cancellationToken);

				callEndTicks = StaticWatch.CurrentTicks;
				methodCallElapsedMilliseconds = StaticWatch.ElapsedMilliseconds(callStartTicks, callEndTicks);

				return result;
			}
			catch (Exception ex)
			{
				callEndTicks = StaticWatch.CurrentTicks;
				methodCallElapsedMilliseconds = StaticWatch.ElapsedMilliseconds(callStartTicks, callEndTicks);

				var errorMessage =
					_logger.LogErrorMessage(
						traceInfo,
						x => x.ExceptionInfo(ex)
							.Detail($"{nameof(CanExecuteAsync)}<{nameof(TResult)}> error - Command = {command?.GetType().FullName ?? "NULL"}")
							.LogCode((long)CommandLogCode.CommandDispatcherError)
							.ClientMessage(_applicationResources.GlobalExceptionMessage)
							.CommandQueryName(commandType?.FullName));

				var result = new CommandResultInternal<bool>();
				result.ErrorMessages.Add(errorMessage);
				return result;
			}
			finally
			{
				commandProcessor?.DisposeHandler(handler);

				_logger.LogDebugMessage(
					traceInfo,
					x => x.LogCode(LogCode.MethodExit)
						.MethodCallElapsedMilliseconds(methodCallElapsedMilliseconds)
						.CommandQueryName(commandType?.FullName));
			}
		}

		public ICommandResult Execute(ICommand command, ICommandInterceptorOptions? options = default)
		{
			long callStartTicks = StaticWatch.CurrentTicks;
			long callEndTicks;
			decimal methodCallElapsedMilliseconds = -1;

			Type? commandType = command?.GetType();
			VoidCommandProcessor? commandProcessor = null;
			ICommandHandler? handler = null;
			var traceInfo = new TraceInfoBuilder(TraceFrame.Create(), _applicationContext.TraceInfo).Build();
			using var scope = _logger.BeginMethodCallScope(traceInfo);

			_logger.LogTraceMessage(
				traceInfo,
				x => x.LogCode(LogCode.MethodEntry)
						.CommandQueryName(commandType?.FullName));

			try
			{
				if (command == null || commandType == null)
					throw new ArgumentNullException(nameof(command));

				commandProcessor = (VoidCommandProcessor)_voidCommandProcessors.GetOrAdd(commandType,
					t => CreateCommandProcessor(typeof(VoidCommandProcessor<>).MakeGenericType(commandType)));

				handler = commandProcessor.CreateHandler();
				handler.Dispatcher = this;
				var result = commandProcessor.Execute(traceInfo, handler, command, options);

				callEndTicks = StaticWatch.CurrentTicks;
				methodCallElapsedMilliseconds = StaticWatch.ElapsedMilliseconds(callStartTicks, callEndTicks);

				return result;
			}
			catch (Exception ex)
			{
				callEndTicks = StaticWatch.CurrentTicks;
				methodCallElapsedMilliseconds = StaticWatch.ElapsedMilliseconds(callStartTicks, callEndTicks);

				var errorMessage =
					_logger.LogErrorMessage(
						traceInfo,
						x => x.ExceptionInfo(ex)
							.Detail($"{nameof(Execute)} error - Command = {command?.GetType().FullName ?? "NULL"}")
							.LogCode((long)CommandLogCode.CommandDispatcherError)
							.ClientMessage(_applicationResources.GlobalExceptionMessage)
							.CommandQueryName(commandType?.FullName));

				var result = new CommandResultInternal();
				result.ErrorMessages.Add(errorMessage);
				return result;
			}
			finally
			{
				commandProcessor?.DisposeHandler(handler);

				_logger.LogDebugMessage(
					traceInfo,
					x => x.LogCode(LogCode.MethodExit)
						.MethodCallElapsedMilliseconds(methodCallElapsedMilliseconds)
						.CommandQueryName(commandType?.FullName));
			}
		}

		public async Task<ICommandResult> ExecuteAsync(ICommand command, ICommandInterceptorOptions? options = default, CancellationToken cancellationToken = default)
		{
			long callStartTicks = StaticWatch.CurrentTicks;
			long callEndTicks;
			decimal methodCallElapsedMilliseconds = -1;

			Type? commandType = command?.GetType();
			AsyncVoidCommandProcessor? commandProcessor = null;
			ICommandHandler? handler = null;
			var traceInfo = new TraceInfoBuilder(TraceFrame.Create(), _applicationContext.TraceInfo).Build();
			using var scope = _logger.BeginMethodCallScope(traceInfo);

			_logger.LogTraceMessage(
				traceInfo,
				x => x.LogCode(LogCode.MethodEntry)
						.CommandQueryName(commandType?.FullName));

			try
			{
				if (command == null || commandType == null)
					throw new ArgumentNullException(nameof(command));

				commandProcessor = (AsyncVoidCommandProcessor)_asyncVoidCommandProcessors.GetOrAdd(commandType,
					t => CreateCommandProcessor(typeof(AsyncVoidCommandProcessor<>).MakeGenericType(commandType)));

				handler = commandProcessor.CreateHandler();
				handler.Dispatcher = this;
				var result = await commandProcessor.ExecuteAsync(traceInfo, handler, command, options, cancellationToken);

				callEndTicks = StaticWatch.CurrentTicks;
				methodCallElapsedMilliseconds = StaticWatch.ElapsedMilliseconds(callStartTicks, callEndTicks);

				return result;
			}
			catch (Exception ex)
			{
				callEndTicks = StaticWatch.CurrentTicks;
				methodCallElapsedMilliseconds = StaticWatch.ElapsedMilliseconds(callStartTicks, callEndTicks);

				var errorMessage =
					_logger.LogErrorMessage(
						traceInfo,
						x => x.ExceptionInfo(ex)
							.Detail($"{nameof(ExecuteAsync)} error - Command = {command?.GetType().FullName ?? "NULL"}")
							.LogCode((long)CommandLogCode.CommandDispatcherError)
							.ClientMessage(_applicationResources.GlobalExceptionMessage)
							.CommandQueryName(commandType?.FullName));

				var result = new CommandResultInternal();
				result.ErrorMessages.Add(errorMessage);
				return result;
			}
			finally
			{
				commandProcessor?.DisposeHandler(handler);

				_logger.LogDebugMessage(
					traceInfo,
					x => x.LogCode(LogCode.MethodExit)
						.MethodCallElapsedMilliseconds(methodCallElapsedMilliseconds)
						.CommandQueryName(commandType?.FullName));
			}
		}

		public ICommandResult<TResult> Execute<TResult>(ICommand<TResult> command, ICommandInterceptorOptions? options = default)
		{
			long callStartTicks = StaticWatch.CurrentTicks;
			long callEndTicks;
			decimal methodCallElapsedMilliseconds = -1;

			Type? commandType = command?.GetType();
			CommandProcessor<TResult>? commandProcessor = null;
			ICommandHandler? handler = null;
			var traceInfo = new TraceInfoBuilder(TraceFrame.Create(), _applicationContext.TraceInfo).Build();
			using var scope = _logger.BeginMethodCallScope(traceInfo);

			_logger.LogTraceMessage(
				traceInfo,
				x => x.LogCode(LogCode.MethodEntry)
						.CommandQueryName(commandType?.FullName));

			try
			{
				if (command == null || commandType == null)
					throw new ArgumentNullException(nameof(command));

				commandProcessor = (CommandProcessor<TResult>)_commandProcessors.GetOrAdd(commandType,
					t => CreateCommandProcessor(typeof(CommandProcessor<,>).MakeGenericType(commandType, typeof(TResult))));

				handler = commandProcessor.CreateHandler();
				handler.Dispatcher = this;
				var result = commandProcessor.Execute(traceInfo, handler, command, options);

				callEndTicks = StaticWatch.CurrentTicks;
				methodCallElapsedMilliseconds = StaticWatch.ElapsedMilliseconds(callStartTicks, callEndTicks);

				return result;
			}
			catch (Exception ex)
			{
				callEndTicks = StaticWatch.CurrentTicks;
				methodCallElapsedMilliseconds = StaticWatch.ElapsedMilliseconds(callStartTicks, callEndTicks);

				var errorMessage =
					_logger.LogErrorMessage(
						traceInfo,
						x => x.ExceptionInfo(ex)
							.Detail($"{nameof(Execute)}<{nameof(TResult)}> error - Command = {command?.GetType().FullName ?? "NULL"}")
							.LogCode((long)CommandLogCode.CommandDispatcherError)
							.ClientMessage(_applicationResources.GlobalExceptionMessage)
							.CommandQueryName(commandType?.FullName));

				var result = new CommandResultInternal<TResult>();
				result.ErrorMessages.Add(errorMessage);
				return result;
			}
			finally
			{
				commandProcessor?.DisposeHandler(handler);

				_logger.LogDebugMessage(
					traceInfo,
					x => x.LogCode(LogCode.MethodExit)
						.MethodCallElapsedMilliseconds(methodCallElapsedMilliseconds)
						.CommandQueryName(commandType?.FullName));
			}
		}

		public async Task<ICommandResult<TResult>> ExecuteAsync<TResult>(ICommand<TResult> command, ICommandInterceptorOptions? options = default, CancellationToken cancellationToken = default)
		{
			long callStartTicks = StaticWatch.CurrentTicks;
			long callEndTicks;
			decimal methodCallElapsedMilliseconds = -1;

			Type? commandType = command?.GetType();
			AsyncCommandProcessor<TResult>? commandProcessor = null;
			ICommandHandler? handler = null;
			var traceInfo = new TraceInfoBuilder(TraceFrame.Create(), _applicationContext.TraceInfo).Build();
			using var scope = _logger.BeginMethodCallScope(traceInfo);

			_logger.LogTraceMessage(
				traceInfo,
				x => x.LogCode(LogCode.MethodEntry)
						.CommandQueryName(commandType?.FullName));

			try
			{
				if (command == null || commandType == null)
					throw new ArgumentNullException(nameof(command));

				commandProcessor = (AsyncCommandProcessor<TResult>)_asyncCommandProcessors.GetOrAdd(commandType,
					t => CreateCommandProcessor(typeof(AsyncCommandProcessor<,>).MakeGenericType(commandType, typeof(TResult))));

				handler = commandProcessor.CreateHandler();
				handler.Dispatcher = this;
				var result = await commandProcessor.ExecuteAsync(traceInfo, handler, command, options, cancellationToken);

				callEndTicks = StaticWatch.CurrentTicks;
				methodCallElapsedMilliseconds = StaticWatch.ElapsedMilliseconds(callStartTicks, callEndTicks);

				return result;
			}
			catch (Exception ex)
			{
				callEndTicks = StaticWatch.CurrentTicks;
				methodCallElapsedMilliseconds = StaticWatch.ElapsedMilliseconds(callStartTicks, callEndTicks);

				var errorMessage =
					_logger.LogErrorMessage(
						traceInfo,
						x => x.ExceptionInfo(ex)
							.Detail($"{nameof(ExecuteAsync)}<{nameof(TResult)}> error - Command = {command?.GetType().FullName ?? "NULL"}")
							.LogCode((long)CommandLogCode.CommandDispatcherError)
							.ClientMessage(_applicationResources.GlobalExceptionMessage)
							.CommandQueryName(commandType?.FullName));

				var result = new CommandResultInternal<TResult>();
				result.ErrorMessages.Add(errorMessage);
				return result;
			}
			finally
			{
				commandProcessor?.DisposeHandler(handler);

				_logger.LogDebugMessage(
					traceInfo,
					x => x.LogCode(LogCode.MethodExit)
						.MethodCallElapsedMilliseconds(methodCallElapsedMilliseconds)
						.CommandQueryName(commandType?.FullName));
			}
		}

#pragma warning disable CS8603 // Possible null reference return.
		private CommandProcessorBase CreateCommandProcessor(Type type)
			=> Activator.CreateInstance(type, _handlerRegistry, _handlerFactory, _serviceFactory) as CommandProcessorBase;
#pragma warning restore CS8603 // Possible null reference return.

	}
}

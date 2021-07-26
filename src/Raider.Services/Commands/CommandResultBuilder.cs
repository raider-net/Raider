using Microsoft.Extensions.Logging;
using Raider.Commands;
using Raider.Extensions;
using Raider.Logging;
using Raider.Trace;
using Raider.Validation;
using System;

namespace Raider.Services.Commands
{
	public interface ICommandResultBuilder<TBuilder, TObject>
		where TBuilder : ICommandResultBuilder<TBuilder, TObject>
		where TObject : ICommandResult
	{
		TBuilder Object(TObject commandResult);
		TObject Build();

		bool MergeHasError(ICommandResult otherCommandResult);

		bool MergeAllHasError(ICommandResult otherCommandResult);


		bool MergeHasError(MethodLogScope scope, ValidationResult validationResult);

		bool MergeHasError(ITraceInfo traceInfo, ValidationResult validationResult);

		bool HasError();

		object? GetResult();

		TBuilder AddAffectedEntities(long count);

		TBuilder AddAffectedEntities(params ICommandResult[] commandResults);

		TBuilder ClearAllSuccessMessages();

		TBuilder WithSuccess(ILogMessage message);

		TBuilder WithWarn(ILogMessage message);

		TBuilder WithError(IErrorMessage message);

		TBuilder WithSuccess(MethodLogScope scope, Action<LogMessageBuilder>? logMessageConfigurator);

		TBuilder WithSuccess(ITraceInfo traceInfo, Action<LogMessageBuilder>? logMessageConfigurator);

		TBuilder WithWarn(MethodLogScope scope, Action<LogMessageBuilder>? logMessageConfigurator);

		TBuilder WithWarn(ITraceInfo traceInfo, Action<LogMessageBuilder>? logMessageConfigurator);

		TBuilder WithError(MethodLogScope scope, Action<ErrorMessageBuilder>? logMessageConfigurator);

		TBuilder WithError(ITraceInfo traceInfo, Action<ErrorMessageBuilder>? logMessageConfigurator);

		TBuilder ForAllSuccessMessages(Action<ILogMessage> logMessageConfigurator);

		TBuilder ForAllWarningMessages(Action<ILogMessage> logMessageConfigurator);

		TBuilder ForAllIErrorMessages(Action<ILogMessage> errorMessageConfigurator);

		TBuilder ForAllMessages(Action<ILogMessage> messageConfigurator);

		TBuilder Merge(ICommandResult otherCommandResult);
	}

	public abstract class CommandResultBuilderBase<TBuilder, TObject> : ICommandResultBuilder<TBuilder, TObject>
		where TBuilder : CommandResultBuilderBase<TBuilder, TObject>
		where TObject : ICommandResult
	{
		protected readonly TBuilder _builder;
		protected TObject _commandResult;

		protected CommandResultBuilderBase(TObject commandResult)
		{
			_commandResult = commandResult;
			_builder = (TBuilder)this;
		}

		public virtual TBuilder Object(TObject commandResult)
		{
			_commandResult = commandResult ?? throw new ArgumentNullException(nameof(commandResult));

			return _builder;
		}

		public TObject Build()
			=> _commandResult;

		public TBuilder AddAffectedEntities(long count)
		{
			if (!_commandResult.AffectedEntities.HasValue)
			{
				_commandResult.AffectedEntities = 0;
			}
			_commandResult.AffectedEntities += count;

			return _builder;
		}

		public TBuilder AddAffectedEntities(params ICommandResult[] commandResults)
		{
			if (commandResults != null)
			{
				if (!_commandResult.AffectedEntities.HasValue)
				{
					_commandResult.AffectedEntities = 0;
				}

				foreach (var cr in commandResults)
				{
					if (cr.AffectedEntities.HasValue)
					{
						_commandResult.AffectedEntities += cr.AffectedEntities.Value;
					}
				}
			}
			return _builder;
		}

		#region API

		public bool MergeHasError(ICommandResult otherCommandResult)
		{
			if (otherCommandResult != null && otherCommandResult.HasError)
				_commandResult.ErrorMessages.AddRange(otherCommandResult.ErrorMessages);

			return _commandResult.HasError;
		}

		public bool MergeAllHasError(ICommandResult otherCommandResult)
		{
			if (otherCommandResult != null)
			{
				if (otherCommandResult.HasSuccessMessage)
					_commandResult.SuccessMessages.AddRange(otherCommandResult.SuccessMessages);

				if (otherCommandResult.HasWarning)
					_commandResult.WarningMessages.AddRange(otherCommandResult.WarningMessages);

				if (otherCommandResult.HasError)
					_commandResult.ErrorMessages.AddRange(otherCommandResult.ErrorMessages);

				AddAffectedEntities(otherCommandResult.AffectedEntities ?? 0);
			}

			return _commandResult.HasError;
		}

		public bool MergeHasError(MethodLogScope scope, ValidationResult validationResult)
		{
			if (scope == null)
				throw new ArgumentNullException(nameof(scope));

			return MergeHasError(scope.TraceInfo, validationResult);
		}

		public bool MergeHasError(ITraceInfo traceInfo, ValidationResult validationResult)
		{
			if (traceInfo == null)
				throw new ArgumentNullException(nameof(traceInfo));
			if (validationResult == null)
				throw new ArgumentNullException(nameof(validationResult));

			foreach (var failure in validationResult.Errors)
			{
				if (failure.Severity == ValidationSeverity.Error)
				{
					var errorMessage = CommandResultBuilderBase<TBuilder, TObject>.ValidationFailureToErrorMessage(traceInfo, failure);
					_commandResult.ErrorMessages.Add(errorMessage);
				}
				else
				{
					var warnigMessage = CommandResultBuilderBase<TBuilder, TObject>.ValidationFailureToWarningMessage(traceInfo, failure);
					_commandResult.WarningMessages.Add(warnigMessage);
				}
			}

			return _commandResult.HasError;
		}

		private static IErrorMessage ValidationFailureToErrorMessage(ITraceInfo traceInfo, IValidationFailure failure)
		{
			if (failure == null)
				throw new ArgumentNullException(nameof(failure));

			var errorMessageBuilder =
				new ErrorMessageBuilder(traceInfo)
					.LogLevel( LogLevel.Error)
					.ValidationFailure(failure, true)
					.ClientMessage(failure.Message, true) //TODO read from settings when to use MessageWithPropertyName
					.PropertyName(failure.ValidationFrame.ToString()?.TrimPrefix("_."), true);

			return errorMessageBuilder.Build();
		}

		private static ILogMessage ValidationFailureToWarningMessage(ITraceInfo traceInfo, IValidationFailure failure)
		{
			if (failure == null)
				throw new ArgumentNullException(nameof(failure));

			var logMessageBuilder =
				new LogMessageBuilder(traceInfo)
					.LogLevel(LogLevel.Warning)
					.ValidationFailure(failure, true)
					.ClientMessage(failure.Message, true) //TODO read from settings when to use MessageWithPropertyName
					.PropertyName(failure.ValidationFrame.ToString()?.TrimPrefix("_."), true);

			return logMessageBuilder.Build();
		}

		public bool HasError()
		{
			return _commandResult.HasError;
		}

		public virtual object? GetResult()
		{
			return null;
		}

		public TBuilder ClearAllSuccessMessages()
		{
			_commandResult.SuccessMessages.Clear();
			return _builder;
		}

		public TBuilder WithSuccess(ILogMessage message)
		{
			_commandResult.SuccessMessages.Add(message);
			return _builder;
		}

		public TBuilder WithWarn(ILogMessage message)
		{
			_commandResult.WarningMessages.Add(message);
			return _builder;
		}

		public TBuilder WithError(IErrorMessage message)
		{
			_commandResult.ErrorMessages.Add(message);
			return _builder;
		}

		public TBuilder WithSuccess(MethodLogScope scope, Action<LogMessageBuilder>? logMessageConfigurator)
			=> WithSuccess(scope?.TraceInfo!, logMessageConfigurator);

		public TBuilder WithSuccess(ITraceInfo traceInfo, Action<LogMessageBuilder>? logMessageConfigurator)
		{
			var logMessageBuilder =
				new LogMessageBuilder(traceInfo)
					.LogLevel(LogLevel.Information);
			logMessageConfigurator?.Invoke(logMessageBuilder);
			_commandResult.SuccessMessages.Add(logMessageBuilder.Build());
			return _builder;
		}

		public TBuilder WithWarn(MethodLogScope scope, Action<LogMessageBuilder>? logMessageConfigurator)
			=> WithWarn(scope?.TraceInfo!, logMessageConfigurator);

		public TBuilder WithWarn(ITraceInfo traceInfo, Action<LogMessageBuilder>? logMessageConfigurator)
		{
			var logMessageBuilder =
				new LogMessageBuilder(traceInfo)
					.LogLevel(LogLevel.Warning);
			logMessageConfigurator?.Invoke(logMessageBuilder);
			_commandResult.WarningMessages.Add(logMessageBuilder.Build());
			return _builder;
		}

		public TBuilder WithError(MethodLogScope scope, Action<ErrorMessageBuilder>? errorMessageConfigurator)
			=> WithError(scope?.TraceInfo!, errorMessageConfigurator);

		public TBuilder WithError(ITraceInfo traceInfo, Action<ErrorMessageBuilder>? errorMessageConfigurator)
		{
			var errorMessageBuilder =
				new ErrorMessageBuilder(traceInfo)
					.LogLevel(LogLevel.Error);
			errorMessageConfigurator?.Invoke(errorMessageBuilder);
			_commandResult.ErrorMessages.Add(errorMessageBuilder.Build());
			return _builder;
		}

		public TBuilder ForAllSuccessMessages(Action<ILogMessage> logMessageConfigurator)
		{
			if (logMessageConfigurator == null)
				throw new ArgumentNullException(nameof(logMessageConfigurator));

			foreach (var successMessage in _commandResult.SuccessMessages)
				logMessageConfigurator.Invoke(successMessage);

			return _builder;
		}

		public TBuilder ForAllWarningMessages(Action<ILogMessage> logMessageConfigurator)
		{
			if (logMessageConfigurator == null)
				throw new ArgumentNullException(nameof(logMessageConfigurator));

			foreach (var warningMessage in _commandResult.WarningMessages)
				logMessageConfigurator.Invoke(warningMessage);

			return _builder;
		}

		public TBuilder ForAllIErrorMessages(Action<ILogMessage> errorMessageConfigurator)
		{
			if (errorMessageConfigurator == null)
				throw new ArgumentNullException(nameof(errorMessageConfigurator));

			foreach (var errorMessage in _commandResult.ErrorMessages)
				errorMessageConfigurator.Invoke(errorMessage);

			return _builder;
		}

		public TBuilder ForAllMessages(Action<ILogMessage> messageConfigurator)
			=> ForAllSuccessMessages(messageConfigurator)
				.ForAllWarningMessages(messageConfigurator)
				.ForAllIErrorMessages(messageConfigurator);

		public TBuilder Merge(ICommandResult otherCommandResult)
		{
			if (otherCommandResult != null)
			{
				if (otherCommandResult.HasError)
					_commandResult.ErrorMessages.AddRange(otherCommandResult.ErrorMessages);
				if (otherCommandResult.HasWarning)
					_commandResult.WarningMessages.AddRange(otherCommandResult.WarningMessages);
				if (otherCommandResult.HasSuccessMessage)
					_commandResult.SuccessMessages.AddRange(otherCommandResult.SuccessMessages);
			}

			return _builder;
		}

		#endregion API
	}

	public class CommandResultBuilder : CommandResultBuilderBase<CommandResultBuilder, CommandResult>
	{
		public CommandResultBuilder()
			: this(new CommandResult())
		{
		}

		public CommandResultBuilder(CommandResult commandResult)
			: base(commandResult)
		{
		}

		public static implicit operator CommandResult?(CommandResultBuilder builder)
		{
			if (builder == null)
				return null;

			return builder._commandResult;
		}

		public static implicit operator CommandResultBuilder?(CommandResult commandResult)
		{
			if (commandResult == null)
				return null;

			return new CommandResultBuilder(commandResult);
		}

		public static ICommandResult Empty()
			=> new CommandResultBuilder().Build();
	}

	public interface ICommandResultBuilder<TBuilder, T, TObject> : ICommandResultBuilder<TBuilder, TObject>
		where TBuilder : ICommandResultBuilder<TBuilder, T, TObject>
		where TObject : ICommandResult<T>
	{
		TBuilder WithResult(T? data);

		TBuilder ClearResult();

		bool MergeAllHasError(ICommandResult<T> otherCommandResult);
	}

	public abstract class CommandResultBuilderBase<TBuilder, T, TObject> : CommandResultBuilderBase<TBuilder, TObject>, ICommandResultBuilder<TBuilder, T, TObject>
		where TBuilder : CommandResultBuilderBase<TBuilder, T, TObject>
		where TObject : ICommandResult<T>
	{
		protected CommandResultBuilderBase(TObject commandResult)
			: base(commandResult)
		{
		}

		public override object? GetResult()
		{
			return _commandResult.Result;
		}

		public TBuilder WithResult(T? result)
		{
			_commandResult.Result = result;
			return _builder;
		}

		public TBuilder ClearResult()
		{
			_commandResult.ClearResult();
			return _builder;
		}

		public bool MergeAllHasError(ICommandResult<T> otherCommandResult)
		{
			if (otherCommandResult != null)
			{
				if (otherCommandResult.HasSuccessMessage)
					_commandResult.SuccessMessages.AddRange(otherCommandResult.SuccessMessages);

				if (otherCommandResult.HasWarning)
					_commandResult.WarningMessages.AddRange(otherCommandResult.WarningMessages);

				if (otherCommandResult.HasError)
					_commandResult.ErrorMessages.AddRange(otherCommandResult.ErrorMessages);

				AddAffectedEntities(otherCommandResult.AffectedEntities ?? 0);

				_commandResult.Result = otherCommandResult.Result;
			}

			return _commandResult.HasError;
		}
	}

	public class CommandResultBuilder<T> : CommandResultBuilderBase<CommandResultBuilder<T>, T, ICommandResult<T>>
	{
		public CommandResultBuilder()
			: this(new CommandResult<T>())
		{
		}

		public CommandResultBuilder(CommandResult<T> commandResult)
			: base(commandResult)
		{
		}

		public static implicit operator CommandResult<T>?(CommandResultBuilder<T> builder)
		{
			if (builder == null)
				return null;

			return builder._commandResult as CommandResult<T>;
		}

		public static implicit operator CommandResultBuilder<T>?(CommandResult<T> commandResult)
		{
			if (commandResult == null)
				return null;

			return new CommandResultBuilder<T>(commandResult);
		}

		public static implicit operator CommandResultBuilder?(CommandResultBuilder<T> builder)
		{
			if (builder == null)
				return null;

			return new CommandResultBuilder((CommandResult<T>)builder._commandResult);
		}

		public static ICommandResult<T> Empty()
			=> new CommandResultBuilder<T>().Build();

		public static ICommandResult<T> FromResult(T result)
			=> new CommandResultBuilder<T>().WithResult(result).Build();
	}
}

using Microsoft.Extensions.Logging;
using Raider.Commands;
using Raider.Logging;
using Raider.Trace;
using System;
using System.Collections.Generic;

namespace Raider.Services.Commands
{
	public interface ICommandResultBuilder<TBuilder, TObject>
		where TBuilder : ICommandResultBuilder<TBuilder, TObject>
		where TObject : ICommandResult
	{
		TBuilder Object(TObject commandResult);
		TObject Build();

		bool MergeHasError(ICommandResult otherCommandResult);

		//bool MergeIfError(string error);

		bool HasError();

		object? GetResult();

		TBuilder AddAffectedEntities(long count);

		TBuilder AddAffectedEntities(params ICommandResult[] commandResults);

		TBuilder ClearAllSuccessMessages();

		//TBuilder WithSuccess(string message, Action<ILogMessage>? logMessageConfigurator = null);

		//TBuilder WithWarn(string message, Action<ILogMessage>? logMessageConfigurator = null);

		//TBuilder WithError(string message, Action<IErrorMessage>? errorMessageConfigurator = null);

		//TBuilder WithError(Exception ex, Action<IErrorMessage>? errorMessageConfigurator = null);

		TBuilder WithSuccess(ILogMessage message);

		TBuilder WithWarn(ILogMessage message);

		TBuilder WithError(IErrorMessage message);

		TBuilder WithSuccess(ITraceInfo traceInfo, Action<LogMessageBuilder>? logMessageConfigurator);

		TBuilder WithWarn(ITraceInfo traceInfo, Action<LogMessageBuilder>? logMessageConfigurator);

		TBuilder WithError(ITraceInfo traceInfo, Action<ErrorMessageBuilder>? logMessageConfigurator);

		//TBuilder WithTraceInfo(ITraceInfo traceInfo, bool force = false);

		TBuilder ForAllSuccessMessages(Action<ILogMessage> logMessageConfigurator);

		TBuilder ForAllWarningMessages(Action<ILogMessage> logMessageConfigurator);

		TBuilder ForAllIErrorMessages(Action<ILogMessage> errorMessageConfigurator);

		TBuilder ForAllMessages(Action<ILogMessage> messageConfigurator);

		//TBuilder MergeAllIErrorMessages(string? separator = null);

		//TBuilder MergeAllWarningMessages(string? separator = null);

		//TBuilder MergeAllSuccessMessages(string? separator = null);

		//TBuilder MergeAllMessages(string? separator = null);

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

		//public bool MergeIfError(string error)
		//{
		//	if (!string.IsNullOrWhiteSpace(error))
		//		WithError(error);

		//	return _commandResult.HasError;
		//}

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

		//public TBuilder WithSuccess(string message, Action<ILogMessage>? logMessageConfigurator = null)
		//{
		//	var logMessage =
		//		new LogMessageBuilder(_commandResult.TraceInfo)
		//			.LogLevel(LogLevel.Information)
		//			.Message(message)
		//			.StackTrace(StackTraceHelper.GetStackTrace(true))
		//			.Build();
		//	logMessageConfigurator?.Invoke(logMessage);
		//	_commandResult.SuccessMessages.Add(logMessage);
		//	return _builder;
		//}

		//public TBuilder WithWarn(string message, Action<ILogMessage>? logMessageConfigurator = null)
		//{
		//	var logMessage =
		//		new LogMessageBuilder(_commandResult.TraceInfo)
		//			.LogLevel(LogLevel.Warning)
		//			.Message(message)
		//			.StackTrace(StackTraceHelper.GetStackTrace(true))
		//			.Build();
		//	logMessageConfigurator?.Invoke(logMessage);
		//	_commandResult.WarningMessages.Add(logMessage);
		//	return _builder;
		//}

		//public TBuilder WithError(string message, Action<IErrorMessage>? errorMessageConfigurator = null)
		//{
		//	var errorMessage = new LogMessageBuilder(_commandResult.TraceInfo)
		//		.LogLevel(LogLevel.Error)
		//		.Message(message)
		//		.StackTrace(StackTraceHelper.GetStackTrace(true))
		//		.BuildErrorMessage();
		//	errorMessageConfigurator?.Invoke(errorMessage);
		//	_commandResult.ErrorMessages.Add(errorMessage);
		//	return _builder;
		//}

		//public TBuilder WithError(Exception ex, Action<IErrorMessage>? errorMessageConfigurator = null)
		//{
		//	var errorMessage = new LogMessageBuilder(_commandResult.TraceInfo)
		//		.LogLevel(LogLevel.Error)
		//		.ExceptionInfo(ex)
		//		.StackTrace(StackTraceHelper.GetStackTrace(true))
		//		.BuildErrorMessage();
		//	errorMessageConfigurator?.Invoke(errorMessage);
		//	_commandResult.ErrorMessages.Add(errorMessage);
		//	return _builder;
		//}

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

		public TBuilder WithSuccess(ITraceInfo traceInfo, Action<LogMessageBuilder>? logMessageConfigurator)
		{
			var logMessageBuilder =
				new LogMessageBuilder(traceInfo)
					.LogLevel(LogLevel.Information);
			logMessageConfigurator?.Invoke(logMessageBuilder);
			_commandResult.SuccessMessages.Add(logMessageBuilder.Build());
			return _builder;
		}

		public TBuilder WithWarn(ITraceInfo traceInfo, Action<LogMessageBuilder>? logMessageConfigurator)
		{
			var logMessageBuilder =
				new LogMessageBuilder(traceInfo)
					.LogLevel(LogLevel.Warning);
			logMessageConfigurator?.Invoke(logMessageBuilder);
			_commandResult.WarningMessages.Add(logMessageBuilder.Build());
			return _builder;
		}

		public TBuilder WithError(ITraceInfo traceInfo, Action<ErrorMessageBuilder>? errorMessageConfigurator)
		{
			var errorMessageBuilder =
				new ErrorMessageBuilder(traceInfo)
					.LogLevel(LogLevel.Error);
			errorMessageConfigurator?.Invoke(errorMessageBuilder);
			_commandResult.ErrorMessages.Add(errorMessageBuilder.Build());
			return _builder;
		}

		//public TBuilder WithTraceInfo(ITraceInfo traceInfo, bool force = false)
		//{
		//	_commandResult.TraceInfo = traceInfo;

		//	var builder = new LogMessageBuilder(_commandResult.TraceInfo);
		//	void SetTraceInfo(ILogMessage msg) => builder.Object(msg).TraceInfo(traceInfo, force);

		//	_commandResult.SuccessMessages.ForEach(x => SetTraceInfo(x));
		//	_commandResult.WarningMessages.ForEach(x => SetTraceInfo(x));
		//	_commandResult.ErrorMessages.ForEach(x => SetTraceInfo(x));

		//	return _builder;
		//}

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

		//public TBuilder MergeAllIErrorMessages(string? separator = null)
		//{
		//	if (1 < _commandResult.ErrorMessages.Count)
		//	{
		//		if (separator == null)
		//			separator = Environment.NewLine;

		//		var errorMessage = _commandResult.ErrorMessages[0];

		//		var joinedMessages = string.Join(separator, _commandResult.ErrorMessages.Select(x => x.Message));
		//		var joinedDetails = string.Join(separator, _commandResult.ErrorMessages.Select(x => x.Detail));

		//		new LogMessageBuilder(errorMessage).LogLevel(LogLevel.Error)
		//			.Message(joinedMessages)
		//			.Detail(joinedDetails);

		//		_commandResult.ErrorMessages.Clear();
		//		_commandResult.ErrorMessages.Add(errorMessage);
		//	}

		//	return _builder;
		//}

		//public TBuilder MergeAllWarningMessages(string? separator = null)
		//{
		//	if (1 < _commandResult.WarningMessages.Count)
		//	{
		//		if (separator == null)
		//			separator = Environment.NewLine;

		//		var warningMessage = new LogMessageBuilder(_commandResult.WarningMessages[0]);
		//		var joinedMessages = string.Join(separator, _commandResult.WarningMessages.Select(x => x.Message));
		//		var joinedDetails = string.Join(separator, _commandResult.WarningMessages.Select(x => x.Detail));
		//		warningMessage.Message(joinedMessages);
		//		warningMessage.Detail(joinedDetails);
		//		_commandResult.WarningMessages.Clear();
		//		_commandResult.WarningMessages.Add(warningMessage.Build());
		//	}

		//	return _builder;
		//}

		//public TBuilder MergeAllSuccessMessages(string? separator = null)
		//{
		//	if (1 < _commandResult.SuccessMessages.Count)
		//	{
		//		if (separator == null)
		//			separator = Environment.NewLine;

		//		var successMessage = new LogMessageBuilder(_commandResult.SuccessMessages[0]);
		//		var joinedMessages = string.Join(separator, _commandResult.SuccessMessages.Select(x => x.Message));
		//		var joinedDetails = string.Join(separator, _commandResult.SuccessMessages.Select(x => x.Detail));
		//		successMessage.Message(joinedMessages);
		//		successMessage.Detail(joinedDetails);
		//		_commandResult.SuccessMessages.Clear();
		//		_commandResult.SuccessMessages.Add(successMessage.Build());
		//	}

		//	return _builder;
		//}

		//public TBuilder MergeAllMessages(string? separator = null)
		//{
		//	MergeAllIErrorMessages(separator);
		//	MergeAllWarningMessages(separator);
		//	MergeAllSuccessMessages(separator);
		//	return _builder;
		//}

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

	public class CommandResultBuilder : CommandResultBuilderBase<CommandResultBuilder, ICommandResult>
	{
		private class CommandResult : ICommandResult
		{
			//public ITraceInfo TraceInfo { get; set; }

			public List<ILogMessage> SuccessMessages { get; }

			public List<ILogMessage> WarningMessages { get; }

			public List<IErrorMessage> ErrorMessages { get; }

			public bool HasSuccessMessage => 0 < SuccessMessages.Count;

			public bool HasWarning => 0 < WarningMessages.Count;

			public bool HasError => 0 < ErrorMessages.Count;

			public bool HasAnyMessage => HasSuccessMessage || HasWarning || HasError;

			public long? AffectedEntities { get; set; }

			public CommandResult()
			{
				//TraceInfo = traceInfo;
				SuccessMessages = new List<ILogMessage>();
				WarningMessages = new List<ILogMessage>();
				ErrorMessages = new List<IErrorMessage>();
			}
		}

		public CommandResultBuilder()
			: this(new CommandResult())
		{
		}

		public CommandResultBuilder(ICommandResult commandResult)
			: base(commandResult)
		{
		}

		//public static implicit operator CommandResult?(CommandResultBuilder builder)
		//{
		//	if (builder == null)
		//		return null;

		//	return builder._commandResult;
		//}

		//public static implicit operator CommandResultBuilder?(CommandResult commandResult)
		//{
		//	if (commandResult == null)
		//		return null;

		//	return new CommandResultBuilder(commandResult);
		//}
	}

	public interface ICommandResultBuilder<TBuilder, T, TObject> : ICommandResultBuilder<TBuilder, TObject>
		where TBuilder : ICommandResultBuilder<TBuilder, T, TObject>
		where TObject : ICommandResult<T>
	{
		TBuilder WithResult(T data);
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

		public TBuilder WithResult(T result)
		{
			_commandResult.Result = result;
			return _builder;
		}
	}

	public class CommandResultBuilder<T> : CommandResultBuilderBase<CommandResultBuilder<T>, T, ICommandResult<T>>
	{
		//TODO inherit from "private/internal" CommandResult
		private class CommandResultGeneric : /* //TODO : CommandResult*/ ICommandResult<T>
		{
			//public ITraceInfo TraceInfo { get; set; }

			public List<ILogMessage> SuccessMessages { get; }

			public List<ILogMessage> WarningMessages { get; }

			public List<IErrorMessage> ErrorMessages { get; }

			public bool HasSuccessMessage => 0 < SuccessMessages.Count;

			public bool HasWarning => 0 < WarningMessages.Count;

			public bool HasError => 0 < ErrorMessages.Count;

			public bool HasAnyMessage => HasSuccessMessage || HasWarning || HasError;

			public long? AffectedEntities { get; set; }


			public T? Result { get; set; }


			public CommandResultGeneric()
			{
				//TraceInfo = traceInfo;
				SuccessMessages = new List<ILogMessage>();
				WarningMessages = new List<ILogMessage>();
				ErrorMessages = new List<IErrorMessage>();
			}
		}

		public CommandResultBuilder()
			: this(new CommandResultGeneric())
		{
		}

		public CommandResultBuilder(ICommandResult<T> commandResult)
			: base(commandResult)
		{
		}

		//public static implicit operator CommandResult<T>?(CommandResultBuilder<T> builder)
		//{
		//	if (builder == null)
		//		return null;

		//	return builder._commandResult;
		//}

		//public static implicit operator CommandResultBuilder<T>?(CommandResult<T> commandResult)
		//{
		//	if (commandResult == null)
		//		return null;

		//	return new CommandResultBuilder<T>(commandResult);
		//}

		//public static implicit operator CommandResultBuilder?(CommandResultBuilder<T> builder)
		//{
		//	if (builder == null)
		//		return null;

		//	return new CommandResultBuilder(builder._commandResult);
		//}
	}
}

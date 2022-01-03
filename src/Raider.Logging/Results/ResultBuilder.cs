using Microsoft.Extensions.Logging;
using Raider.Extensions;
using Raider.Logging;
using Raider.Trace;
using Raider.Validation;
using System;

namespace Raider
{
	public interface IResultBuilder<TBuilder, TObject>
		where TBuilder : IResultBuilder<TBuilder, TObject>
		where TObject : IResult
	{
		TBuilder Object(TObject Result);
		TObject Build();

		bool MergeHasError(IResult otherResult);

		bool MergeAllHasError(IResult otherResult);


		bool MergeHasError(MethodLogScope scope, IValidationResult validationResult);

		bool MergeHasError(ITraceInfo traceInfo, IValidationResult validationResult);

		bool HasError();

		object? GetResult();

		TBuilder AddAffectedEntities(long count);

		TBuilder AddAffectedEntities(params IResult[] Results);

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

		TBuilder Merge(IResult otherResult);
	}

	public abstract class ResultBuilderBase<TBuilder, TObject> : IResultBuilder<TBuilder, TObject>
		where TBuilder : ResultBuilderBase<TBuilder, TObject>
		where TObject : IResult
	{
		protected readonly TBuilder _builder;
		protected TObject _result;

		protected ResultBuilderBase(TObject Result)
		{
			_result = Result;
			_builder = (TBuilder)this;
		}

		public virtual TBuilder Object(TObject Result)
		{
			_result = Result ?? throw new ArgumentNullException(nameof(Result));

			return _builder;
		}

		public TObject Build()
			=> _result;

		public TBuilder AddAffectedEntities(long count)
		{
			if (!_result.AffectedEntities.HasValue)
			{
				_result.AffectedEntities = 0;
			}
			_result.AffectedEntities += count;

			return _builder;
		}

		public TBuilder AddAffectedEntities(params IResult[] Results)
		{
			if (Results != null)
			{
				if (!_result.AffectedEntities.HasValue)
				{
					_result.AffectedEntities = 0;
				}

				foreach (var cr in Results)
				{
					if (cr.AffectedEntities.HasValue)
					{
						_result.AffectedEntities += cr.AffectedEntities.Value;
					}
				}
			}
			return _builder;
		}

		#region API

		public bool MergeHasError(IResult otherResult)
		{
			if (otherResult != null && otherResult.HasError)
				_result.ErrorMessages.AddRange(otherResult.ErrorMessages);

			return _result.HasError;
		}

		public bool MergeAllHasError(IResult otherResult)
		{
			if (otherResult != null)
			{
				if (otherResult.HasSuccessMessage)
					_result.SuccessMessages.AddRange(otherResult.SuccessMessages);

				if (otherResult.HasWarning)
					_result.WarningMessages.AddRange(otherResult.WarningMessages);

				if (otherResult.HasError)
					_result.ErrorMessages.AddRange(otherResult.ErrorMessages);

				AddAffectedEntities(otherResult.AffectedEntities ?? 0);
			}

			return _result.HasError;
		}

		public bool MergeHasError(MethodLogScope scope, IValidationResult validationResult)
		{
			if (scope == null)
				throw new ArgumentNullException(nameof(scope));

			return MergeHasError(scope.TraceInfo, validationResult);
		}

		public bool MergeHasError(ITraceInfo traceInfo, IValidationResult validationResult)
		{
			if (traceInfo == null)
				throw new ArgumentNullException(nameof(traceInfo));
			if (validationResult == null)
				throw new ArgumentNullException(nameof(validationResult));

			foreach (var failure in validationResult.Errors)
			{
				if (failure.Severity == ValidationSeverity.Error)
				{
					var errorMessage = ResultBuilderBase<TBuilder, TObject>.ValidationFailureToErrorMessage(traceInfo, failure);
					_result.ErrorMessages.Add(errorMessage);
				}
				else
				{
					var warnigMessage = ResultBuilderBase<TBuilder, TObject>.ValidationFailureToWarningMessage(traceInfo, failure);
					_result.WarningMessages.Add(warnigMessage);
				}
			}

			return _result.HasError;
		}

		private static IErrorMessage ValidationFailureToErrorMessage(ITraceInfo traceInfo, IBaseValidationFailure failure)
		{
			if (failure == null)
				throw new ArgumentNullException(nameof(failure));

			var errorMessageBuilder =
				new ErrorMessageBuilder(traceInfo)
					.LogLevel(LogLevel.Error)
					.ValidationFailure(failure, true)
					.ClientMessage(failure.Message, true) //TODO read from settings when to use MessageWithPropertyName
					.PropertyName(string.IsNullOrWhiteSpace(failure.ValidationFrame.PropertyName) ? null : failure.ValidationFrame.ToString()?.TrimPrefix("_."), !string.IsNullOrWhiteSpace(failure.ValidationFrame.PropertyName));

			return errorMessageBuilder.Build();
		}

		private static ILogMessage ValidationFailureToWarningMessage(ITraceInfo traceInfo, IBaseValidationFailure failure)
		{
			if (failure == null)
				throw new ArgumentNullException(nameof(failure));

			var logMessageBuilder =
				new LogMessageBuilder(traceInfo)
					.LogLevel(LogLevel.Warning)
					.ValidationFailure(failure, true)
					.ClientMessage(failure.Message, true) //TODO read from settings when to use MessageWithPropertyName
					.PropertyName(string.IsNullOrWhiteSpace(failure.ValidationFrame.PropertyName) ? null : failure.ValidationFrame.ToString()?.TrimPrefix("_."), !string.IsNullOrWhiteSpace(failure.ValidationFrame.PropertyName));

			return logMessageBuilder.Build();
		}

		public bool HasError()
		{
			return _result.HasError;
		}

		public virtual object? GetResult()
		{
			return null;
		}

		public TBuilder ClearAllSuccessMessages()
		{
			_result.SuccessMessages.Clear();
			return _builder;
		}

		public TBuilder WithSuccess(ILogMessage message)
		{
			_result.SuccessMessages.Add(message);
			return _builder;
		}

		public TBuilder WithWarn(ILogMessage message)
		{
			_result.WarningMessages.Add(message);
			return _builder;
		}

		public TBuilder WithError(IErrorMessage message)
		{
			_result.ErrorMessages.Add(message);
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
			_result.SuccessMessages.Add(logMessageBuilder.Build());
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
			_result.WarningMessages.Add(logMessageBuilder.Build());
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
			_result.ErrorMessages.Add(errorMessageBuilder.Build());
			return _builder;
		}

		public TBuilder ForAllSuccessMessages(Action<ILogMessage> logMessageConfigurator)
		{
			if (logMessageConfigurator == null)
				throw new ArgumentNullException(nameof(logMessageConfigurator));

			foreach (var successMessage in _result.SuccessMessages)
				logMessageConfigurator.Invoke(successMessage);

			return _builder;
		}

		public TBuilder ForAllWarningMessages(Action<ILogMessage> logMessageConfigurator)
		{
			if (logMessageConfigurator == null)
				throw new ArgumentNullException(nameof(logMessageConfigurator));

			foreach (var warningMessage in _result.WarningMessages)
				logMessageConfigurator.Invoke(warningMessage);

			return _builder;
		}

		public TBuilder ForAllIErrorMessages(Action<ILogMessage> errorMessageConfigurator)
		{
			if (errorMessageConfigurator == null)
				throw new ArgumentNullException(nameof(errorMessageConfigurator));

			foreach (var errorMessage in _result.ErrorMessages)
				errorMessageConfigurator.Invoke(errorMessage);

			return _builder;
		}

		public TBuilder ForAllMessages(Action<ILogMessage> messageConfigurator)
			=> ForAllSuccessMessages(messageConfigurator)
				.ForAllWarningMessages(messageConfigurator)
				.ForAllIErrorMessages(messageConfigurator);

		public TBuilder Merge(IResult otherResult)
		{
			if (otherResult != null)
			{
				if (otherResult.HasError)
					_result.ErrorMessages.AddRange(otherResult.ErrorMessages);
				if (otherResult.HasWarning)
					_result.WarningMessages.AddRange(otherResult.WarningMessages);
				if (otherResult.HasSuccessMessage)
					_result.SuccessMessages.AddRange(otherResult.SuccessMessages);
			}

			return _builder;
		}

		#endregion API
	}

	public class ResultBuilder : ResultBuilderBase<ResultBuilder, Result>
	{
		public ResultBuilder()
			: this(new Result())
		{
		}

		public ResultBuilder(Result Result)
			: base(Result)
		{
		}

		public static implicit operator Result?(ResultBuilder builder)
		{
			if (builder == null)
				return null;

			return builder._result;
		}

		public static implicit operator ResultBuilder?(Result Result)
		{
			if (Result == null)
				return null;

			return new ResultBuilder(Result);
		}

		public static IResult Empty()
			=> new ResultBuilder().Build();
	}

	public interface IResultBuilder<TBuilder, T, TObject> : IResultBuilder<TBuilder, TObject>
		where TBuilder : IResultBuilder<TBuilder, T, TObject>
		where TObject : IResult<T>
	{
		TBuilder WithData(T? data);

		TBuilder ClearData();

		bool MergeAllHasError(IResult<T> otherResult);
	}

	public abstract class ResultBuilderBase<TBuilder, T, TObject> : ResultBuilderBase<TBuilder, TObject>, IResultBuilder<TBuilder, T, TObject>
		where TBuilder : ResultBuilderBase<TBuilder, T, TObject>
		where TObject : IResult<T>
	{
		protected ResultBuilderBase(TObject Result)
			: base(Result)
		{
		}

		public override object? GetResult()
		{
			return _result.Data;
		}

		public TBuilder WithData(T? data)
		{
			_result.Data = data;
			return _builder;
		}

		public TBuilder ClearData()
		{
			_result.ClearData();
			return _builder;
		}

		public bool MergeAllHasError(IResult<T> otherResult)
		{
			if (otherResult != null)
			{
				if (otherResult.HasSuccessMessage)
					_result.SuccessMessages.AddRange(otherResult.SuccessMessages);

				if (otherResult.HasWarning)
					_result.WarningMessages.AddRange(otherResult.WarningMessages);

				if (otherResult.HasError)
					_result.ErrorMessages.AddRange(otherResult.ErrorMessages);

				AddAffectedEntities(otherResult.AffectedEntities ?? 0);

				_result.Data = otherResult.Data;
			}

			return _result.HasError;
		}
	}

	public class ResultBuilder<T> : ResultBuilderBase<ResultBuilder<T>, T, IResult<T>>
	{
		public ResultBuilder()
			: this(new Result<T>())
		{
		}

		public ResultBuilder(Result<T> Result)
			: base(Result)
		{
		}

		public static implicit operator Result<T>?(ResultBuilder<T> builder)
		{
			if (builder == null)
				return null;

			return builder._result as Result<T>;
		}

		public static implicit operator ResultBuilder<T>?(Result<T> Result)
		{
			if (Result == null)
				return null;

			return new ResultBuilder<T>(Result);
		}

		public static implicit operator ResultBuilder?(ResultBuilder<T> builder)
		{
			if (builder == null)
				return null;

			return new ResultBuilder((Result<T>)builder._result);
		}

		public static IResult<T> Empty()
			=> new ResultBuilder<T>().Build();

		public static IResult<T> FromResult(T result)
			=> new ResultBuilder<T>().WithData(result).Build();
	}
}

using Microsoft.Extensions.Logging;
using Raider.Extensions;
using Raider.Logging;
using Raider.Queries;
using Raider.Trace;
using Raider.Validation;
using System;

namespace Raider.QueryServices.Queries
{
	public interface IQueryResultBuilder<TBuilder, TResult, TObject>
		where TBuilder : IQueryResultBuilder<TBuilder, TResult, TObject>
		where TObject : IQueryResult<TResult>
	{
		TBuilder Object(TObject queryResult);
		TObject Build();

		bool MergeHasError<T>(IQueryResult<T> otherQueryResult);

		bool MergeAllHasError(IQueryResult<TResult> otherQueryResult);

		bool MergeHasError(MethodLogScope scope, ValidationResult validationResult);

		bool MergeHasError(ITraceInfo traceInfo, ValidationResult validationResult);

		bool HasError();

		object? GetResult();

		TBuilder ResultCount(long count);

		TBuilder ResultCount(params IQueryResult<TResult>[] queryResults);

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

		//TBuilder WithTraceInfo(ITraceInfo traceInfo, bool force = false);

		TBuilder ForAllSuccessMessages(Action<ILogMessage> logMessageConfigurator);

		TBuilder ForAllWarningMessages(Action<ILogMessage> logMessageConfigurator);

		TBuilder ForAllIErrorMessages(Action<ILogMessage> errorMessageConfigurator);

		TBuilder ForAllMessages(Action<ILogMessage> messageConfigurator);

		TBuilder Merge(IQueryResult<TResult> otherQueryResult);
		TBuilder WithResult(TResult? result);
		
		TBuilder ClearResult();
	}

	public abstract class QueryResultBuilderBase<TBuilder, TResult, TObject> : IQueryResultBuilder<TBuilder, TResult, TObject>
		where TBuilder : QueryResultBuilderBase<TBuilder, TResult, TObject>
		where TObject : IQueryResult<TResult>
	{
		protected readonly TBuilder _builder;
		protected TObject _queryResult;

		protected QueryResultBuilderBase(TObject queryResult)
		{
			_queryResult = queryResult;
			_builder = (TBuilder)this;
		}

		public virtual TBuilder Object(TObject queryResult)
		{
			_queryResult = queryResult ?? throw new ArgumentNullException(nameof(queryResult));

			return _builder;
		}

		public TObject Build()
			=> _queryResult;

		public TBuilder ResultCount(long count)
		{
			if (!_queryResult.ResultCount.HasValue)
			{
				_queryResult.ResultCount = 0;
			}
			_queryResult.ResultCount += count;

			return _builder;
		}

		public TBuilder ResultCount(params IQueryResult<TResult>[] queryResults)
		{
			if (queryResults != null)
			{
				if (!_queryResult.ResultCount.HasValue)
				{
					_queryResult.ResultCount = 0;
				}

				foreach (var cr in queryResults)
				{
					if (cr.ResultCount.HasValue)
					{
						_queryResult.ResultCount += cr.ResultCount.Value;
					}
				}
			}
			return _builder;
		}

		#region API

		public bool MergeHasError<T>(IQueryResult<T> otherQueryResult)
		{
			if (otherQueryResult != null && otherQueryResult.HasError)
				_queryResult.ErrorMessages.AddRange(otherQueryResult.ErrorMessages);

			return _queryResult.HasError;
		}

		public bool MergeAllHasError(IQueryResult<TResult> otherQueryResult)
		{
			if (otherQueryResult != null)
			{
				if (otherQueryResult.HasSuccessMessage)
					_queryResult.SuccessMessages.AddRange(otherQueryResult.SuccessMessages);

				if (otherQueryResult.HasWarning)
					_queryResult.WarningMessages.AddRange(otherQueryResult.WarningMessages);

				if (otherQueryResult.HasError)
					_queryResult.ErrorMessages.AddRange(otherQueryResult.ErrorMessages);

				ResultCount(otherQueryResult.ResultCount ?? 0);

				_queryResult.Result = otherQueryResult.Result;
			}

			return _queryResult.HasError;
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
					var errorMessage = QueryResultBuilderBase<TBuilder, TResult, TObject>.ValidationFailureToErrorMessage(traceInfo, failure);
					_queryResult.ErrorMessages.Add(errorMessage);
				}
				else
				{
					var warnigMessage = QueryResultBuilderBase<TBuilder, TResult, TObject>.ValidationFailureToWarningMessage(traceInfo, failure);
					_queryResult.WarningMessages.Add(warnigMessage);
				}
			}

			return _queryResult.HasError;
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
			return _queryResult.HasError;
		}

		public virtual object? GetResult()
		{
			return null;
		}

		public TBuilder ClearAllSuccessMessages()
		{
			_queryResult.SuccessMessages.Clear();
			return _builder;
		}


		public TBuilder WithSuccess(ILogMessage message)
		{
			_queryResult.SuccessMessages.Add(message);
			return _builder;
		}

		public TBuilder WithWarn(ILogMessage message)
		{
			_queryResult.WarningMessages.Add(message);
			return _builder;
		}

		public TBuilder WithError(IErrorMessage message)
		{
			_queryResult.ErrorMessages.Add(message);
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
			_queryResult.SuccessMessages.Add(logMessageBuilder.Build());
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
			_queryResult.WarningMessages.Add(logMessageBuilder.Build());
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
			_queryResult.ErrorMessages.Add(errorMessageBuilder.Build());
			return _builder;
		}


		public TBuilder ForAllSuccessMessages(Action<ILogMessage> logMessageConfigurator)
		{
			if (logMessageConfigurator == null)
				throw new ArgumentNullException(nameof(logMessageConfigurator));

			foreach (var successMessage in _queryResult.SuccessMessages)
				logMessageConfigurator.Invoke(successMessage);

			return _builder;
		}

		public TBuilder ForAllWarningMessages(Action<ILogMessage> logMessageConfigurator)
		{
			if (logMessageConfigurator == null)
				throw new ArgumentNullException(nameof(logMessageConfigurator));

			foreach (var warningMessage in _queryResult.WarningMessages)
				logMessageConfigurator.Invoke(warningMessage);

			return _builder;
		}

		public TBuilder ForAllIErrorMessages(Action<ILogMessage> errorMessageConfigurator)
		{
			if (errorMessageConfigurator == null)
				throw new ArgumentNullException(nameof(errorMessageConfigurator));

			foreach (var errorMessage in _queryResult.ErrorMessages)
				errorMessageConfigurator.Invoke(errorMessage);

			return _builder;
		}

		public TBuilder ForAllMessages(Action<ILogMessage> messageConfigurator)
			=> ForAllSuccessMessages(messageConfigurator)
				.ForAllWarningMessages(messageConfigurator)
				.ForAllIErrorMessages(messageConfigurator);



		public TBuilder Merge(IQueryResult<TResult> otherQueryResult)
		{
			if (otherQueryResult != null)
			{
				if (otherQueryResult.HasError)
					_queryResult.ErrorMessages.AddRange(otherQueryResult.ErrorMessages);
				if (otherQueryResult.HasWarning)
					_queryResult.WarningMessages.AddRange(otherQueryResult.WarningMessages);
				if (otherQueryResult.HasSuccessMessage)
					_queryResult.SuccessMessages.AddRange(otherQueryResult.SuccessMessages);
			}

			return _builder;
		}

		public TBuilder WithResult(TResult? result)
		{
			_queryResult.Result = result;
			return _builder;
		}

		public TBuilder ClearResult()
		{
			_queryResult.ClearResult();
			return _builder;
		}

		#endregion API
	}

	public class QueryResultBuilder<TResult> : QueryResultBuilderBase<QueryResultBuilder<TResult>, TResult, IQueryResult<TResult>>
	{
		public QueryResultBuilder()
			: this(new QueryResult<TResult>())
		{
		}

		public QueryResultBuilder(IQueryResult<TResult> queryResult)
			: base(queryResult)
		{
		}

		public static implicit operator QueryResult<TResult>?(QueryResultBuilder<TResult> builder)
		{
			if (builder == null)
				return null;

			return builder._queryResult as QueryResult<TResult>;
		}

		public static implicit operator QueryResultBuilder<TResult>?(QueryResult<TResult> queryResult)
		{
			if (queryResult == null)
				return null;

			return new QueryResultBuilder<TResult>(queryResult);
		}

		public static IQueryResult<TResult> Empty()
			=> new QueryResultBuilder<TResult>().Build();

		public static IQueryResult<TResult> FromResult(TResult result)
			=> new QueryResultBuilder<TResult>().WithResult(result).Build();
	}
}

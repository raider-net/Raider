using Microsoft.Extensions.Logging;
using Raider.Queries;
using Raider.Logging;
using Raider.Trace;
using System;
using System.Collections.Generic;

namespace Raider.QueryServices.Queries
{
	public interface IQueryResultBuilder<TBuilder, TResult, TObject>
		where TBuilder : IQueryResultBuilder<TBuilder, TResult, TObject>
		where TObject : IQueryResult<TResult>
	{
		TBuilder Object(TObject queryResult);
		TObject Build();

		bool MergeHasError<T>(IQueryResult<T> otherQueryResult);

		//bool MergeIfError(string error);

		bool HasError();

		object? GetResult();

		TBuilder ResultCount(long count);

		TBuilder ResultCount(params IQueryResult<TResult>[] queryResults);

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

		TBuilder Merge(IQueryResult<TResult> otherQueryResult);
		TBuilder WithResult(TResult? result);
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

		//public bool MergeIfError(string error)
		//{
		//	if (!string.IsNullOrWhiteSpace(error))
		//		WithError(error);

		//	return _queryResult.HasError;
		//}

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

		//public TBuilder WithSuccess(string message, Action<ILogMessage>? logMessageConfigurator = null)
		//{
		//	var logMessage =
		//		new LogMessageBuilder(_queryResult.TraceInfo)
		//			.LogLevel(LogLevel.Information)
		//			.Message(message)
		//			.StackTrace(StackTraceHelper.GetStackTrace(true))
		//			.Build();
		//	logMessageConfigurator?.Invoke(logMessage);
		//	_queryResult.SuccessMessages.Add(logMessage);
		//	return _builder;
		//}

		//public TBuilder WithWarn(string message, Action<ILogMessage>? logMessageConfigurator = null)
		//{
		//	var logMessage =
		//		new LogMessageBuilder(_queryResult.TraceInfo)
		//			.LogLevel(LogLevel.Warning)
		//			.Message(message)
		//			.StackTrace(StackTraceHelper.GetStackTrace(true))
		//			.Build();
		//	logMessageConfigurator?.Invoke(logMessage);
		//	_queryResult.WarningMessages.Add(logMessage);
		//	return _builder;
		//}

		//public TBuilder WithError(string message, Action<IErrorMessage>? errorMessageConfigurator = null)
		//{
		//	var errorMessage = new LogMessageBuilder(_queryResult.TraceInfo)
		//		.LogLevel(LogLevel.Error)
		//		.Message(message)
		//		.StackTrace(StackTraceHelper.GetStackTrace(true))
		//		.BuildErrorMessage();
		//	errorMessageConfigurator?.Invoke(errorMessage);
		//	_queryResult.ErrorMessages.Add(errorMessage);
		//	return _builder;
		//}

		//public TBuilder WithError(Exception ex, Action<IErrorMessage>? errorMessageConfigurator = null)
		//{
		//	var errorMessage = new LogMessageBuilder(_queryResult.TraceInfo)
		//		.LogLevel(LogLevel.Error)
		//		.ExceptionInfo(ex)
		//		.StackTrace(StackTraceHelper.GetStackTrace(true))
		//		.BuildErrorMessage();
		//	errorMessageConfigurator?.Invoke(errorMessage);
		//	_queryResult.ErrorMessages.Add(errorMessage);
		//	return _builder;
		//}

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

		public TBuilder WithSuccess(ITraceInfo traceInfo, Action<LogMessageBuilder>? logMessageConfigurator)
		{
			var logMessageBuilder =
				new LogMessageBuilder(traceInfo)
					.LogLevel(LogLevel.Information);
			logMessageConfigurator?.Invoke(logMessageBuilder);
			_queryResult.SuccessMessages.Add(logMessageBuilder.Build());
			return _builder;
		}

		public TBuilder WithWarn(ITraceInfo traceInfo, Action<LogMessageBuilder>? logMessageConfigurator)
		{
			var logMessageBuilder =
				new LogMessageBuilder(traceInfo)
					.LogLevel(LogLevel.Warning);
			logMessageConfigurator?.Invoke(logMessageBuilder);
			_queryResult.WarningMessages.Add(logMessageBuilder.Build());
			return _builder;
		}

		public TBuilder WithError(ITraceInfo traceInfo, Action<ErrorMessageBuilder>? errorMessageConfigurator)
		{
			var errorMessageBuilder =
				new ErrorMessageBuilder(traceInfo)
					.LogLevel(LogLevel.Error);
			errorMessageConfigurator?.Invoke(errorMessageBuilder);
			_queryResult.ErrorMessages.Add(errorMessageBuilder.Build());
			return _builder;
		}

		//public TBuilder WithTraceInfo(ITraceInfo traceInfo, bool force = false)
		//{
		//	_queryResult.TraceInfo = traceInfo;

		//	var builder = new LogMessageBuilder(_queryResult.TraceInfo);
		//	void SetTraceInfo(ILogMessage msg) => builder.Object(msg).TraceInfo(traceInfo, force);

		//	_queryResult.SuccessMessages.ForEach(x => SetTraceInfo(x));
		//	_queryResult.WarningMessages.ForEach(x => SetTraceInfo(x));
		//	_queryResult.ErrorMessages.ForEach(x => SetTraceInfo(x));

		//	return _builder;
		//}

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

		//public TBuilder MergeAllIErrorMessages(string? separator = null)
		//{
		//	if (1 < _queryResult.ErrorMessages.Count)
		//	{
		//		if (separator == null)
		//			separator = Environment.NewLine;

		//		var errorMessage = _queryResult.ErrorMessages[0];

		//		var joinedMessages = string.Join(separator, _queryResult.ErrorMessages.Select(x => x.Message));
		//		var joinedDetails = string.Join(separator, _queryResult.ErrorMessages.Select(x => x.Detail));

		//		new LogMessageBuilder(errorMessage).LogLevel(LogLevel.Error)
		//			.Message(joinedMessages)
		//			.Detail(joinedDetails);

		//		_queryResult.ErrorMessages.Clear();
		//		_queryResult.ErrorMessages.Add(errorMessage);
		//	}

		//	return _builder;
		//}

		//public TBuilder MergeAllWarningMessages(string? separator = null)
		//{
		//	if (1 < _queryResult.WarningMessages.Count)
		//	{
		//		if (separator == null)
		//			separator = Environment.NewLine;

		//		var warningMessage = new LogMessageBuilder(_queryResult.WarningMessages[0]);
		//		var joinedMessages = string.Join(separator, _queryResult.WarningMessages.Select(x => x.Message));
		//		var joinedDetails = string.Join(separator, _queryResult.WarningMessages.Select(x => x.Detail));
		//		warningMessage.Message(joinedMessages);
		//		warningMessage.Detail(joinedDetails);
		//		_queryResult.WarningMessages.Clear();
		//		_queryResult.WarningMessages.Add(warningMessage.Build());
		//	}

		//	return _builder;
		//}

		//public TBuilder MergeAllSuccessMessages(string? separator = null)
		//{
		//	if (1 < _queryResult.SuccessMessages.Count)
		//	{
		//		if (separator == null)
		//			separator = Environment.NewLine;

		//		var successMessage = new LogMessageBuilder(_queryResult.SuccessMessages[0]);
		//		var joinedMessages = string.Join(separator, _queryResult.SuccessMessages.Select(x => x.Message));
		//		var joinedDetails = string.Join(separator, _queryResult.SuccessMessages.Select(x => x.Detail));
		//		successMessage.Message(joinedMessages);
		//		successMessage.Detail(joinedDetails);
		//		_queryResult.SuccessMessages.Clear();
		//		_queryResult.SuccessMessages.Add(successMessage.Build());
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

		#endregion API
	}

	public class QueryResultBuilder<TResult> : QueryResultBuilderBase<QueryResultBuilder<TResult>, TResult, IQueryResult<TResult>>
	{
		private class QueryResult<T> : IQueryResult<T>
		{
			//public ITraceInfo TraceInfo { get; set; }

			public List<ILogMessage> SuccessMessages { get; }

			public List<ILogMessage> WarningMessages { get; }

			public List<IErrorMessage> ErrorMessages { get; }

			public bool HasSuccessMessage => 0 < SuccessMessages.Count;

			public bool HasWarning => 0 < WarningMessages.Count;

			public bool HasError => 0 < ErrorMessages.Count;

			public bool HasAnyMessage => HasSuccessMessage || HasWarning || HasError;

			public long? ResultCount { get; set; }

			public T? Result { get; set; }

			public QueryResult()
			{
				//TraceInfo = traceInfo;
				SuccessMessages = new List<ILogMessage>();
				WarningMessages = new List<ILogMessage>();
				ErrorMessages = new List<IErrorMessage>();
			}
		}

		public QueryResultBuilder()
			: this(new QueryResult<TResult>())
		{
		}

		public QueryResultBuilder(IQueryResult<TResult> queryResult)
			: base(queryResult)
		{
		}

		//public static implicit operator QueryResult?(QueryResultBuilder builder)
		//{
		//	if (builder == null)
		//		return null;

		//	return builder._queryResult;
		//}

		//public static implicit operator QueryResultBuilder?(QueryResult queryResult)
		//{
		//	if (queryResult == null)
		//		return null;

		//	return new QueryResultBuilder(queryResult);
		//}
	}
}

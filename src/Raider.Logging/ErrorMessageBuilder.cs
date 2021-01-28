using Microsoft.Extensions.Logging;
using Raider.Trace;
using System;

namespace Raider.Logging
{
	public interface IErrorMessageBuilder<TBuilder, TObject> : ILogMessageBuilder<TBuilder, TObject>
		where TBuilder : IErrorMessageBuilder<TBuilder, TObject>
		where TObject : IErrorMessage
	{
	}

	public abstract class ErrorMessageBuilderBase<TBuilder, TObject> : LogMessageBuilderBase<TBuilder, TObject>, IErrorMessageBuilder<TBuilder, TObject>
		where TBuilder : ErrorMessageBuilderBase<TBuilder, TObject>
		where TObject : IErrorMessage
	{
		protected ErrorMessageBuilderBase(TObject errorMessage)
			:base(errorMessage)
		{
		}

		public override TBuilder LogLevel(LogLevel logLevel, bool force = false)
		{
			if (logLevel != Microsoft.Extensions.Logging.LogLevel.Error
				&& logLevel != Microsoft.Extensions.Logging.LogLevel.Critical)
				throw new InvalidOperationException($"Invalid {nameof(logLevel)} = {logLevel}");

			if (force || _logMessage.LogLevel == default)
				_logMessage.LogLevel = logLevel;

			return _builder;
		}
	}

	public class ErrorMessageBuilder : ErrorMessageBuilderBase<ErrorMessageBuilder, IErrorMessage>
	{
		public ErrorMessageBuilder(ITraceInfo traceInfo)
			: this(new Internal.ErrorMessage(traceInfo))
		{
		}

		public ErrorMessageBuilder(IErrorMessage errorMessage)
			: base(errorMessage)
		{
		}

		//public static implicit operator ErrorMessageInternal?(ErrorMessageBuilder builder)
		//{
		//	if (builder == null)
		//		return null;

		//	return builder._errorMessage as ErrorMessageInternal;
		//}

		//public static implicit operator ErrorMessageBuilder?(ErrorMessageInternal errorMessage)
		//{
		//	if (errorMessage == null)
		//		return null;

		//	return new ErrorMessageBuilder(errorMessage);
		//}
	}
}

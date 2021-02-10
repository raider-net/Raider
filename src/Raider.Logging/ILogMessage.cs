using Microsoft.Extensions.Logging;
using Raider.Trace;
using System;

namespace Raider.Logging
{
	public interface ILogMessage : Serializer.IDictionaryObject
	{
		long? IdLogMessage { get; set; }

		LogLevel LogLevel { get; set; }

		int IdLogLevel => (int)LogLevel;

		DateTimeOffset Created { get; set; }

		ITraceInfo TraceInfo { get; set; }

		string? LogCode { get; set; }

		string? ClientMessage { get; set; }

		string? InternalMessage { get; set; }

		string? StackTrace { get; set; }

		string? Detail { get; set; }

		string ClientMessageWithId => ToString(true, false, false);

		string ClientMessageWithIdAndPropName => ToString(true, true, false);

		string FullMessage => ToString(true, true, true);

		string? CommandQueryName { get; set; }

		long? IdCommandQuery { get; set; }

		bool IsLogged { get; set; }

		decimal? MethodCallElapsedMilliseconds { get; set; }

		string? PropertyName { get; set; }

		object? ValidationFailure { get; set; }

		string? DisplayPropertyName { get; set; }

		bool? IsValidationError { get; set; }

		string ToString(bool withId, bool withPropertyName, bool withDetail);
	}
}

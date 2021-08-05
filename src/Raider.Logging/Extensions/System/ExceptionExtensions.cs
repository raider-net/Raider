using Raider.Logging;
using System;

namespace Raider.Extensions
{
	public static class ExceptionExtensions
	{
		private const string RAIDER_LOG_MESSAGE = nameof(RAIDER_LOG_MESSAGE);

		public static T AppendLogMessage<T>(this T exception, ILogMessage logMessage)
			where T : Exception
		{
			if (exception == null)
				throw new ArgumentNullException(nameof(exception));

			if (exception.Data.Contains(RAIDER_LOG_MESSAGE))
			{
				var value = exception.Data[RAIDER_LOG_MESSAGE];
				if (value is ErrorMessage msg)
				{
					msg.Detail = string.IsNullOrWhiteSpace(msg.Detail)
						? $"---NEXT LOG MESSAGE---{Environment.NewLine}{logMessage.FullMessage}"
						: $"{msg.Detail}{Environment.NewLine}---NEXT LOG MESSAGE---{Environment.NewLine}{logMessage.FullMessage}";
				}
			}
			else
			{
				exception.Data[RAIDER_LOG_MESSAGE] = logMessage;
			}

			return exception;
		}

		public static T SetLogMessage<T>(this T exception, ILogMessage logMessage)
			where T : Exception
		{
			if (exception == null)
				throw new ArgumentNullException(nameof(exception));

			exception.Data[RAIDER_LOG_MESSAGE] = logMessage;

			return exception;
		}

		public static ILogMessage? GetLogMessage(this Exception exception)
		{
			if (exception == null)
				throw new ArgumentNullException(nameof(exception));

			return exception.Data.Contains(RAIDER_LOG_MESSAGE)
				? exception.Data[RAIDER_LOG_MESSAGE] as ILogMessage
				: null;
		}
	}
}

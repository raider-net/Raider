using Raider.Extensions;
using System;
using System.Runtime.Serialization;

namespace Raider.Queries.Exceptions
{
	public class QueryResultException<TResult> : Exception
	{
		public QueryResultException(IQueryResult<TResult> commandResult)
		{
			if (commandResult?.ErrorMessages != null)
			{
				foreach (var errorMessage in commandResult.ErrorMessages)
					this.AppendLogMessage(errorMessage);
			}
		}

		public QueryResultException(IQueryResult<TResult> commandResult, string? message)
			: base(message)
		{
			if (commandResult?.ErrorMessages != null)
			{
				foreach (var errorMessage in commandResult.ErrorMessages)
					this.AppendLogMessage(errorMessage);
			}
		}

		public QueryResultException(IQueryResult<TResult> commandResult, string? message, Exception? innerException)
			: base(message, innerException)
		{
			if (commandResult?.ErrorMessages != null)
			{
				foreach (var errorMessage in commandResult.ErrorMessages)
					this.AppendLogMessage(errorMessage);
			}
		}

		protected QueryResultException(IQueryResult<TResult> commandResult, SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
			if (commandResult?.ErrorMessages != null)
			{
				foreach (var errorMessage in commandResult.ErrorMessages)
					this.AppendLogMessage(errorMessage);
			}
		}
	}
}

using Raider.Extensions;
using System;
using System.Runtime.Serialization;

namespace Raider.Exceptions
{
	public class ResultException : Exception
	{
		public ResultException(IResult Result)
		{
			if (Result?.ErrorMessages != null)
			{
				foreach (var errorMessage in Result.ErrorMessages)
					this.AppendLogMessage(errorMessage);
			}
		}

		public ResultException(IResult Result, string? message)
			: base(message)
		{
			if (Result?.ErrorMessages != null)
			{
				foreach (var errorMessage in Result.ErrorMessages)
					this.AppendLogMessage(errorMessage);
			}
		}

		public ResultException(IResult Result, string? message, Exception? innerException)
			: base(message, innerException)
		{
			if (Result?.ErrorMessages != null)
			{
				foreach (var errorMessage in Result.ErrorMessages)
					this.AppendLogMessage(errorMessage);
			}
		}

		protected ResultException(IResult Result, SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
			if (Result?.ErrorMessages != null)
			{
				foreach (var errorMessage in Result.ErrorMessages)
					this.AppendLogMessage(errorMessage);
			}
		}
	}
}

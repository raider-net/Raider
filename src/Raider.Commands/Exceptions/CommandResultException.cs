using Raider.Extensions;
using System;
using System.Runtime.Serialization;

namespace Raider.Commands.Exceptions
{
	public class CommandResultException : Exception
	{
		public CommandResultException(ICommandResult commandResult)
		{
			if (commandResult?.ErrorMessages != null)
			{
				foreach (var errorMessage in commandResult.ErrorMessages)
					this.AppendLogMessage(errorMessage);
			}
		}

		public CommandResultException(ICommandResult commandResult, string? message)
			: base(message)
		{
			if (commandResult?.ErrorMessages != null)
			{
				foreach (var errorMessage in commandResult.ErrorMessages)
					this.AppendLogMessage(errorMessage);
			}
		}

		public CommandResultException(ICommandResult commandResult, string? message, Exception? innerException)
			: base(message, innerException)
		{
			if (commandResult?.ErrorMessages != null)
			{
				foreach (var errorMessage in commandResult.ErrorMessages)
					this.AppendLogMessage(errorMessage);
			}
		}

		protected CommandResultException(ICommandResult commandResult, SerializationInfo info, StreamingContext context)
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

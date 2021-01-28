using Raider.Exceptions;
using System;

namespace Raider.Extensions
{
	public static class ExceptionExtensions
	{
		public static string ToStringTrace(this Exception ex)
		{
			return ExceptionHelper.ToStringTrace(ex);
		}
	}
}

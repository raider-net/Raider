using Microsoft.AspNetCore.Http;
using Raider.Logging;
using System;
using System.Collections.Generic;

namespace Raider.AspNetCore.Middleware.Exceptions
{
	public enum ExceptionHandlerMode
	{
		CatchAndRedirect = 0,
		CatchOnly = 1
	}

	public class ExceptionHandlerOptions
	{
		public ExceptionHandlerMode Mode { get; set; } = ExceptionHandlerMode.CatchAndRedirect;
		public string? DefaultExceptionPath { get; set; }
		public string? NotFoundExceptionPath { get; set; }
		public bool HandleAllClientAndServerErrors { get; set; }
		public List<int>? HandleOnlyStatusCodes { get; set; }
		internal Action<IErrorMessage, HttpContext>? OnErrorOccurs { get; set; } //Action<IErrorMessage, HttpContext>
		public ExceptionHandlerDelegate? ExternalExceptionHandler { get; set; }
		public bool CheckEveryResponseStatusCode { get; set; }

		public ExceptionHandlerOptions OnError(Action<IErrorMessage, HttpContext> action)
		{
			OnErrorOccurs = action;
			return this;
		}
	}
}

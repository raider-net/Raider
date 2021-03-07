using Microsoft.AspNetCore.Http;
using Raider.Logging;
using System;
using System.Collections.Generic;

namespace Raider.AspNetCore.Middleware.Exceptions
{
	public class ExceptionHandlerOptions
	{
		public int MinimalStatusCodeToRedirect { get; set; } = StatusCodes.Status401Unauthorized;
		public string? DefaultExceptionPath { get; set; }
		public Dictionary<int, string> StatusCodeRedirectPaths { get; set; }
		internal Action<IErrorMessage, HttpContext, bool>? OnErrorOccurs { get; set; } //Action<IErrorMessage, HttpContext, redirecting>

		public ExceptionHandlerOptions()
		{
			StatusCodeRedirectPaths = new Dictionary<int, string>();
		}

		public ExceptionHandlerOptions SetStatusCodeRedirecting(int statusCode, string redirectToPath)
		{
			StatusCodeRedirectPaths[statusCode] = redirectToPath;
			return this;
		}

		public ExceptionHandlerOptions OnError(Action<IErrorMessage, HttpContext, bool> action)
		{
			OnErrorOccurs = action;
			return this;
		}
	}
}

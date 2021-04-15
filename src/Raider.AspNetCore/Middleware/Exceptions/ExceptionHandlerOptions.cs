using Microsoft.AspNetCore.Http;
using Raider.Logging;
using System;

namespace Raider.AspNetCore.Middleware.Exceptions
{
	public class ExceptionHandlerOptions
	{
		public string? DefaultExceptionPath { get; set; }
		public string? NotFoundExceptionPath { get; set; }
		internal Action<IErrorMessage, HttpContext>? OnErrorOccurs { get; set; } //Action<IErrorMessage, HttpContext>
		public RequestDelegate? ExceptionHandler { get; set; }

		public ExceptionHandlerOptions OnError(Action<IErrorMessage, HttpContext> action)
		{
			OnErrorOccurs = action;
			return this;
		}
	}
}

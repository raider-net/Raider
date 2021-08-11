using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;

namespace Raider.AspNetCore.Middleware.Security
{
	public class SecurityMiddleware
	{
		private readonly RequestDelegate _next;
		private readonly SecurityOptions _options;

		public SecurityMiddleware(
			RequestDelegate next,
			IOptions<SecurityOptions> options)
		{
			_next = next;
			_options = options?.Value ?? throw new ArgumentNullException(nameof(options));
		}

		public Task Invoke(HttpContext context)
		{
			context.Response.OnStarting(OnResponseStarting, Tuple.Create(this, context));
			return _next(context);
		}

		private void SetHeaders(HttpContext context)
		{
			foreach (var kvp in _options.AddHeaders)
			{
				ResponseHeaderOptions.Apply(context, kvp.Value);
			}
			foreach (var kvp in _options.RemoveHeaders)
			{
				ResponseHeaderOptions.Apply(context, kvp.Value);
			}
		}

		private static Task OnResponseStarting(object state)
		{
			var tuple = (Tuple<SecurityMiddleware, HttpContext>)state;
			var middleware = tuple.Item1;
			var context = tuple.Item2;

			middleware.SetHeaders(context);
			return Task.CompletedTask;
		}
	}
}

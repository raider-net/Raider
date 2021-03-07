using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

namespace Raider.AspNetCore.Middleware.Authentication.Authenticate
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
	public abstract class AuthenticateAttribute : TypeFilterAttribute
	{
		public AuthenticateAttribute()
			: base(typeof(AuthenticateAuthorizationFilter))
		{
			AddArgument(new AuthenticateAuthorizationRequirement());
			Order = Int32.MinValue;
		}

		protected void AddArgument(object value)
		{
			if (value == null)
			{
				throw new ArgumentNullException(nameof(value));
			}

			List<object> args = new List<object>(base.Arguments ?? new object[] { })
			{
				value
			};

			base.Arguments = args.ToArray();
		}
	}
}

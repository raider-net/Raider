using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

namespace Raider.AspNetCore.Middleware.Authorization
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
	public abstract class PermissionAttribute : TypeFilterAttribute
	{
		protected PermissionAttribute()
			: base(typeof(PermissionAuthorizationFilter))
		{
		}

		public PermissionAttribute(object[] tokens)
			: base(typeof(PermissionAuthorizationFilter))
		{
			AddArgument(new PermissionAuthorizationRequirement(tokens));
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

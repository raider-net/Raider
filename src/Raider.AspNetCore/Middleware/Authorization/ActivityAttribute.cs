using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

namespace Raider.AspNetCore.Middleware.Authorization
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public abstract class ActivityAttribute : TypeFilterAttribute
	{
		protected ActivityAttribute()
            : base(typeof(ActivityAuthorizationFilter))
        {
        }

        public ActivityAttribute(object[] tokens)
            : base(typeof(ActivityAuthorizationFilter))
        {
            AddArgument(new ActivityAuthorizationRequirement(tokens));
            Order = Int32.MinValue;
		}

		//public string GetActivityToken()
		//{
		//	var requirement = base
		//			.Arguments
		//			.FirstOrDefault(a => a is ActivityAuthorizationRequirement) as ActivityAuthorizationRequirement;
		//	return requirement?.Token.ToString();
		//}

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

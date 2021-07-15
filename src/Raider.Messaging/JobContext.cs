using System;

namespace Raider.Messaging
{
	public class JobContext : SubscriberContext
	{
		public JobContext(IServiceProvider serviceProvider, IApplicationContext applicationContext)
			: base(serviceProvider, applicationContext)
		{
		}
	}
}

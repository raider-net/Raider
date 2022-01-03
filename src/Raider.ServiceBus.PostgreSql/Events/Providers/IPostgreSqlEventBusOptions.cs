using Raider.ServiceBus.Config;
using Raider.ServiceBus.Events.Config;
using Raider.Validation;

namespace Raider.ServiceBus.PostgreSql.Events.Providers
{
	public interface IPostgreSqlEventBusOptions : IPostgreSqlBusOptions, IEventBusOptions, IBusOptions, IValidable
	{
	}
}

using Raider.ServiceBus.Config;
using Raider.ServiceBus.Messages.Config;
using Raider.Validation;

namespace Raider.ServiceBus.PostgreSql.Messages.Providers
{
	public interface IPostgreSqlMessageBusOptions : IPostgreSqlBusOptions, IMessageBusOptions, IBusOptions, IValidable
	{
	}
}

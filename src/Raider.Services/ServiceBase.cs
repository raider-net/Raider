using System.Threading;
using System.Threading.Tasks;

namespace Raider.Services
{
	public abstract class ServiceBase
	{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
		public ServiceContext ServiceContext { get; internal set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

		public virtual void Initialize() { }

		public virtual Task InitializeAsync(CancellationToken cancellationToken = default)
			=> Task.CompletedTask;
	}
}

using System.Threading;
using System.Threading.Tasks;

namespace Raider.Web
{
	public interface ICookieStoreVallidator
	{
		Task<bool> ExistsAsync(IApplicationContext appContext, string authCookie, CancellationToken cancellationToken = default);
	}
}

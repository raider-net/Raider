using Microsoft.AspNetCore.Http;
using Raider.Trace;
using Raider.Web;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Raider.AspNetCore.Middleware.Authentication
{
	public interface ICookieStore : ICookieStoreVallidator
	{
		Task<bool> InsertAsync(HttpContext context, string authCookie, DateTime createdUtc, DateTime validToUtc, int? idUser, CancellationToken cancellationToken = default);
		Task<bool> DeleteAsync(HttpContext context, string authCookie, bool setDeletedFlag, CancellationToken cancellationToken = default);
		Task ClearAsync(ITraceInfo traceInfo, bool setDeletedFlag, DateTime? expiredUtc, int? idUser, CancellationToken cancellationToken = default);
	}
}

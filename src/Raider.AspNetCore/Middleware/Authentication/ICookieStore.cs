﻿using Microsoft.AspNetCore.Http;
using Raider.Trace;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Raider.AspNetCore.Middleware.Authentication
{
	public interface ICookieStore
	{
		Task<bool> InsertAsync(HttpContext context, string authCookie, DateTime createdUtc, DateTime validToUtc, int? idUser, CancellationToken cancellationToken = default);
		Task<bool> ExistsAsync(HttpContext context, string authCookie, CancellationToken cancellationToken = default);
		Task DeleteAsync(HttpContext context, string authCookie, bool setDeletedFlag, CancellationToken cancellationToken = default);
		Task ClearAsync(ITraceInfo traceInfo, bool setDeletedFlag, DateTime? expiredUtc, int? idUser, CancellationToken cancellationToken = default);
	}
}

using Raider.AspNetCore.Logging.Dto;
using Raider.Web.Logging;
using System;

namespace Raider.AspNetCore.Logging.Internal
{
	internal class SilentAspNetLogWriter : IAspNetLogWriter, IDisposable
	{
		public static readonly IAspNetLogWriter Instance = new SilentAspNetLogWriter();

		public void WriteRequest(RequestDto request) { }

		public void WriteRequestAuthentication(RequestAuthentication requestAuthentication) { }

		public void WriteResponse(ResponseDto response) { }

		public void Dispose() { }
	}
}

using Raider.AspNetCore.Logging.Dto;
using System;

namespace Raider.AspNetCore.Logging.Internal
{
	internal class SilentAspNetLogWriter : IAspNetLogWriter, IDisposable
	{
		public static readonly IAspNetLogWriter Instance = new SilentAspNetLogWriter();

		public void WriteRequest(Request request) { }

		public void WriteRequestAuthentication(RequestAuthentication requestAuthentication) { }

		public void WriteResponse(Response response) { }

		public void Dispose() { }
	}
}

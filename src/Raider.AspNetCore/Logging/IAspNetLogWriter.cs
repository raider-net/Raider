using Raider.AspNetCore.Logging.Dto;
using System;

namespace Raider.AspNetCore.Logging
{
	public interface IAspNetLogWriter : IDisposable
	{
		void WriteRequest(Request request);
		void WriteRequestAuthentication(RequestAuthentication requestAuthentication);
		void WriteResponse(Response response);
	}
}

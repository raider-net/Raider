using Raider.AspNetCore.Logging.Dto;
using Raider.Web.Logging;
using System;

namespace Raider.AspNetCore.Logging
{
	public interface IAspNetLogWriter : IDisposable
	{
		void WriteRequest(RequestDto request);
		void WriteRequestAuthentication(RequestAuthentication requestAuthentication);
		void WriteResponse(ResponseDto response);
	}
}

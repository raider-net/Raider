﻿using Microsoft.AspNetCore.Http;
using Raider.Web.Logging;
using System;
using System.Collections.Generic;

namespace Raider.AspNetCore.Logging
{
	public static class ResponseDtoFactory
	{
		public static ResponseDto Create(
			HttpResponse httpResponse,
			Guid correlationId,
			string? externalCorrelationId,
			int? statusCode,
			string? body,
			byte[]? bodyByteArray,
			string? error,
			decimal? elapsedMilliseconds,
			bool logResponseHeaders)
		{
			if (httpResponse == null)
				throw new ArgumentNullException(nameof(httpResponse));

			var reponse = new ResponseDto
			{
				CorrelationId = correlationId,
				ExternalCorrelationId = externalCorrelationId,
				StatusCode = statusCode,
				Body = string.IsNullOrWhiteSpace(body)
					? null
					: body,
				BodyByteArray = (bodyByteArray != null && bodyByteArray.Length == 0)
					? null
					: bodyByteArray,
				Error = error,
				ElapsedMilliseconds = elapsedMilliseconds,
			};

			if (logResponseHeaders)
			{
				try
				{
					if (httpResponse.Headers != null)
					{
						var headers = new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>(httpResponse.Headers);
						reponse.Headers = System.Text.Json.JsonSerializer.Serialize(headers);
					}
				}
				catch { }
			}

			return reponse;
		}
	}
}

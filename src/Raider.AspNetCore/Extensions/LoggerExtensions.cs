using Microsoft.Extensions.Logging;
using Raider.AspNetCore.Logging.Dto;

namespace Raider.Logging.Extensions
{
	internal static class LoggerExtensions
	{
		public static void LogRequest(this ILogger logger, Request request)
			=> logger.LogInformation($"{Raider.AspNetCore.Logging.LoggerSettings.Request_Template}", request.ToDictionary());

		public static void LogResponse(this ILogger logger, Response response)
			=> logger.LogInformation($"{Raider.AspNetCore.Logging.LoggerSettings.Response_Template}", response.ToDictionary());

		public static void LogRequestAuthentication(this ILogger logger, RequestAuthentication requestAuthentication)
			=> logger.LogInformation($"{Raider.AspNetCore.Logging.LoggerSettings.RequestAuthentication_Template}", requestAuthentication.ToDictionary());
	}
}

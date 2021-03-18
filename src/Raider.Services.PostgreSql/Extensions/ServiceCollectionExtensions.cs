using Microsoft.Extensions.DependencyInjection;
using Raider.Services.Commands;
using System;

namespace Raider.Services.PostgreSql.Extensions
{
	public static class ServiceCollectionExtensions
	{
		public static IServiceCollection AddCommandLogger(this IServiceCollection services, Action<CommandEntryOptions> entryOptions, Action<CommandExitOptions> exitOptions)
		{
			var entryOpt = new CommandEntryOptions();
			entryOptions?.Invoke(entryOpt);
			CommandLogger.SetEntryWriter(new CommandEntryWriter(entryOpt));

			var exitOpt = new CommandExitOptions();
			exitOptions?.Invoke(exitOpt);
			CommandLogger.SetExitWriter(new CommandExitWriter(exitOpt));

			services.AddSingleton<ICommandLogger, CommandLogger>();

			return services;
		}
	}
}

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Raider.Logging.Extensions;
using Raider.Plugins.Catalogs;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Raider.Plugins.Internal
{
	internal class PluginsInitializer : IHostedService
	{
		private readonly IEnumerable<IPluginCatalog> _pluginCatalogs;
		private readonly ILogger _logger;

		public PluginsInitializer(IEnumerable<IPluginCatalog> pluginCatalogs, ILogger<PluginsInitializer> logger)
		{
			_pluginCatalogs = pluginCatalogs ?? throw new ArgumentNullException(nameof(pluginCatalogs));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		public async Task StartAsync(CancellationToken cancellationToken)
		{
			foreach (var pluginCatalog in _pluginCatalogs)
			{
				try
				{
					await pluginCatalog.Initialize();

					foreach (var plugin in pluginCatalog.GetPlugins())
						;
				}
				catch (Exception ex)
				{
					_logger.LogErrorMessage(x => x.ExceptionInfo(ex).Detail($"Failed to initialize {pluginCatalog.GetType().AssemblyQualifiedName}"));
				}
			}
		}

		public Task StopAsync(CancellationToken cancellationToken)
		{
			return Task.CompletedTask;
		}
	}
}

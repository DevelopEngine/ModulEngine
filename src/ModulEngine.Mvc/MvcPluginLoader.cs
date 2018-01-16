using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ModulEngine.Composition;

namespace ModulEngine
{
    public class MvcPluginLoader : MvcPluginLoader<IPlugin>
    {
        public MvcPluginLoader(ILogger<MvcPluginLoader> logger) : base(logger)
        {
        }
    }

    public class MvcPluginLoader<TPlugin> : IPluginLoader where TPlugin : class, IPlugin
    {
        private readonly ILogger<MvcPluginLoader> _logger;

        public MvcPluginLoader(
            ILogger<MvcPluginLoader> logger
        )
        {
            _logger = logger;
        }

        public void LoadPlugins(IAppBuilder builder, IServiceProvider provider)
        {
            var plugins = provider.GetServices<TPlugin>();
            _logger.LogDebug("Resolved {0} plugins from provider.", plugins.Count());
            foreach (var plugin in plugins)
            {
                var name = plugin.GetType().Name;
                try
                {
                    _logger.LogDebug($"Registering '{name}' plugin");
                    plugin.AddServices(builder);
                    _logger.LogInformation($"Successfully loaded '{name}' plugin!");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"Plugin {name} failed to register!");
                    _logger.LogError(500, ex, ex.Message);
                }
            }
        }
    }
}
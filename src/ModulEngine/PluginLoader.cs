using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using ModulEngine.Composition;

namespace ModulEngine
{
    public class PluginLoader : PluginLoader<IPlugin>
    {
    }

    public class PluginLoader<TPlugin> : IPluginLoader where TPlugin : IPlugin
    {
        public void LoadPlugins(IAppBuilder builder, IServiceProvider provider)
        {
            var plugins = provider.GetServices<TPlugin>();
            foreach (var plugin in plugins)
            {
                var name = plugin.GetType().Name;
                try
                {
                    plugin.AddServices(builder);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Plugin {name} failed to register!");
                    Console.WriteLine(ex);
                }
            }
        }
    }
}
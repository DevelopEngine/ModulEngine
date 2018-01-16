using System;
using Microsoft.Extensions.DependencyInjection;
using ModulEngine.Composition;

namespace ModulEngine
{
    public class AppBuilder : IAppBuilder
    {
        public AppBuilder(IServiceCollection services)
        {
            Services = services;
        }
        public IServiceCollection Services { get; }

        public void Build() {
            var provider = Services.BuildServiceProvider();
            var loader = provider.GetService<IPluginLoader>();
            loader.LoadPlugins(this, provider);
        }
    }
}

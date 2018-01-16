using System;
using Microsoft.Extensions.DependencyInjection;

namespace ModulEngine.Composition

{
    public interface IPluginLoader {
        void LoadPlugins(IAppBuilder builder, IServiceProvider provider);
    }
}
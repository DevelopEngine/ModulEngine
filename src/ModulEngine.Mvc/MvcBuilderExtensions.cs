using System;
// using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ModulEngine.Composition;

namespace ModulEngine
{
    public static class MvcBuilderExtensions
    {
        public static IMvcBuilder AddModulEngine(this IMvcBuilder builder, Action<IAppBuilder> configure = null)
        {
            BuildPluginEngine<MvcPluginLoader>(builder.Services, configure);
            return builder;
        }

        public static IMvcCoreBuilder AddModulEngine(this IMvcCoreBuilder builder, Action<IAppBuilder> configure = null) {
            BuildPluginEngine<MvcPluginLoader>(builder.Services, configure);
            return builder;
        }

        public static IMvcBuilder AddModulEngine<TPlugin>(this IMvcBuilder builder, Action<IAppBuilder> configure = null) where TPlugin : class, IPlugin
        {
            BuildPluginEngine<MvcPluginLoader<TPlugin>>(builder.Services, configure);
            return builder;
        }

        public static IMvcCoreBuilder AddModulEngine<TPlugin>(this IMvcCoreBuilder builder, Action<IAppBuilder> configure = null) where TPlugin : class, IPlugin
        {
            BuildPluginEngine<MvcPluginLoader<TPlugin>>(builder.Services, configure);
            return builder;
        }

        public static IMvcBuilder AddModulEngine<TPlugin, TLoader>(this IMvcBuilder builder, Action<IAppBuilder> configure = null)
            where TLoader : class, IPluginLoader where TPlugin : class, IPlugin
        {
            BuildPluginEngine<TLoader>(builder.Services, configure);
            return builder;
        }

        public static IMvcCoreBuilder AddModulEngine<TPlugin, TLoader>(this IMvcCoreBuilder builder, Action<IAppBuilder> configure = null)
            where TLoader : class, IPluginLoader where TPlugin : class, IPlugin
        {
            BuildPluginEngine<TLoader>(builder.Services, configure);
            return builder;
        }

        private static void BuildPluginEngine<TLoader>(IServiceCollection services, Action<IAppBuilder> configure = null) where TLoader : class, IPluginLoader {
            var builder = new AppBuilder(services);
            builder.Services.AddSingleton<IPluginLoader, TLoader>();
            configure?.Invoke(builder);
            builder.Build();
        }
    }
}

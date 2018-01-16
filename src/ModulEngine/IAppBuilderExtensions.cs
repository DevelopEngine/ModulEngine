using System;
using Microsoft.Extensions.DependencyInjection;
using ModulEngine.Composition;

namespace ModulEngine
{
    public static class IAppBuilderExtensions
    {
        public static IAppBuilder AddPlugin<T>(this IAppBuilder builder) where T : class, IPlugin {
            builder.Services.AddSingleton<IPlugin, T>();
            return builder;
        }

        public static IAppBuilder AddPlugin(this IAppBuilder builder, params Type[] pluginType) {
            foreach (var type in pluginType)
            {
                builder.Services.AddSingleton(typeof(IPlugin), type);
            }
            return builder;
        }

        public static IServiceCollection AddPlugin<T, TPlugin>(this IServiceCollection services) where T : class, TPlugin where TPlugin : class, IPlugin {
            services.AddSingleton<TPlugin, T>();
            return services;
        }

        public static IServiceCollection AddPlugin<TPlugin>(this IServiceCollection services, params Type[] pluginType) where TPlugin : class, IPlugin {
            foreach (var type in pluginType)
            {
                services.AddSingleton(typeof(TPlugin), type);
            }
            return services;
        }
     }
}

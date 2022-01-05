using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using McMaster.NETCore.Plugins;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ModulEngine
{

    public class PluginBuilder<TPlugin, TOptions> : PluginBuilder<TPlugin> where TPlugin : IPlugin where TOptions : IPluginOptions, new() {

        private TOptions _options {get;set;}
        public PluginBuilder<TPlugin, TOptions> UseConfiguration(IConfiguration configuration, string sectionName = "Plugins") {
            _options = configuration
                .GetSection(sectionName)
                .Get<TOptions>() ?? new TOptions();
            AddConfigSearchPaths();
            return this;
        }

        public PluginBuilder<TPlugin, TOptions> UseConfiguration(IConfigurationSection section) {
            _options = section.Get<TOptions>() ?? new TOptions();
            return this;
        }

        private void AddConfigSearchPaths() {
            var paths = _options.PluginPaths;
            if (paths.Any()) {
                foreach (var path in paths)
                {
                    AddSearchPath(path);
                }
            }
        }
    }

    public enum RegistrationType
    {
        None,
        Singleton,
        Scoped,
        Transient
    }

    public class PluginBuilder<TPlugin> where TPlugin : IPlugin{
        private Action<string> _loggerFunc;

        private List<Type> _sharedTypes {get;set;} = new List<Type>{typeof(TPlugin), typeof(IServiceCollection), typeof(ILogger), typeof(ILogger<>)};
        private List<Type> ForceLoadTypes { get; set; } = new List<Type>();
        private RegistrationType _defaultRegistrationType;

        private RegistrationType DefaultRegistrationType
        {
            get => _defaultRegistrationType;
            set
            {
                _defaultRegistrationType = value;
                Register = _defaultRegistrationType switch {
                    RegistrationType.None => (collection, type, impl) => collection,
                    RegistrationType.Singleton => (services, type, impl) => services.AddSingleton(type, impl),
                    RegistrationType.Scoped => (services, type, impl) => services.AddScoped(type, impl),
                    RegistrationType.Transient => (services, type, impl) => services.AddTransient(type, impl),
                    _ => throw new ArgumentOutOfRangeException()
                };
            }
        }

        private Func<IServiceCollection, Type, Type, IServiceCollection> Register = (collection, type, implementationType) =>
            collection.AddSingleton(type, implementationType);


        private List<string> SearchPaths {get;set;} = new List<string>();

        public PluginBuilder()
        {
            SearchPaths.AddRange(GetDefaultPaths());
        }

        private IEnumerable<string> GetDefaultPaths() {
            return new List<string> {
                Path.Combine(AppContext.BaseDirectory, "plugins"),
                Path.Combine(Environment.CurrentDirectory, "plugins")
            };
        }

        public PluginBuilder<TPlugin> AddSearchPath(string path) {
            if (!string.IsNullOrWhiteSpace(path)
                    && Directory.Exists(path) 
                    && (Directory.GetDirectories(path).Any() || Directory.GetFiles(path).Any(f => Path.GetExtension(f) == ".dll"))) {
                SearchPaths.Add(Directory.GetFiles(path).Any(f => Path.GetExtension(f) == ".dll") ? Directory.GetParent(path).FullName : path);
            }
            return this;
        }

        public PluginBuilder<TPlugin> AddSearchPaths(IEnumerable<string> paths) {
            if (paths.Any()) {
                foreach (var path in paths)
                {
                    this.AddSearchPath(path);
                }
            }
            return this;
        }

        

        public PluginBuilder<TPlugin> UseLogger(ILogger logger) {
            _loggerFunc = msg => logger.LogDebug(msg);
            return this;
        }

        public PluginBuilder<TPlugin> UseLogger(Func<ILogger> loggerFunc) {
            _loggerFunc = msg => loggerFunc().LogDebug(msg);
            return this;
        }

        public PluginBuilder<TPlugin> UseConsoleLogging() {
            _loggerFunc = msg => Console.WriteLine($"PluginLoader: {msg}");
            return this;
        }

        public PluginBuilder<TPlugin> ShareTypes(params Type[] types) {
            _sharedTypes.AddRange(types);
            return this;
        }

        public PluginBuilder<TPlugin> AlwaysLoad<TService>() {
            ForceLoadTypes.Add(typeof(TService));
            return this;
        }

        public PluginBuilder<TPlugin> AlwaysLoad(params Type[] types) {
            ForceLoadTypes.AddRange(types);
            return this;
        }

        public PluginBuilder<TPlugin> UseDefaultRegistration(RegistrationType registrationType) {
            DefaultRegistrationType = registrationType;
            return this;
        }

        private IEnumerable<PluginLoader> BuildLoaders(string pluginsDir) {
            var loaders = new List<PluginLoader>();
            // create plugin loaders
            // var pluginsDir = pluginSearchPath ?? Path.Combine(AppContext.BaseDirectory, "plugins");
            _loggerFunc?.Invoke($"Loading all plugins from {pluginsDir}");
            if (!Directory.Exists(pluginsDir)) return new List<PluginLoader>();
            foreach (var dir in Directory.GetDirectories(pluginsDir).Distinct())
            {
                var dirName = Path.GetFileName(dir);
                var pluginDll = Path.Combine(dir, dirName + ".dll");
                if (File.Exists(pluginDll))
                {
                    _loggerFunc?.Invoke($"Plugin located! Loading {pluginDll}");
                    var loader = PluginLoader.CreateFromAssemblyFile(
                        pluginDll,
                        sharedTypes: _sharedTypes.ToArray()
                    );
                    loaders.Add(loader);
                }
            }
            return loaders;
        }

        public IEnumerable<PluginLoader> BuildLoaders() {
            var loaders = SearchPaths.Select(sp => Path.IsPathRooted(sp) ? sp : Path.GetFullPath(sp) ).SelectMany(sp => BuildLoaders(sp));
            return loaders;
        }

        public IServiceCollection BuildServices(IServiceCollection services = null, IConfiguration config = null, bool disableLoaderInjection = true) {
            services ??= new ServiceCollection();
            try {
                config ??= services.BuildServiceProvider().GetService<IConfiguration>();
            }
            catch {
                //ignored
            }
            var provider = disableLoaderInjection ? new ServiceCollection() : services;
            var loaders = BuildLoaders();
            foreach (var loader in loaders)
            {
                //grab that
                var ass = loader.LoadDefaultAssembly();
                var types = ass.GetTypes();
                if (types.Any(IsPlugin)) {
                    foreach (var pluginType in types.Where(IsPlugin))
                    {
                        provider.AddSingleton(pluginType);
                    }
                } else {
                    _loggerFunc?.Invoke($"Found no compatible plugin types in {ass.FullName}");
                }

                if (ForceLoadTypes.Any()) {
                    // current loader may not contain any force-load-enabled types
                    // ReSharper disable LoopCanBeConvertedToQuery
                    foreach (var flt in ForceLoadTypes) {
                        foreach (var implType in types.Where(t => IsCompatible(flt, t))) {
                            services = Register(services, flt, implType);
                        }
                    }
                    // ReSharper restore LoopCanBeConvertedToQuery
                }
            }
            var allLoaders = provider.BuildServiceProvider().GetServices<TPlugin>();
            foreach (var dynamicLoader in allLoaders)
            {
                services = dynamicLoader?.ConfigureServices(services, config);
            }
            return services;
        }

        internal static bool IsPlugin(Type t) {
            return typeof(TPlugin).IsAssignableFrom(t) && !t.IsAbstract;
        }

        internal static bool IsCompatible(Type registration, Type implementation) {
            return registration.IsAssignableFrom(implementation) && !implementation.IsAbstract;
        }
    }
}
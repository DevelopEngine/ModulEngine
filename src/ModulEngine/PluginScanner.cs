using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ModulEngine.Composition;

namespace ModulEngine
{
    public class PluginScanner : PluginScanner<IPlugin>
    {
    }

    public class PluginScanner<TPlugin> : IPluginScanner where TPlugin : IPlugin
    {
        public IEnumerable<Type> LoadModulesFromAssemblies(IEnumerable<Assembly> assemblies)
        {
            var pluginTypes = new List<Type>();
            foreach (var assembly in assemblies)
            {
                var plugins = assembly.GetExportedTypes().Where(t => t.IsAssignableTo(typeof(TPlugin)));
                if (plugins.Any()) {
                    pluginTypes.AddRange(plugins);
                }
            }
            return pluginTypes;
        }
    }
}
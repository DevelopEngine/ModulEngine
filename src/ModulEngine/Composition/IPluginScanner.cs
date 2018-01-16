using System;
using System.Collections.Generic;
using System.Reflection;

namespace ModulEngine.Composition
{
    public interface IPluginScanner {
        IEnumerable<Type> LoadModulesFromAssemblies(IEnumerable<Assembly> assemblies);
    }
}
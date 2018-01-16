using Microsoft.Extensions.DependencyInjection;

namespace ModulEngine.Composition
{
    public interface IPlugin
    {
        void AddServices(IAppBuilder builder);
    }
}
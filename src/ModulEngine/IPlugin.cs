using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ModulEngine
{
    public interface IPlugin {
        IServiceCollection ConfigureServices(IServiceCollection services, IConfiguration configuration);
    }
}
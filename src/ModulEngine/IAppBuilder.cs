using System;
using Microsoft.Extensions.DependencyInjection;

namespace ModulEngine
{
    public interface IAppBuilder
    {
        IServiceCollection Services {get;}
        void Build();
    }
}

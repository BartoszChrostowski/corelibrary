using Autofac.Core;
using AutoMapper;
using LeanCode.Components;
using Microsoft.Extensions.DependencyInjection;

namespace LeanCode.DomainModels.EventsExecutor
{
    public class EventsExecutorComponent : IAppComponent
    {
        public Profile MapperProfile => null;
        public IModule AutofacModule { get; } = new EventsExecutorModule();

        public void ConfigureServices(IServiceCollection services)
        { }
    }
}
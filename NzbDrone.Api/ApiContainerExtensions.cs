using System.Linq;
using Autofac;
using Nancy.Bootstrapper;
using NzbDrone.Common;

namespace NzbDrone.Api
{
    public static class ApiContainerExtensions
    {
        public static void RegisterApiServices(this ContainerBuilder containerBuilder)
        {
            containerBuilder.RegisterAssemblyTypes("NzbDrone.Api");
            containerBuilder.RegisterType<NancyBootstrapper>().As<INancyBootstrapper>();
        }
    }
}
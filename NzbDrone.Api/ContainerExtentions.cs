using System.Linq;
using System.Reflection;
using Autofac;

namespace NzbDrone.Api
{
    public static class ContainerExtensions
    {
        public static void RegisterApiServices(this ContainerBuilder containerBuilder)
        {
            var apiAssembly = Assembly.Load("NzbDrone.Api");


            containerBuilder.RegisterAssemblyTypes(apiAssembly)
                .AsImplementedInterfaces()
                .SingleInstance();

            containerBuilder.RegisterAssemblyTypes(apiAssembly)
                .AsSelf()
                .SingleInstance();
        }
    }
}
using System;
using System.Linq;
using System.Reflection;
using Autofac;

namespace NzbDrone.Common
{
    public static class CommonContainerExtensions
    {
        public static void RegisterCommonServices(this ContainerBuilder containerBuilder)
        {
            containerBuilder.RegisterAssemblyTypes("NzbDrone.Common");
        }

        public static void RegisterAssemblyTypes(this ContainerBuilder containerBuilder, string assemblyName)
        {
            var apiAssembly = Assembly.Load(assemblyName);

            if (apiAssembly == null)
            {
                throw new ApplicationException("Couldn't load assembly " + assemblyName);
            }

            containerBuilder.RegisterAssemblyTypes(apiAssembly)
                            .AsImplementedInterfaces();

            containerBuilder.RegisterAssemblyTypes(apiAssembly)
                            .AsSelf();
        }
    }
}
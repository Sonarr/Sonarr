using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Autofac;
using NLog;
using NzbDrone.Common;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.ExternalNotification;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Instrumentation;
using NzbDrone.Core.Providers.Search;

namespace NzbDrone.Core
{
    public static class ContainerExtensions
    {

        private static readonly Logger logger = LogManager.GetLogger("ServiceRegistration");

        public static void RegisterCoreServices(this ContainerBuilder containerBuilder)
        {
            containerBuilder.RegisterAssembly("NzbDrone.Common");
            containerBuilder.RegisterAssembly("NzbDrone.Core");

            containerBuilder.InitDatabase();


            containerBuilder.RegisterModule<LogInjectionModule>();
        }


        private static void RegisterAssembly(this ContainerBuilder container, string assemblyName)
        {

            container.RegisterAssemblyTypes(assemblyName);

            var assembly = Assembly.Load(assemblyName);

            container.RegisterAssemblyTypes(assembly)
                     .Where(t => t.IsSubclassOf(typeof(IndexerBase)))
                     .As<IndexerBase>().SingleInstance();

            container.RegisterAssemblyTypes(assembly)
                      .Where(t => t.IsSubclassOf(typeof(SearchBase)))
                     .As<SearchBase>().SingleInstance();

            container.RegisterAssemblyTypes(assembly)
                      .Where(t => t.IsSubclassOf(typeof(ExternalNotificationBase)))
                     .As<ExternalNotificationBase>().SingleInstance();
        }

        private static void InitDatabase(this ContainerBuilder container)
        {
            logger.Info("Registering Database...");

            var appDataPath = new EnvironmentProvider().GetAppDataPath();
            if (!Directory.Exists(appDataPath)) Directory.CreateDirectory(appDataPath);



            container.Register(c =>
                      {
                          return c.Resolve<IObjectDbFactory>().Create();
                      }).As<IObjectDatabase>().SingleInstance();

            container.RegisterGeneric(typeof(BasicRepository<>)).As(typeof(IBasicRepository<>));
        }
    }
}
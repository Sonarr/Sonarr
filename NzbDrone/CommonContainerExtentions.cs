using System;
using System.IO;
using System.Linq;
using System.Reflection;
using FluentMigrator.Runner;
using NLog;
using Nancy.Bootstrapper;
using NzbDrone.Api;
using NzbDrone.Common;
using NzbDrone.Common.Eventing;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Datastore.Migration.Framework;
using NzbDrone.Core.ExternalNotification;
using NzbDrone.Core.IndexerSearch;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Lifecycle;
using NzbDrone.Core.RootFolders;
using TinyIoC;

namespace NzbDrone
{
    public static class ContainerBuilder
    {
        private static readonly Logger logger = LogManager.GetLogger("ContainerBuilder");

        public static TinyIoCContainer Instance { get; private set; }

        static ContainerBuilder()
        {
            var container = new TinyIoCContainer();

            container.AutoRegisterInterfaces("NzbDrone");
            container.AutoRegisterInterfaces("NzbDrone.Common");
            container.AutoRegisterInterfaces("NzbDrone.Core");
            container.AutoRegisterInterfaces("NzbDrone.Api");

            container.AutoRegisterImplementations<IIndexerBase>();
            container.AutoRegisterImplementations<ExternalNotificationBase>();
            container.AutoRegisterMultipleImplementations<IInitializable>();

            container.Register<IEventAggregator, EventAggregator>().AsSingleton();
            container.Register<INancyBootstrapper, TinyNancyBootstrapper>().AsSingleton();
            container.Register<IAnnouncer, MigrationLogger>().AsSingleton();

            container.Register(typeof(IBasicRepository<RootFolder>), typeof(BasicRepository<RootFolder>)).AsMultiInstance();

            container.InitDatabase();

            Instance = container;
        }

        private static void InitDatabase(this TinyIoCContainer container)
        {
            logger.Info("Registering Database...");

            //TODO: move this to factory
            var environmentProvider = new EnvironmentProvider();
            var appDataPath = environmentProvider.GetAppDataPath();
            if (!Directory.Exists(appDataPath)) Directory.CreateDirectory(appDataPath);

            container.Register(
                delegate(TinyIoCContainer c, NamedParameterOverloads p)
                {
                    return c.Resolve<IDbFactory>().Create(environmentProvider.GetNzbDroneDatabase());
                });
        }

        private static void AutoRegisterInterfaces(this TinyIoCContainer container, string assemblyName)
        {
            var assembly = Assembly.Load(assemblyName);

            if (assembly == null)
            {
                throw new ApplicationException("Couldn't load assembly " + assemblyName);
            }

            var interfaces = assembly.GetInterfaces().Where(c => !c.FullName.StartsWith("Nancy."));

            foreach (var contract in interfaces)
            {
                container.AutoRegisterImplementations(contract);
            }
        }

        private static void AutoRegisterImplementations<TContract>(this TinyIoCContainer container)
        {
            container.AutoRegisterImplementations(typeof(TContract));
        }

        private static void AutoRegisterImplementations(this TinyIoCContainer container, Type contractType)
        {
            var implementations = contractType.Assembly.GetImplementations(contractType).ToList();

            foreach(var implementation in implementations)
            {
                logger.Trace("Registering {0} as {1}", implementation.Name, contractType.Name);
                container.Register(contractType, implementation).AsMultiInstance();
            }
        }

        private static void AutoRegisterMultipleImplementations<TContract>(this TinyIoCContainer container)
        {
            container.AutoRegisterMultipleImplementations(typeof(TContract));
        }

        private static void AutoRegisterMultipleImplementations(this TinyIoCContainer container, Type contractType)
        {
            var implementations = contractType.Assembly.GetImplementations(contractType);
            container.RegisterMultiple(contractType, implementations).AsMultiInstance();
        }
    }
}
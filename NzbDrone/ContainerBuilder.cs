using System;
using System.Collections.Generic;
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
using NzbDrone.Core.Organizer;
using NzbDrone.Core.RootFolders;
using TinyIoC;

namespace NzbDrone
{
    public static class ContainerBuilder
    {
        private static readonly Logger Logger = LogManager.GetLogger("ContainerBuilder");
        public static TinyIoCContainer Instance { get; private set; }
        private static readonly List<Type> NzbDroneTypes;

        static ContainerBuilder()
        {
            var container = new TinyIoCContainer();

            NzbDroneTypes = new List<Type>();
            NzbDroneTypes.AddRange(Assembly.Load("NzbDrone").GetTypes());
            NzbDroneTypes.AddRange(Assembly.Load("NzbDrone.Common").GetTypes());
            NzbDroneTypes.AddRange(Assembly.Load("NzbDrone.Core").GetTypes());
            NzbDroneTypes.AddRange(Assembly.Load("NzbDrone.Api").GetTypes());

            container.AutoRegisterInterfaces();

            container.AutoRegisterImplementations<ExternalNotificationBase>();

            container.Register<IEventAggregator, EventAggregator>().AsSingleton();
            container.Register<INancyBootstrapper, TinyNancyBootstrapper>().AsSingleton();
            container.Register<IAnnouncer, MigrationLogger>().AsSingleton();
            container.Register<Router>().AsSingleton();

            container.Register(typeof(IBasicRepository<RootFolder>), typeof(BasicRepository<RootFolder>)).AsMultiInstance();
            container.Register(typeof(IBasicRepository<NameSpecification>), typeof(BasicRepository<NameSpecification>)).AsMultiInstance();

            container.InitDatabase();

            Instance = container;
        }

        private static void InitDatabase(this TinyIoCContainer container)
        {
            Logger.Info("Registering Database...");

            //TODO: move this to factory
            var environmentProvider = new EnvironmentProvider();
            var appDataPath = environmentProvider.GetAppDataPath();
            
            if (!Directory.Exists(appDataPath))
            {
                Directory.CreateDirectory(appDataPath);
            }

            container.Register((c, p) => c.Resolve<IDbFactory>().Create(environmentProvider.GetNzbDroneDatabase()));
        }

        private static void AutoRegisterInterfaces(this TinyIoCContainer container)
        {
            var interfaces = NzbDroneTypes.Where(t => t.IsInterface);

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
            var implementations = GetImplementations(contractType).ToList();

            if (implementations.Count == 0)
            {
                return;
            }
            if (implementations.Count == 1)
            {
                container.Register(contractType, implementations.Single()).AsMultiInstance();
            }
            else
            {
                container.RegisterMultiple(contractType, implementations).AsMultiInstance();
            }
        }

        private static IEnumerable<Type> GetImplementations(Type contractType)
        {
            return NzbDroneTypes
                    .Where(implementation =>
                    contractType.IsAssignableFrom(implementation) &&
                    !implementation.IsInterface &&
                    !implementation.IsAbstract
                );
        }
    }
}
using System.IO;
using FluentMigrator.Runner;
using NLog;
using Nancy.Bootstrapper;
using NzbDrone.Api;
using NzbDrone.Common;
using NzbDrone.Common.Messaging;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Datastore.Migration.Framework;
using NzbDrone.Core.ExternalNotification;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.RootFolders;
using TinyIoC;

namespace NzbDrone
{
    public class MainAppContainerBuilder : ContainerBuilderBase
    {
        private static readonly Logger Logger = LogManager.GetLogger("ContainerBuilderBase");

        public static TinyIoCContainer BuildContainer()
        {
            return new MainAppContainerBuilder().Container;
        }


        private MainAppContainerBuilder()
            : base("NzbDrone", "NzbDrone.Common", "NzbDrone.Core", "NzbDrone.Api")
        {
            AutoRegisterImplementations<ExternalNotificationBase>();

            Container.Register<IMessageAggregator, MessageAggregator>().AsSingleton();
            Container.Register<INancyBootstrapper, NancyBootstrapper>().AsSingleton();
            Container.Register<IAnnouncer, MigrationLogger>().AsSingleton();
            Container.Register<Router>().AsSingleton();

            Container.Register(typeof(IBasicRepository<RootFolder>), typeof(BasicRepository<RootFolder>)).AsMultiInstance();
            Container.Register(typeof(IBasicRepository<NamingConfig>), typeof(BasicRepository<NamingConfig>)).AsMultiInstance();

            InitDatabase();

            ReportingService.RestProvider = Container.Resolve<RestProvider>();
        }

        private void InitDatabase()
        {
            Logger.Info("Registering Database...");

            //TODO: move this to factory
            var environmentProvider = new EnvironmentProvider();
            var appDataPath = environmentProvider.GetAppDataPath();

            if (!Directory.Exists(appDataPath))
            {
                Directory.CreateDirectory(appDataPath);
            }

            Container.Register((c, p) => c.Resolve<IDbFactory>().Create(environmentProvider.GetNzbDroneDatabase()));
        }
    }
}
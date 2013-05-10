using System.IO;
using NLog;
using Nancy.Bootstrapper;
using NzbDrone.Api;
using NzbDrone.Api.SignalR;
using NzbDrone.Common;
using NzbDrone.Common.Composition;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.ExternalNotification;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.RootFolders;

namespace NzbDrone
{
    public class MainAppContainerBuilder : ContainerBuilderBase
    {
        private static readonly Logger Logger = LogManager.GetLogger("ContainerBuilderBase");

        public static IContainer BuildContainer()
        {
            return new MainAppContainerBuilder().Container;
        }


        private MainAppContainerBuilder()
            : base("NzbDrone", "NzbDrone.Common", "NzbDrone.Core", "NzbDrone.Api")
        {
            AutoRegisterImplementations<ExternalNotificationBase>();
            AutoRegisterImplementations<NzbDronePersistentConnection>();

            Container.Register(typeof(IBasicRepository<RootFolder>), typeof(BasicRepository<RootFolder>));
            Container.Register(typeof(IBasicRepository<NamingConfig>), typeof(BasicRepository<NamingConfig>));

            Container.Register<INancyBootstrapper, NancyBootstrapper>();

            InitDatabase();



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

            Container.Register(c => c.Resolve<IDbFactory>().Create(environmentProvider.GetNzbDroneDatabase()));
        }
    }
}
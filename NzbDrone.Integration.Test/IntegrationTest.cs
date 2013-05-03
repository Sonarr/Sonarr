using System;
using System.IO;
using NLog;
using NLog.Config;
using NLog.Targets;
using NUnit.Framework;
using Nancy.Hosting.Self;
using NzbDrone.Api;
using NzbDrone.Api.Commands;
using NzbDrone.Api.Indexers;
using NzbDrone.Api.RootFolders;
using NzbDrone.Common;
using NzbDrone.Core.Datastore;
using NzbDrone.Integration.Test.Client;
using RestSharp;
using TinyIoC;

namespace NzbDrone.Integration.Test
{
    [TestFixture]
    public abstract class IntegrationTest
    {
        private NancyBootstrapper _bootstrapper;
        private NancyHost _host;
        protected RestClient RestClient { get; private set; }

        private static readonly Logger Logger = LogManager.GetLogger("TEST");

        protected TinyIoCContainer Container { get; private set; }


        protected SeriesClient Series;
        protected ClientBase<RootFolderResource> RootFolders;
        protected ClientBase<CommandResource> Commands;
        protected ReleaseClient Releases;
        protected IndexerClient Indexers;

        static IntegrationTest()
        {
            if (LogManager.Configuration == null || LogManager.Configuration is XmlLoggingConfiguration)
            {
                LogManager.Configuration = new LoggingConfiguration();
                var consoleTarget = new ConsoleTarget { Layout = "${time} - ${logger} - ${message} ${exception}" };
                LogManager.Configuration.AddTarget(consoleTarget.GetType().Name, consoleTarget);
                LogManager.Configuration.LoggingRules.Add(new LoggingRule("*", LogLevel.Trace, consoleTarget));
            }


            LogManager.ReconfigExistingLoggers();
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

            var dbPath = Path.Combine(environmentProvider.WorkingDirectory, DateTime.Now.Ticks + ".db");


            Logger.Info("Working Folder: {0}", environmentProvider.WorkingDirectory);
            Logger.Info("Data Folder: {0}", environmentProvider.GetAppDataPath());
            Logger.Info("DB Na: {0}", dbPath);


            Container.Register((c, p) => c.Resolve<IDbFactory>().Create(dbPath));
        }

        [SetUp]
        public void SmokeTestSetup()
        {
            Container = MainAppContainerBuilder.BuildContainer();

            InitDatabase();

            _bootstrapper = new NancyBootstrapper(Container);

            const string url = "http://localhost:1313";

            _host = new NancyHost(new Uri(url), _bootstrapper);

            RestClient = new RestClient(url + "/api/");
            Series = new SeriesClient(RestClient);
            Releases = new ReleaseClient(RestClient);
            RootFolders = new ClientBase<RootFolderResource>(RestClient);
            Commands = new ClientBase<CommandResource>(RestClient);
            Indexers = new IndexerClient(RestClient);

            _host.Start();
        }

        [TearDown]
        public void SmokeTestTearDown()
        {
            _host.Stop();

            _bootstrapper.Shutdown();
        }
    }

}

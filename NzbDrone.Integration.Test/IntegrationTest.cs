using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Moq;
using NLog;
using NLog.Config;
using NLog.Targets;
using NUnit.Framework;
using NzbDrone.Api;
using NzbDrone.Api.Commands;
using NzbDrone.Api.RootFolders;
using NzbDrone.Common;
using NzbDrone.Common.Composition;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Jobs;
using NzbDrone.Integration.Test.Client;
using NzbDrone.Owin;
using NzbDrone.Owin.MiddleWare;
using NzbDrone.Test.Common.Categories;
using RestSharp;

namespace NzbDrone.Integration.Test
{
    [TestFixture]
    [IntegrationTest]
    public abstract class IntegrationTest
    {
        private NancyBootstrapper _bootstrapper;
        private IHostController _hostController;
        protected RestClient RestClient { get; private set; }

        private static readonly Logger Logger = LogManager.GetLogger("TEST");

        protected IContainer Container { get; private set; }


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


            Container.Register(c => c.Resolve<IDbFactory>().Create(dbPath));
        }

        [SetUp]
        public void SmokeTestSetup()
        {
            Container = MainAppContainerBuilder.BuildContainer();

            InitDatabase();

            var taskManagerMock = new Mock<ITaskManager>();
            taskManagerMock.Setup(c => c.GetPending()).Returns(new List<ScheduledTask>());

            Container.TinyContainer.Register(taskManagerMock.Object);

            _bootstrapper = new NancyBootstrapper(Container.TinyContainer);


            var _hostConfig = new Mock<IConfigFileProvider>();
            _hostConfig.SetupGet(c => c.Port).Returns(1313);

            _hostController = new OwinHostController(_hostConfig.Object, new[] { new NancyMiddleWare(_bootstrapper) }, Logger);


            RestClient = new RestClient(_hostController.AppUrl + "/api/");
            Series = new SeriesClient(RestClient);
            Releases = new ReleaseClient(RestClient);
            RootFolders = new ClientBase<RootFolderResource>(RestClient);
            Commands = new ClientBase<CommandResource>(RestClient);
            Indexers = new IndexerClient(RestClient);

            _hostController.StartServer();
        }

        [TearDown]
        public void SmokeTestTearDown()
        {
            _hostController.StopServer();

            _bootstrapper.Shutdown();
        }
    }

}

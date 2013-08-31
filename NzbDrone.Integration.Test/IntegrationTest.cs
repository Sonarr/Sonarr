using System.Runtime.CompilerServices;
using NLog;
using NLog.Config;
using NLog.Targets;
using NUnit.Framework;
using NzbDrone.Api.Commands;
using NzbDrone.Api.Config;
using NzbDrone.Api.RootFolders;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Integration.Test.Client;
using NzbDrone.Test.Common.Categories;
using RestSharp;

namespace NzbDrone.Integration.Test
{
    [TestFixture]
    [IntegrationTest]
    public abstract class IntegrationTest
    {
        protected RestClient RestClient { get; private set; }

        protected SeriesClient Series;
        protected ClientBase<RootFolderResource> RootFolders;
        protected ClientBase<CommandResource> Commands;
        protected ReleaseClient Releases;
        protected IndexerClient Indexers;
        protected EpisodeClient Episodes;
        protected SeasonClient Seasons;
        protected ClientBase<NamingConfigResource> NamingConfig;

        private NzbDroneRunner _runner;

        public IntegrationTest()
        {
            new StartupArguments();

            LogManager.Configuration = new LoggingConfiguration();
            var consoleTarget = new ConsoleTarget { Layout = "${level}: ${message} ${exception}" };
            LogManager.Configuration.AddTarget(consoleTarget.GetType().Name, consoleTarget);
            LogManager.Configuration.LoggingRules.Add(new LoggingRule("*", LogLevel.Trace, consoleTarget));
        }

        [TestFixtureSetUp]
        public void SmokeTestSetup()
        {
            _runner = new NzbDroneRunner();
            _runner.KillAll();

            InitRestClients();

            _runner.Start();
        }

        private void InitRestClients()
        {
            RestClient = new RestClient("http://localhost:8989/api");
            Series = new SeriesClient(RestClient);
            Releases = new ReleaseClient(RestClient);
            RootFolders = new ClientBase<RootFolderResource>(RestClient);
            Commands = new ClientBase<CommandResource>(RestClient);
            Indexers = new IndexerClient(RestClient);
            Episodes = new EpisodeClient(RestClient);
            Seasons = new SeasonClient(RestClient);
            NamingConfig = new ClientBase<NamingConfigResource>(RestClient, "config/naming");
        }

        [TestFixtureTearDown]
        public void SmokeTestTearDown()
        {
            _runner.KillAll();
        }
    }

}

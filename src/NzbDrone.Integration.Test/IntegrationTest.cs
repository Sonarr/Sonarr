using NLog;
using NLog.Config;
using NLog.Targets;
using NUnit.Framework;
using NzbDrone.Api.Commands;
using NzbDrone.Api.Config;
using NzbDrone.Api.History;
using NzbDrone.Api.RootFolders;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Integration.Test.Client;
using NzbDrone.Test.Common;
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
        protected ClientBase<HistoryResource> History;
        protected IndexerClient Indexers;
        protected EpisodeClient Episodes;
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

        //[TestFixtureSetUp]
        [SetUp]
        public void SmokeTestSetup()
        {
            _runner = new NzbDroneRunner();
            _runner.KillAll();

            _runner.Start();
            InitRestClients();
        }

        private void InitRestClients()
        {
            RestClient = new RestClient("http://localhost:8989/api");
            Series = new SeriesClient(RestClient, _runner.ApiKey);
            Releases = new ReleaseClient(RestClient, _runner.ApiKey);
            RootFolders = new ClientBase<RootFolderResource>(RestClient, _runner.ApiKey);
            Commands = new ClientBase<CommandResource>(RestClient, _runner.ApiKey);
            History = new ClientBase<HistoryResource>(RestClient, _runner.ApiKey);
            Indexers = new IndexerClient(RestClient, _runner.ApiKey);
            Episodes = new EpisodeClient(RestClient, _runner.ApiKey);
            NamingConfig = new ClientBase<NamingConfigResource>(RestClient, _runner.ApiKey, "config/naming");
        }

        //[TestFixtureTearDown]
        [TearDown]
        public void SmokeTestTearDown()
        {
            _runner.KillAll();
        }
    }
}

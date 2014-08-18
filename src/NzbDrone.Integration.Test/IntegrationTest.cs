using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.AspNet.SignalR.Client;
using Microsoft.AspNet.SignalR.Client.Transports;
using NLog;
using NLog.Config;
using NLog.Targets;
using NUnit.Framework;
using NzbDrone.Api.Commands;
using NzbDrone.Api.Config;
using NzbDrone.Api.History;
using NzbDrone.Api.RootFolders;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Serializer;
using NzbDrone.Integration.Test.Client;
using NzbDrone.SignalR;
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
        protected NotificationClient Notifications;

        private NzbDroneRunner _runner;
        private List<SignalRMessage> _signalRReceived;
        private Connection _signalrConnection;

        protected IEnumerable<SignalRMessage> SignalRMessages
        {
            get
            {
                return _signalRReceived;
            }
        }

        public IntegrationTest()
        {
            new StartupContext();

            LogManager.Configuration = new LoggingConfiguration();
            var consoleTarget = new ConsoleTarget { Layout = "${level}: ${message} ${exception}" };
            LogManager.Configuration.AddTarget(consoleTarget.GetType().Name, consoleTarget);
            LogManager.Configuration.LoggingRules.Add(new LoggingRule("*", LogLevel.Trace, consoleTarget));
        }

        [TestFixtureSetUp]
        //[SetUp]
        public void SmokeTestSetup()
        {
            _runner = new NzbDroneRunner();
            _runner.KillAll();

            _runner.Start();
            InitRestClients();

            // Add Wombles
            var wombles = Indexers.Post(new Api.Indexers.IndexerResource
            {
                EnableRss = true,
                ConfigContract = "NullConfig",
                Implementation = "Wombles",
                Name = "Wombles",
                Protocol = Core.Indexers.DownloadProtocol.Usenet,
                Fields = new List<Api.ClientSchema.Field>()
            });
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
            Notifications = new NotificationClient(RestClient, _runner.ApiKey);
        }

        [TestFixtureTearDown]
        //[TearDown]
        public void SmokeTestTearDown()
        {
            _runner.KillAll();
        }

        [TearDown]
        public void IntegrationSetup()
        {
            if (_signalrConnection != null)
            {
                switch (_signalrConnection.State)
                {
                    case ConnectionState.Connected:
                    case ConnectionState.Connecting:
                        {
                            _signalrConnection.Stop();
                            break;
                        }
                }

                _signalrConnection = null;
                _signalRReceived = new List<SignalRMessage>();
            }
        }

        protected void ConnectSignalR()
        {
            _signalRReceived = new List<SignalRMessage>();
            _signalrConnection = new Connection("http://localhost:8989/signalr");
            _signalrConnection.Start(new LongPollingTransport()).ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    Assert.Fail("SignalrConnection failed. {0}", task.Exception.GetBaseException());
                }
            });

            var retryCount = 0;

            while (_signalrConnection.State != ConnectionState.Connected)
            {
                if (retryCount > 25)
                {
                    Assert.Fail("Couldn't establish signalr connection. State: {0}", _signalrConnection.State);
                }

                retryCount++;
                Console.WriteLine("Connecting to signalR" + _signalrConnection.State);
                Thread.Sleep(200);
            }

            _signalrConnection.Received += json => _signalRReceived.Add(Json.Deserialize<SignalRMessage>(json)); ;
        }

    }
}

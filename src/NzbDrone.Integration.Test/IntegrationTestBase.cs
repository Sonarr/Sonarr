using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.AspNet.SignalR.Client;
using Microsoft.AspNet.SignalR.Client.Transports;
using NLog;
using NLog.Config;
using NLog.Targets;
using NUnit.Framework;
using NzbDrone.Api.Blacklist;
using NzbDrone.Api.Commands;
using NzbDrone.Api.Config;
using NzbDrone.Api.Episodes;
using NzbDrone.Api.History;
using NzbDrone.Api.RootFolders;
using NzbDrone.Api.Series;
using NzbDrone.Api.Tags;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Serializer;
using NzbDrone.Integration.Test.Client;
using NzbDrone.SignalR;
using NzbDrone.Test.Common;
using NzbDrone.Test.Common.Categories;
using RestSharp;

namespace NzbDrone.Integration.Test
{
    [IntegrationTest]
    public abstract class IntegrationTestBase
    {
        protected RestClient RestClient { get; private set; }

        public ClientBase<BlacklistResource> Blacklist;
        public ClientBase<CommandResource> Commands;
        public DownloadClientClient DownloadClients;
        public EpisodeClient Episodes;
        public ClientBase<HistoryResource> History;
        public IndexerClient Indexers;
        public ClientBase<NamingConfigResource> NamingConfig;
        public NotificationClient Notifications;
        public ReleaseClient Releases;
        public ClientBase<RootFolderResource> RootFolders;
        public SeriesClient Series;
        public ClientBase<TagResource> Tags;

        private List<SignalRMessage> _signalRReceived;
        private Connection _signalrConnection;

        protected IEnumerable<SignalRMessage> SignalRMessages
        {
            get
            {
                return _signalRReceived;
            }
        }

        public IntegrationTestBase()
        {
            new StartupContext();

            LogManager.Configuration = new LoggingConfiguration();
            var consoleTarget = new ConsoleTarget { Layout = "${level}: ${message} ${exception}" };
            LogManager.Configuration.AddTarget(consoleTarget.GetType().Name, consoleTarget);
            LogManager.Configuration.LoggingRules.Add(new LoggingRule("*", LogLevel.Trace, consoleTarget));
        }

        public string TempDirectory { get; private set; }

        public abstract string SeriesRootFolder { get; }

        protected abstract string RootUrl { get; }

        protected abstract string ApiKey { get; }

        protected abstract void StartTestTarget();

        protected abstract void InitializeTestTarget();

        protected abstract void StopTestTarget();

        [OneTimeSetUp]
        public void SmokeTestSetup()
        {
            StartTestTarget();
            InitRestClients();
            InitializeTestTarget();
        }

        protected virtual void InitRestClients()
        {
            RestClient = new RestClient(RootUrl + "api/");
            RestClient.AddDefaultHeader("Authentication", ApiKey);
            RestClient.AddDefaultHeader("X-Api-Key", ApiKey);

            Blacklist = new ClientBase<BlacklistResource>(RestClient, ApiKey);
            Commands = new CommandClient(RestClient, ApiKey);
            DownloadClients = new DownloadClientClient(RestClient, ApiKey);
            Episodes = new EpisodeClient(RestClient, ApiKey);
            History = new ClientBase<HistoryResource>(RestClient, ApiKey);
            Indexers = new IndexerClient(RestClient, ApiKey);
            NamingConfig = new ClientBase<NamingConfigResource>(RestClient, ApiKey, "config/naming");
            Notifications = new NotificationClient(RestClient, ApiKey);
            Releases = new ReleaseClient(RestClient, ApiKey);
            RootFolders = new ClientBase<RootFolderResource>(RestClient, ApiKey);
            Series = new SeriesClient(RestClient, ApiKey);
            Tags = new ClientBase<TagResource>(RestClient, ApiKey);
        }

        [OneTimeTearDown]
        public void SmokeTestTearDown()
        {
            StopTestTarget();
        }

        [SetUp]
        public void IntegrationSetUp()
        {
            TempDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, "_test_" + DateTime.UtcNow.Ticks);
        }

        [TearDown]
        public void IntegrationTearDown()
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

        public string GetTempDirectory(params string[] args)
        {
            var path = Path.Combine(TempDirectory, Path.Combine(args));

            Directory.CreateDirectory(path);

            return path;
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

        public static void WaitForCompletion(Func<bool> predicate, int timeout = 10000, int interval = 500)
        {
            var count = timeout / interval;
            for (var i = 0; i < count; i++)
            {
                if (predicate())
                    return;

                Thread.Sleep(interval);
            }

            if (predicate())
                return;

            Assert.Fail("Timed on wait");
        }

        public SeriesResource EnsureSeries(int tvdbId, string seriesTitle, bool? monitored = null)
        {
            var result = Series.All().FirstOrDefault(v => v.TvdbId == tvdbId);

            if (result == null)
            {
                var lookup = Series.Lookup("tvdb:" + tvdbId);
                var series = lookup.First();
                series.ProfileId = 1;
                series.Path = Path.Combine(SeriesRootFolder, series.Title);
                series.Monitored = true;
                series.Seasons.ForEach(v => v.Monitored = true);
                series.AddOptions = new Core.Tv.AddSeriesOptions();
                Directory.CreateDirectory(series.Path);

                result = Series.Post(series);

                WaitForCompletion(() => Episodes.GetEpisodesInSeries(result.Id).Count > 0);
            }

            if (monitored.HasValue)
            {
                var changed = false;
                if (result.Monitored != monitored.Value)
                {
                    result.Monitored = monitored.Value;
                    changed = true;
                }

                result.Seasons.ForEach(season =>
                {
                    if (season.Monitored != monitored.Value)
                    {
                        season.Monitored = monitored.Value;
                        changed = true;
                    }
                });

                if (changed)
                {
                    Series.Put(result);
                }
            }

            return result;
        }

        public void EnsureNoSeries(int tvdbId, string seriesTitle)
        {
            var result = Series.All().FirstOrDefault(v => v.TvdbId == tvdbId);

            if (result != null)
            {
                Series.Delete(result.Id);
            }
        }

        public TagResource EnsureTag(string tagLabel)
        {
            var tag = Tags.All().FirstOrDefault(v => v.Label == tagLabel);

            if (tag == null)
            {
                tag = Tags.Post(new TagResource { Label = tagLabel });
            }

            return tag;
        }
    }
}

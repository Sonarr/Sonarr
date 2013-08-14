using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using NUnit.Framework;
using NzbDrone.Api.Commands;
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

        private NzbDroneRunner _runner;

        [SetUp]
        public void SmokeTestSetup()
        {
            new StartupArguments();

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
        }

        [TearDown]
        public void SmokeTestTearDown()
        {
            _runner.KillAll();
        }
    }

}

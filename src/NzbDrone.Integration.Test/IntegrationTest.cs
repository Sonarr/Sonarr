using System;
using System.Linq;
using System.Threading;
using NLog;
using NUnit.Framework;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Datastore.Migration.Framework;
using NzbDrone.Core.Indexers.Newznab;
using NzbDrone.Test.Common;
using NzbDrone.Test.Common.Datastore;

namespace NzbDrone.Integration.Test
{
    [Parallelizable(ParallelScope.Fixtures)]
    public abstract class IntegrationTest : IntegrationTestBase
    {
        protected static int StaticPort = 8989;

        protected NzbDroneRunner _runner;

        public override string SeriesRootFolder => GetTempDirectory("SeriesRootFolder");

        protected int Port { get; private set; }

        protected PostgresOptions PostgresOptions { get; set; } = new ();

        protected override string RootUrl => $"http://localhost:{Port}/";

        protected override string ApiKey => _runner.ApiKey;

        protected override void StartTestTarget()
        {
            Port = Interlocked.Increment(ref StaticPort);

            PostgresOptions = PostgresDatabase.GetTestOptions();

            if (PostgresOptions?.Host != null)
            {
                CreatePostgresDb(PostgresOptions);
            }

            _runner = new NzbDroneRunner(LogManager.GetCurrentClassLogger(), PostgresOptions, Port);
            _runner.Kill();

            _runner.Start();
        }

        protected override void InitializeTestTarget()
        {
            // Make sure tasks have been initialized so the config put below doesn't cause errors
            WaitForCompletion(() => Tasks.All().SelectList(x => x.TaskName).Contains("RssSync"));

            var indexer = Indexers.Schema().FirstOrDefault(i => i.Implementation == nameof(Newznab));

            if (indexer == null)
            {
                throw new NullReferenceException("Expected valid indexer schema, found null");
            }

            indexer.EnableRss = false;
            indexer.EnableInteractiveSearch = false;
            indexer.EnableAutomaticSearch = false;
            indexer.ConfigContract = nameof(NewznabSettings);
            indexer.Implementation = nameof(Newznab);
            indexer.Name = "NewznabTest";
            indexer.Protocol = Core.Indexers.DownloadProtocol.Usenet;

            // Change Console Log Level to Debug so we get more details.
            var config = HostConfig.Get(1);
            config.ConsoleLogLevel = "Debug";
            HostConfig.Put(config);
        }

        protected override void StopTestTarget()
        {
            _runner.Kill();
            if (PostgresOptions?.Host != null)
            {
                DropPostgresDb(PostgresOptions);
            }
        }

        private static void CreatePostgresDb(PostgresOptions options)
        {
            PostgresDatabase.Create(options, MigrationType.Main);
            PostgresDatabase.Create(options, MigrationType.Log);
        }

        private static void DropPostgresDb(PostgresOptions options)
        {
            PostgresDatabase.Drop(options, MigrationType.Main);
            PostgresDatabase.Drop(options, MigrationType.Log);
        }
    }
}

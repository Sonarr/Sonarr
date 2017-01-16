using System.Collections.Generic;
using NLog;
using NzbDrone.Test.Common;

namespace NzbDrone.Integration.Test
{
    public abstract class IntegrationTest : IntegrationTestBase
    {
        protected NzbDroneRunner _runner;

        public override string SeriesRootFolder => GetTempDirectory("SeriesRootFolder");

        protected override string RootUrl => "http://localhost:8989/";

        protected override string ApiKey => _runner.ApiKey;

        protected override void StartTestTarget()
        {
            _runner = new NzbDroneRunner(LogManager.GetCurrentClassLogger());
            _runner.KillAll();

            _runner.Start();
        }

        protected override void InitializeTestTarget()
        {
            // Add Wombles
            Indexers.Post(new Api.Indexers.IndexerResource
            {
                EnableRss = true,
                ConfigContract = "NullConfig",
                Implementation = "Wombles",
                Name = "Wombles",
                Protocol = Core.Indexers.DownloadProtocol.Usenet,
                Fields = new List<Api.ClientSchema.Field>()
            });
        }

        protected override void StopTestTarget()
        {
            _runner.KillAll();
        }
    }
}

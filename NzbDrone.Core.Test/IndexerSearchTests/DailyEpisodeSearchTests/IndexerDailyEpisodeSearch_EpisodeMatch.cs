using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.IndexerSearch;
using NzbDrone.Core.Tv;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.IndexerSearchTests.DailyEpisodeSearchTests
{
    [TestFixture]
    public class IndexerDailyEpisodeSearch_EpisodeMatch : IndexerSearchTestBase<DailyEpisodeSearch>
    {
        [Test]
        public void should_fetch_results_from_indexers()
        {
            WithValidIndexers();


            Subject.PerformSearch(_series, new List<Episode> { _episode }, notification)
            .Should()
            .HaveCount(20);
        }

        [Test]
        public void should_log_error_when_fetching_from_indexer_fails()
        {
            WithBrokenIndexers();

            Mocker.Resolve<DailyEpisodeSearch>()
                  .PerformSearch(_series, new List<Episode> { _episode }, notification)
                  .Should()
                  .HaveCount(0);

            ExceptionVerification.ExpectedErrors(2);
        }
    }
}

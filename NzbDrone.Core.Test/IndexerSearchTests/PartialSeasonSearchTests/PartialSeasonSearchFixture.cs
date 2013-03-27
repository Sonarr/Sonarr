using System.Collections.Generic;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.IndexerSearch;
using NzbDrone.Core.Tv;
using NzbDrone.Test.Common;
using System.Linq;

namespace NzbDrone.Core.Test.IndexerSearchTests.PartialSeasonSearchTests
{
    [TestFixture]
    public class PartialSeasonSearchFixture : IndexerSearchTestBase<PartialSeasonSearch>
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

            Subject.PerformSearch(_series, new List<Episode> { _episode }, notification)
                  .Should()
                  .HaveCount(0);

            ExceptionVerification.ExpectedErrors(2);
        }

        [Test]
        public void should_hit_each_indexer_once_for_each_prefix()
        {
            WithValidIndexers();


            var episodes = Builder<Episode>.CreateListOfSize(4)
                .All()
                .With(c => c.SeasonNumber = 1)
                .Build();

            episodes[0].EpisodeNumber = 1;
            episodes[1].EpisodeNumber = 5;
            episodes[2].EpisodeNumber = 10;
            episodes[3].EpisodeNumber = 15;


            Subject.PerformSearch(_series, episodes.ToList(), notification)
                   .Should()
                   .HaveCount(40);

            _indexer1.Verify(v => v.FetchPartialSeason(_series.Title, 1, 0), Times.Once());
            _indexer1.Verify(v => v.FetchPartialSeason(_series.Title, 1, 1), Times.Once());
            _indexer2.Verify(v => v.FetchPartialSeason(_series.Title, 1, 0), Times.Once());
            _indexer2.Verify(v => v.FetchPartialSeason(_series.Title, 1, 1), Times.Once());

            _indexer1.Verify(v => v.FetchPartialSeason(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()), Times.Exactly(2));
            _indexer2.Verify(v => v.FetchPartialSeason(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()), Times.Exactly(2));
        }
    }
}

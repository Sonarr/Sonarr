using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Indexers.HDBits;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Test.IndexerTests.HDBitsTests
{
    public class HDBitsRequestGeneratorFixture : CoreTest<HDBitsRequestGenerator>
    {
        private Series _series;

        [SetUp]
        public void SetUp()
        {
            Subject.Settings = new HDBitsSettings
            {
                BaseUrl = "https://hdbits.org/",
                ApiKey = "abc"
            };

            _series = new Series { TvdbId = 433335 };
        }

        private SingleEpisodeSearchCriteria SingleEpisode(int seasonNumber, params int[] episodeNumbers)
        {
            return new SingleEpisodeSearchCriteria
            {
                Series = _series,
                SceneTitles = ["Series Title"],
                SeasonNumber = seasonNumber,
                EpisodeNumber = episodeNumbers.First(),
                Episodes = episodeNumbers
                    .Select(e => new Episode { SeasonNumber = seasonNumber, EpisodeNumber = e })
                    .ToList()
            };
        }

        private static List<string> QueriedNames(IndexerPageableRequestChain chain, int tier)
        {
            return chain.GetTier(tier)
                .Select(pageable => pageable.First().HttpRequest.ContentSummary)
                .ToList();
        }

        [Test]
        public void should_not_search_without_a_tvdb_id()
        {
            _series.TvdbId = 0;

            var results = Subject.GetSearchRequests(SingleEpisode(1, 1));

            results.GetAllTiers().Should().BeEmpty();
        }

        [Test]
        public void should_search_each_season_of_a_multi_season_search()
        {
            var criteria = new SeasonSearchCriteria
            {
                Series = _series,
                SceneTitles = ["Series Title"],
                SeasonNumber = 1,
                Episodes =
                [
                    new Episode { SeasonNumber = 1, EpisodeNumber = 1 },
                    new Episode { SeasonNumber = 2, EpisodeNumber = 1 }
                ]
            };

            var names = QueriedNames(Subject.GetSearchRequests(criteria), 0);

            names.Should().HaveCount(2);
            names[0].Should().Contain(@"""season"":1}");
            names[1].Should().Contain(@"""season"":2}");
        }
    }
}

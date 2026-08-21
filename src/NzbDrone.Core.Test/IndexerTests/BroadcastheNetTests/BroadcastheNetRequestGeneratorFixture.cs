using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Indexers.BroadcastheNet;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Test.IndexerTests.BroadcastheNetTests
{
    public class BroadcastheNetRequestGeneratorFixture : CoreTest<BroadcastheNetRequestGenerator>
    {
        private Series _series;

        [SetUp]
        public void SetUp()
        {
            Subject.Settings = new BroadcastheNetSettings
            {
                BaseUrl = "https://api.broadcasthe.net/",
                ApiKey = "abc"
            };

            Subject.PageSize = 100;

            _series = new Series { TvdbId = 433335 };
        }

        private SingleEpisodeSearchCriteria SingleEpisode(int seasonNumber, params int[] episodeNumbers)
        {
            return new SingleEpisodeSearchCriteria
            {
                Series = _series,
                SceneTitles = new List<string> { "Series Title" },
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
        public void should_search_for_three_digit_episode_number_in_the_same_request()
        {
            var results = Subject.GetSearchRequests(SingleEpisode(1, 99));

            results.Tiers.Should().Be(1);
            QueriedNames(results, 0).Should().HaveCount(1);
            QueriedNames(results, 0).First().Should().Contain(@"""Name"":""S01%E%99%""");
        }

        [TestCase(1, "S01%E%01%")]
        [TestCase(9, "S01%E%09%")]
        [TestCase(97, "S01%E%97%")]
        [TestCase(99, "S01%E%99%")]
        public void should_wildcard_episode_numbers_below_one_hundred(int episodeNumber, string expected)
        {
            var results = Subject.GetSearchRequests(SingleEpisode(1, episodeNumber));

            results.Tiers.Should().Be(1);
            QueriedNames(results, 0).Should().HaveCount(1);
            QueriedNames(results, 0).First().Should().Contain($@"""Name"":""{expected}""");
        }

        [TestCase(100, "S01%E100%")]
        [TestCase(101, "S01%E101%")]
        [TestCase(999, "S01%E999%")]
        public void should_not_wildcard_episode_numbers_over_ninety_nine(int episodeNumber, string expected)
        {
            var results = Subject.GetSearchRequests(SingleEpisode(1, episodeNumber));

            results.Tiers.Should().Be(1);
            QueriedNames(results, 0).Should().HaveCount(1);
            QueriedNames(results, 0).First().Should().Contain($@"""Name"":""{expected}""");
        }

        [Test]
        public void should_only_wildcard_the_episodes_below_one_hundred_of_a_multi_episode_search()
        {
            var results = Subject.GetSearchRequests(SingleEpisode(1, 99, 100));

            results.Tiers.Should().Be(1);
            QueriedNames(results, 0).Should().HaveCount(2);
            QueriedNames(results, 0)[0].Should().Contain(@"""Name"":""S01%E%99%""");
            QueriedNames(results, 0)[1].Should().Contain(@"""Name"":""S01%E100%""");
        }

        [Test]
        public void should_not_search_without_a_tvdb_or_tvrage_id()
        {
            _series.TvdbId = 0;

            var results = Subject.GetSearchRequests(SingleEpisode(1, 99));

            results.GetAllTiers().Should().BeEmpty();
        }

        [Test]
        public void should_not_change_season_search()
        {
            var criteria = new SeasonSearchCriteria
            {
                Series = _series,
                SceneTitles = new List<string> { "Series Title" },
                SeasonNumber = 1,
                Episodes = new List<Episode> { new Episode { SeasonNumber = 1, EpisodeNumber = 99 } }
            };

            var results = Subject.GetSearchRequests(criteria);

            results.Tiers.Should().Be(1);
            var names = QueriedNames(results, 0);
            names.Should().HaveCount(2);
            names[0].Should().Contain(@"""Name"":""Season 1%""");

            // the season wildcard already matches three digit episode numbers
            names[1].Should().Contain(@"""Name"":""S01E%""");
        }

        [TestCase(99, "S01E%99")]
        [TestCase(100, "S01E100")]
        public void should_search_for_three_digit_episode_number_for_anime(int episodeNumber, string expected)
        {
            var criteria = new AnimeEpisodeSearchCriteria
            {
                Series = _series,
                SceneTitles = new List<string> { "Series Title" },
                SeasonNumber = 1,
                EpisodeNumber = episodeNumber,
                AbsoluteEpisodeNumber = episodeNumber,
                Episodes = new List<Episode> { new Episode { SeasonNumber = 1, EpisodeNumber = episodeNumber } }
            };

            var results = Subject.GetSearchRequests(criteria);

            results.Tiers.Should().Be(1);
            QueriedNames(results, 0)[0].Should().Contain($@"""Name"":""{expected}""");
        }
    }
}

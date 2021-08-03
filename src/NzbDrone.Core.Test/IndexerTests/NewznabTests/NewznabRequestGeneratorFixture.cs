using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Indexers.Newznab;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.IndexerTests.NewznabTests
{
    public class NewznabRequestGeneratorFixture : CoreTest<NewznabRequestGenerator>
    {
        private SingleEpisodeSearchCriteria _singleEpisodeSearchCriteria;
        private AnimeEpisodeSearchCriteria _animeSearchCriteria;
        private NewznabCapabilities _capabilities;

        [SetUp]
        public void SetUp()
        {
            Subject.Settings = new NewznabSettings()
            {
                BaseUrl = "http://127.0.0.1:1234/",
                Categories = new[] { 1, 2 },
                AnimeCategories = new[] { 3, 4 },
                ApiKey = "abcd",
            };

            _singleEpisodeSearchCriteria = new SingleEpisodeSearchCriteria
            {
                Series = new Tv.Series { TvRageId = 10, TvdbId = 20, TvMazeId = 30, ImdbId = "t40" },
                SceneTitles = new List<string> { "Monkey Island" },
                SeasonNumber = 1,
                EpisodeNumber = 2
            };

            _animeSearchCriteria = new AnimeEpisodeSearchCriteria()
            {
                SceneTitles = new List<string>() { "Monkey+Island" },
                AbsoluteEpisodeNumber = 100,
                SeasonNumber = 5,
                EpisodeNumber = 4
            };

            _capabilities = new NewznabCapabilities();

            Mocker.GetMock<INewznabCapabilitiesProvider>()
                .Setup(v => v.GetCapabilities(It.IsAny<NewznabSettings>()))
                .Returns(_capabilities);
        }

        [Test]
        public void should_use_all_categories_for_feed()
        {
            var results = Subject.GetRecentRequests();

            results.GetAllTiers().Should().HaveCount(1);

            var page = results.GetAllTiers().First().First();

            page.Url.Query.Should().Contain("&cat=1,2,3,4&");
        }

        [Test]
        public void should_not_have_duplicate_categories()
        {
            Subject.Settings.Categories = new[] { 1, 2, 3 };

            var results = Subject.GetRecentRequests();

            results.GetAllTiers().Should().HaveCount(1);

            var page = results.GetAllTiers().First().First();

            page.Url.FullUri.Should().Contain("&cat=1,2,3,4&");
        }

        [Test]
        public void should_use_only_anime_categories_for_anime_search()
        {
            var results = Subject.GetSearchRequests(_animeSearchCriteria);

            results.GetAllTiers().Should().HaveCount(1);

            var page = results.GetAllTiers().First().First();

            page.Url.FullUri.Should().Contain("&cat=3,4&");
        }

        [Test]
        public void should_use_mode_search_for_anime()
        {
            var results = Subject.GetSearchRequests(_animeSearchCriteria);

            results.GetAllTiers().Should().HaveCount(1);

            var page = results.GetAllTiers().First().First();

            page.Url.FullUri.Should().Contain("?t=search&");
        }

        [Test]
        public void should_return_subsequent_pages()
        {
            var results = Subject.GetSearchRequests(_animeSearchCriteria);

            results.GetAllTiers().Should().HaveCount(1);

            var pages = results.GetAllTiers().First().Take(3).ToList();

            pages[0].Url.FullUri.Should().Contain("&offset=0&");
            pages[1].Url.FullUri.Should().Contain("&offset=100&");
            pages[2].Url.FullUri.Should().Contain("&offset=200&");
        }

        [Test]
        public void should_not_get_unlimited_pages()
        {
            var results = Subject.GetSearchRequests(_animeSearchCriteria);

            results.GetAllTiers().Should().HaveCount(1);

            var pages = results.GetAllTiers().First().Take(500).ToList();

            pages.Count.Should().BeLessThan(500);
        }

        [Test]
        public void should_use_only_absolute_numbering_for_anime_search()
        {
            var results = Subject.GetSearchRequests(_animeSearchCriteria);

            results.GetAllTiers().Should().HaveCount(1);

            var page = results.GetAllTiers().First().First();

            page.Url.FullUri.Should().Contain("q=Monkey%20Island+100");
        }

        [Test]
        public void should_also_use_standard_numbering_for_anime_search()
        {
            Subject.Settings.AnimeStandardFormatSearch = true;
            var results = Subject.GetSearchRequests(_animeSearchCriteria);

            results.GetTier(0).Should().HaveCount(2);
            var pages = results.GetTier(0).Take(2).Select(t => t.First()).ToList();

            pages[0].Url.FullUri.Should().Contain("q=Monkey%20Island+100");
            pages[1].Url.FullUri.Should().Contain("q=Monkey%20Island&season=5&ep=4");
        }

        [Test]
        public void should_not_search_by_rid_if_not_supported()
        {
            _capabilities.SupportedTvSearchParameters = new[] { "q", "season", "ep" };

            var results = Subject.GetSearchRequests(_singleEpisodeSearchCriteria);

            results.GetAllTiers().Should().HaveCount(1);

            var page = results.GetAllTiers().First().First();

            page.Url.Query.Should().NotContain("rid=10");
            page.Url.Query.Should().Contain("q=Monkey");
        }

        [Test]
        public void should_search_by_rid_if_supported()
        {
            var results = Subject.GetSearchRequests(_singleEpisodeSearchCriteria);
            results.GetTier(0).Should().HaveCount(1);

            var page = results.GetAllTiers().First().First();

            page.Url.Query.Should().Contain("rid=10");
        }

        [Test]
        public void should_not_search_by_tvdbid_if_not_supported()
        {
            _capabilities.SupportedTvSearchParameters = new[] { "q", "season", "ep" };

            var results = Subject.GetSearchRequests(_singleEpisodeSearchCriteria);
            results.GetTier(0).Should().HaveCount(1);

            var page = results.GetAllTiers().First().First();

            page.Url.Query.Should().NotContain("rid=10");
            page.Url.Query.Should().Contain("q=Monkey");
        }

        [Test]
        public void should_search_by_tvdbid_if_supported()
        {
            _capabilities.SupportedTvSearchParameters = new[] { "q", "tvdbid", "season", "ep" };

            var results = Subject.GetSearchRequests(_singleEpisodeSearchCriteria);
            results.GetTier(0).Should().HaveCount(1);

            var page = results.GetAllTiers().First().First();

            page.Url.Query.Should().Contain("tvdbid=20");
        }

        [Test]
        public void should_search_by_tvmaze_if_supported()
        {
            _capabilities.SupportedTvSearchParameters = new[] { "q", "tvmazeid", "season", "ep" };

            var results = Subject.GetSearchRequests(_singleEpisodeSearchCriteria);
            results.GetTier(0).Should().HaveCount(1);

            var page = results.GetAllTiers().First().First();

            page.Url.Query.Should().Contain("tvmazeid=30");
        }

        [Test]
        public void should_search_by_imdbid_if_supported()
        {
            _capabilities.SupportedTvSearchParameters = new[] { "q", "imdbid", "season", "ep" };

            var results = Subject.GetSearchRequests(_singleEpisodeSearchCriteria);
            results.GetTier(0).Should().HaveCount(1);

            var page = results.GetAllTiers().First().First();

            page.Url.Query.Should().Contain("imdbid=t40");
        }

        [Test]
        public void should_prefer_search_by_tvdbid_if_rid_supported()
        {
            _capabilities.SupportedTvSearchParameters = new[] { "q", "tvdbid", "rid", "season", "ep" };

            var results = Subject.GetSearchRequests(_singleEpisodeSearchCriteria);
            results.GetTier(0).Should().HaveCount(1);

            var page = results.GetAllTiers().First().First();

            page.Url.Query.Should().Contain("tvdbid=20");
            page.Url.Query.Should().NotContain("rid=10");
        }

        [Test]
        public void should_use_aggregrated_id_search_if_supported()
        {
            _capabilities.SupportedTvSearchParameters = new[] { "q", "tvdbid", "rid", "season", "ep" };
            _capabilities.SupportsAggregateIdSearch = true;

            var results = Subject.GetSearchRequests(_singleEpisodeSearchCriteria);
            results.GetTier(0).Should().HaveCount(1);

            var page = results.GetTier(0).First().First();

            page.Url.Query.Should().Contain("tvdbid=20");
            page.Url.Query.Should().Contain("rid=10");
        }

        [Test]
        public void should_not_use_aggregrated_id_search_if_no_ids_supported()
        {
            _capabilities.SupportedTvSearchParameters = new[] { "q", "season", "ep" };
            _capabilities.SupportsAggregateIdSearch = true; // Turns true if indexer supplies supportedParams.

            var results = Subject.GetSearchRequests(_singleEpisodeSearchCriteria);
            results.Tiers.Should().Be(1);
            results.GetTier(0).Should().HaveCount(1);

            var page = results.GetTier(0).First().First();

            page.Url.Query.Should().Contain("q=");
        }

        [Test]
        public void should_not_use_aggregrated_id_search_if_no_ids_are_known()
        {
            _capabilities.SupportedTvSearchParameters = new[] { "q", "rid", "season", "ep" };
            _capabilities.SupportsAggregateIdSearch = true; // Turns true if indexer supplies supportedParams.

            _singleEpisodeSearchCriteria.Series.TvRageId = 0;

            var results = Subject.GetSearchRequests(_singleEpisodeSearchCriteria);

            var page = results.GetTier(0).First().First();

            page.Url.Query.Should().Contain("q=");
        }

        [Test]
        public void should_fallback_to_title()
        {
            _capabilities.SupportedTvSearchParameters = new[] { "q", "title", "tvdbid", "rid", "season", "ep" };
            _capabilities.SupportsAggregateIdSearch = true;

            var results = Subject.GetSearchRequests(_singleEpisodeSearchCriteria);
            results.Tiers.Should().Be(2);

            var pageTier2 = results.GetTier(1).First().First();

            pageTier2.Url.Query.Should().NotContain("tvdbid=20");
            pageTier2.Url.Query.Should().NotContain("rid=10");
            pageTier2.Url.Query.Should().NotContain("q=");
            pageTier2.Url.Query.Should().Contain("title=Monkey%20Island");
        }

        [Test]
        public void should_url_encode_title()
        {
            _capabilities.SupportedTvSearchParameters = new[] { "q", "title", "tvdbid", "rid", "season", "ep" };
            _capabilities.SupportsAggregateIdSearch = true;

            _singleEpisodeSearchCriteria.SceneTitles[0] = "Elith & Little";

            var results = Subject.GetSearchRequests(_singleEpisodeSearchCriteria);
            results.Tiers.Should().Be(2);

            var pageTier2 = results.GetTier(1).First().First();

            pageTier2.Url.Query.Should().NotContain("tvdbid=20");
            pageTier2.Url.Query.Should().NotContain("rid=10");
            pageTier2.Url.Query.Should().NotContain("q=");
            pageTier2.Url.Query.Should().Contain("title=Elith%20%26%20Little");
        }

        [Test]
        public void should_fallback_to_q()
        {
            _capabilities.SupportedTvSearchParameters = new[] { "q", "tvdbid", "rid", "season", "ep" };
            _capabilities.SupportsAggregateIdSearch = true;

            var results = Subject.GetSearchRequests(_singleEpisodeSearchCriteria);
            results.Tiers.Should().Be(2);

            var pageTier2 = results.GetTier(1).First().First();

            pageTier2.Url.Query.Should().NotContain("tvdbid=20");
            pageTier2.Url.Query.Should().NotContain("rid=10");
            pageTier2.Url.Query.Should().Contain("q=");
        }

        [Test]
        public void should_encode_raw_title()
        {
            _capabilities.SupportedTvSearchParameters = new[] { "q", "season", "ep" };
            _capabilities.TvTextSearchEngine = "raw";
            _singleEpisodeSearchCriteria.SceneTitles[0] = "Edith & Little";

            var results = Subject.GetSearchRequests(_singleEpisodeSearchCriteria);
            results.Tiers.Should().Be(1);

            var pageTier = results.GetTier(0).First().First();

            pageTier.Url.Query.Should().Contain("q=Edith%20%26%20Little");
            pageTier.Url.Query.Should().NotContain(" & ");
            pageTier.Url.Query.Should().Contain("%26");
        }

        [Test]
        public void should_use_clean_title_and_encode()
        {
            _capabilities.SupportedTvSearchParameters = new[] { "q", "season", "ep" };
            _capabilities.TvTextSearchEngine = "sphinx";
            _singleEpisodeSearchCriteria.SceneTitles[0] = "Edith & Little";

            var results = Subject.GetSearchRequests(_singleEpisodeSearchCriteria);
            results.Tiers.Should().Be(1);

            var pageTier = results.GetTier(0).First().First();

            pageTier.Url.Query.Should().Contain("q=Edith%20and%20Little");
            pageTier.Url.Query.Should().Contain("and");
            pageTier.Url.Query.Should().NotContain(" & ");
            pageTier.Url.Query.Should().NotContain("%26");
        }
    }
}

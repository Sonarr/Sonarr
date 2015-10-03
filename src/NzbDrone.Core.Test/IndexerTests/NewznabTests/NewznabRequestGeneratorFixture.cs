using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Indexers.Newznab;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.IndexerTests.NewznabTests
{
    public class NewznabRequestGeneratorFixture : CoreTest<NewznabRequestGenerator>
    {
        AnimeEpisodeSearchCriteria _animeSearchCriteria;

        [SetUp]
        public void SetUp()
        {
            Subject.Settings = new NewznabSettings()
            {
                 Url = "http://127.0.0.1:1234/",
                 Categories = new [] { 1, 2 },
                 AnimeCategories = new [] { 3, 4 },
                 ApiKey = "abcd",
            };

            _animeSearchCriteria = new AnimeEpisodeSearchCriteria()
            {
                SceneTitles = new List<string>() { "Monkey+Island" },
                AbsoluteEpisodeNumber = 100
            };
        }

        [Test]
        public void should_use_all_categories_for_feed()
        {
            var results = Subject.GetRecentRequests();

            results.Should().HaveCount(1);

            var page = results.First().First();

            page.Url.Query.Should().Contain("&cat=1,2,3,4&");
        }

        [Test]
        public void should_not_have_duplicate_categories()
        {
            Subject.Settings.Categories = new[] { 1, 2, 3 };

            var results = Subject.GetRecentRequests();

            results.Should().HaveCount(1);

            var page = results.First().First();

            page.Url.Query.Should().Contain("&cat=1,2,3,4&");
        }

        [Test]
        public void should_use_only_anime_categories_for_anime_search()
        {
            var results = Subject.GetSearchRequests(_animeSearchCriteria);

            results.Should().HaveCount(1);

            var page = results.First().First();

            page.Url.Query.Should().Contain("&cat=3,4&");
        }
        
        [Test]
        public void should_use_mode_search_for_anime()
        {
            var results = Subject.GetSearchRequests(_animeSearchCriteria);

            results.Should().HaveCount(1);

            var page = results.First().First();

            page.Url.Query.Should().Contain("?t=search&");
        }

        [Test]
        public void should_return_subsequent_pages()
        {
            var results = Subject.GetSearchRequests(_animeSearchCriteria);

            results.Should().HaveCount(1);

            var pages = results.First().Take(3).ToList();

            pages[0].Url.Query.Should().Contain("&offset=0&");
            pages[1].Url.Query.Should().Contain("&offset=100&");
            pages[2].Url.Query.Should().Contain("&offset=200&");
        }

        [Test]
        public void should_not_get_unlimited_pages()
        {
            var results = Subject.GetSearchRequests(_animeSearchCriteria);

            results.Should().HaveCount(1);

            var pages = results.First().Take(500).ToList();

            pages.Count.Should().BeLessThan(500);
        }
    }
}

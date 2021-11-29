using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Indexers.Nyaa;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.IndexerTests.NyaaTests
{
    public class NyaaRequestGeneratorFixture : CoreTest<NyaaRequestGenerator>
    {
        private SeasonSearchCriteria _seasonSearchCriteria;
        private AnimeEpisodeSearchCriteria _animeSearchCriteria;

        [SetUp]
        public void SetUp()
        {
            Subject.Settings = new NyaaSettings()
            {
                BaseUrl = "http://127.0.0.1:1234/",
            };

            _seasonSearchCriteria = new SeasonSearchCriteria()
            {
                SceneTitles = new List<string>() { "Naruto Shippuuden" },
                SeasonNumber = 1,
            };

            _animeSearchCriteria = new AnimeEpisodeSearchCriteria()
            {
                SceneTitles = new List<string>() { "Naruto Shippuuden" },
                AbsoluteEpisodeNumber = 9,
                SeasonNumber = 1,
                EpisodeNumber = 9
            };
        }

        [Test]
        public void should_not_search_season()
        {
            var results = Subject.GetSearchRequests(_seasonSearchCriteria);

            results.GetAllTiers().Should().HaveCount(0);
        }

        [Test]
        public void should_search_season()
        {
            Subject.Settings.AnimeStandardFormatSearch = true;
            var results = Subject.GetSearchRequests(_seasonSearchCriteria);

            results.GetAllTiers().Should().HaveCount(1);

            var page = results.GetAllTiers().First().First();

            page.Url.FullUri.Should().Contain("term=Naruto+Shippuuden+s01");
        }

        [Test]
        public void should_use_only_absolute_numbering_for_anime_search()
        {
            var results = Subject.GetSearchRequests(_animeSearchCriteria);

            results.GetTier(0).Should().HaveCount(2);
            var pages = results.GetTier(0).Take(2).Select(t => t.First()).ToList();

            pages[0].Url.FullUri.Should().Contain("term=Naruto+Shippuuden+9");
            pages[1].Url.FullUri.Should().Contain("term=Naruto+Shippuuden+09");
        }

        [Test]
        public void should_also_use_standard_numbering_for_anime_search()
        {
            Subject.Settings.AnimeStandardFormatSearch = true;
            var results = Subject.GetSearchRequests(_animeSearchCriteria);

            results.GetTier(0).Should().HaveCount(3);
            var pages = results.GetTier(0).Take(3).Select(t => t.First()).ToList();

            pages[0].Url.FullUri.Should().Contain("term=Naruto+Shippuuden+9");
            pages[1].Url.FullUri.Should().Contain("term=Naruto+Shippuuden+09");
            pages[2].Url.FullUri.Should().Contain("term=Naruto+Shippuuden+s01e09");
        }
    }
}

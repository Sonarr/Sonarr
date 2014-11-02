using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using NzbDrone.Core.Indexers.NzbIndex;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;
using FluentAssertions;

namespace NzbDrone.Core.Test.IndexerTests.NzbIndexTests
{
    public class NzbIndexRequestGeneratorFixture : CoreTest<NzbIndexRequestGenerator>
    {
        [SetUp]
        public void SetUp()
        {
            Subject.Settings = new NzbIndexSettings
            {
                Url = "http://127.0.0.1:1234/",
            };

        }

        [Test]
        public void should_have_proper_request_for_tvepisode_request_with_modified_settings()
        {
            Subject.Settings.MaxSizeParam = "maxi";
            Subject.Settings.MinSizeParam = "mini";
            Subject.Settings.MaxAgeParam = "ouderdom";
            Subject.Settings.QueryParam = "query";
            Subject.Settings.ResponseMaxSizeParam = "pagesize";
            Subject.Settings.ResponseMaxSize = 300;
            Subject.Settings.AdditionalParameters = "&pipo=3";
            Subject.MaxAge = 60;

            var request = new SingleEpisodeSearchCriteria
            {
                EpisodeNumber = 1,
                SeasonNumber = 1,
                Series = new Series
                {
                    Title = "2 Broke girls",
                },
                SceneTitles = new List<string> { "2 Broke Girls", "Two Broke Girls" }
            };
           
            var results = Subject.GetSearchRequests(request);
            results.Count.Should().Be(1);
            results.First().Count().Should().Be(2);
            results.First().First().Url.Query.Should().Be("?query=2+Broke+Girls+S01E01&mini=200&maxi=3000&ouderdom=60&pagesize=300&pipo=3");
            results.First().Last().Url.Query.Should().Be("?query=Two+Broke+Girls+S01E01&mini=200&maxi=3000&ouderdom=60&pagesize=300&pipo=3");
        }

        [Test]
        public void should_have_proper_request_for_tvepisode_request_default_settings()
        {
            var request = new SingleEpisodeSearchCriteria
            {
                EpisodeNumber = 1,
                SeasonNumber = 1,
                Series = new Series
                {
                    Title = "2 Broke girls",
                },
                SceneTitles = new List<string> { "2 Broke Girls", "Two Broke Girls" }
            };

            var results = Subject.GetSearchRequests(request);
            
            results.Count.Should().Be(1);
            results.First().Count().Should().Be(2);
            results.First().First().Url.Query.Should().Be("?q=2+Broke+Girls+S01E01&minsize=200&maxsize=3000&max=50&complete=1&hidespam=1");
            results.First().Last().Url.Query.Should().Be("?q=Two+Broke+Girls+S01E01&minsize=200&maxsize=3000&max=50&complete=1&hidespam=1");
        }

        [Test]
        public void should_get_proper_request_for_tvseries_request_default_settings()
        {
            var request = new SeasonSearchCriteria
            {
                SeasonNumber = 3,
                Series = new Series
                {
                    Title = "2 Broke girls",
                },
                SceneTitles = new List<string> { "2 Broke Girls", "Two Broke Girls" }
            };

            var results = Subject.GetSearchRequests(request);

            results.Count.Should().Be(2);
            results.First().Count().Should().Be(2);

            results.First().First().Url.Query.Should().Be("?q=2+Broke+Girls+S03&minsize=200&maxsize=3000&max=50&complete=1&hidespam=1");
            results.First().Last().Url.Query.Should().Be("?q=Two+Broke+Girls+S03&minsize=200&maxsize=3000&max=50&complete=1&hidespam=1");
            results.Last().First().Url.Query.Should().Be("?q=2+Broke+Girls+Season+3&minsize=200&maxsize=3000&max=50&complete=1&hidespam=1");
            results.Last().Last().Url.Query.Should().Be("?q=Two+Broke+Girls+Season+3&minsize=200&maxsize=3000&max=50&complete=1&hidespam=1");
        }

        [Test]
        public void should_get_proper_request_for_series_specials()
        {
            var request = new SpecialEpisodeSearchCriteria
            {
                Series = new Series
                {
                    Title = "Continuum",
                },
                SceneTitles = new List<string> { "Continuum" },
                EpisodeQueryTitles = new[] { "Season 2 Sneak Peek", "Season 3 Sneak Peek" }
            };

            var results = Subject.GetSearchRequests(request);

            results.Count.Should().Be(1);
            results.First().Count().Should().Be(2);
            results.First().First().Url.Query.Should().Be("?q=Continuum+Season+2+Sneak+Peek&minsize=200&maxsize=3000&max=50&complete=1&hidespam=1");
            results.First().Last().Url.Query.Should() .Be("?q=Continuum+Season+3+Sneak+Peek&minsize=200&maxsize=3000&max=50&complete=1&hidespam=1");
        }

        [Test]
        public void should_get_proper_request_for_dailyshows()
        {
            var request = new DailyEpisodeSearchCriteria
            {
                Series = new Series
                {
                    Title = "Daily show",
                },
                SceneTitles = new List<string> { "Daily show" },
                AirDate = new DateTime(2012,9,30)
            };

            var results = Subject.GetSearchRequests(request);

            results.Count.Should().Be(1);
            results.First().Count().Should().Be(1); 
            results.First().First().Url.Query.Should().Be("?q=Daily+show+2012+09+30&minsize=200&maxsize=3000&max=50&complete=1&hidespam=1");
        }

        [Test]
        public void should_get_proper_request_for_anime()
        {
            var request = new AnimeEpisodeSearchCriteria
            {
                Series = new Series
                {
                    Title = "Sabagebu! - Survival Game Club!",
                },
                SceneTitles = new List<string> { "Sabagebu! - Survival Game Club!" },
                AbsoluteEpisodeNumber = 7
            };

            var results = Subject.GetSearchRequests(request);

            results.Count.Should().Be(1);
            results.First().Count().Should().Be(1);
            results.First().First().Url.Query.Should().Be("?q=Sabagebu+Survival+Game+Club+07&minsize=200&maxsize=3000&max=50&complete=1&hidespam=1");
        }

    }
}

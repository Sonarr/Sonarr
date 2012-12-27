using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Model.TvRage;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Repository;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.ProviderTests.TvRageMappingProviderTests
{
    public class ProcessResultsFixture : TestBase
    {
        private IList<TvRageSearchResult> _searchResults;
        private Series _series;
        private Episode _episode;

        [SetUp]
        public void Setup()
        {
            _searchResults = Builder<TvRageSearchResult>
                    .CreateListOfSize(5)
                    .Build();

            _series = Builder<Series>
                .CreateNew()
                .With(s => s.FirstAired = DateTime.Today.AddDays(-180))
                .Build();

            _episode = Builder<Episode>
                .CreateNew()
                .With(e => e.AirDate = DateTime.Today.AddDays(-365))
                .Build();
        }

        [Test]
        public void should_return_null_if_no_match_is_found()
        {
            Mocker.Resolve<TvRageMappingProvider>()
                  .ProcessResults(_searchResults, _series, "nomatchhere", _episode)
                  .Should()
                  .BeNull();
        }

        [Test]
        public void should_return_result_if_series_clean_name_matches()
        {
            _series.CleanTitle = Parser.NormalizeTitle(_searchResults.First().Name);

            Mocker.Resolve<TvRageMappingProvider>()
                  .ProcessResults(_searchResults, _series, "nomatchhere", _episode)
                  .Should()
                  .Be(_searchResults.First());
        }

        [Test]
        public void should_return_result_if_scene_clean_name_matches()
        {
            Mocker.Resolve<TvRageMappingProvider>()
                  .ProcessResults(_searchResults, _series, Parser.NormalizeTitle(_searchResults.First().Name), _episode)
                  .Should()
                  .Be(_searchResults.First());
        }

        [Test]
        public void should_return_result_if_series_firstAired_matches()
        {
            _series.FirstAired = _searchResults.Last().Started;

            Mocker.Resolve<TvRageMappingProvider>()
                  .ProcessResults(_searchResults, _series, "nomatchhere", _episode)
                  .Should()
                  .Be(_searchResults.Last());
        }

        [Test]
        public void should_return_result_if_episode_firstAired_matches()
        {
            _episode.AirDate = _searchResults.Last().Started;

            Mocker.Resolve<TvRageMappingProvider>()
                  .ProcessResults(_searchResults, _series, "nomatchhere", _episode)
                  .Should()
                  .Be(_searchResults.Last());
        }
    }
}

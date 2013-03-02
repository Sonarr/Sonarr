using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.ReferenceData;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Model.TvRage;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Repository;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.ProviderTests.TvRageMappingProviderTests
{
    public class FindMatchingTvRageSeriesFixture : TestBase
    {
        private IList<TvRageSearchResult> _searchResults;
        private Series _series;
        private Episode _episode;
        private TvRageSeries _tvRageSeries;

        [SetUp]
        public void Setup()
        {
            _searchResults = Builder<TvRageSearchResult>
                    .CreateListOfSize(5)
                    .Build();

            _series = Builder<Series>
                    .CreateNew()
                    .With(s => s.TvRageId = 0)
                    .With(s => s.TvRageTitle = null)
                    .With(s => s.UtcOffset = 0)
                    .With(s => s.FirstAired = DateTime.Today.AddDays(-180))
                    .Build();

            _episode = Builder<Episode>
                .CreateNew()
                .With(e => e.AirDate = DateTime.Today.AddDays(-365))
                .Build();

            _tvRageSeries = Builder<TvRageSeries>
                    .CreateNew()
                    .With(s => s.UtcOffset = -8)
                    .Build();

            Mocker.GetMock<IEpisodeService>()
                  .Setup(s => s.GetEpisode(_series.Id, 1, 1))
                  .Returns(_episode);

            Mocker.GetMock<SceneMappingService>()
                  .Setup(s => s.GetCleanName(_series.Id))
                  .Returns("");

            Mocker.GetMock<TvRageProvider>()
                  .Setup(s => s.SearchSeries(_series.Title))
                  .Returns(_searchResults);

            Mocker.GetMock<TvRageProvider>()
                  .Setup(s => s.GetSeries(_searchResults.First().ShowId))
                  .Returns(_tvRageSeries);
        }

        private void WithMatchingResult()
        {
            _series.CleanTitle = Parser.NormalizeTitle(_searchResults.First().Name);
        }

        [Test]
        public void should_not_set_tvRage_info_when_result_is_null()
        {
            var result = Mocker.Resolve<TvRageMappingProvider>()
                               .FindMatchingTvRageSeries(_series);

            result.TvRageId.Should().Be(0);
            result.TvRageTitle.Should().Be(null);
            result.UtcOffset.Should().Be(0);
        }

        [Test]
        public void should_set_tvRage_info_when_result_is_returned()
        {
            WithMatchingResult();

            var result = Mocker.Resolve<TvRageMappingProvider>()
                               .FindMatchingTvRageSeries(_series);

            result.TvRageId.Should().Be(_searchResults.First().ShowId);
            result.TvRageTitle.Should().Be(_searchResults.First().Name);
            result.UtcOffset.Should().Be(_tvRageSeries.UtcOffset);
        }
    }
}

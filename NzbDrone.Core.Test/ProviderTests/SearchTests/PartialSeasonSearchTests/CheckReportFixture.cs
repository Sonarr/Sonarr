using System;
using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Model;
using NzbDrone.Core.Model.Notification;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Providers.Indexer;
using NzbDrone.Core.Providers.Search;
using NzbDrone.Core.Repository;
using NzbDrone.Core.Repository.Search;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.ProviderTests.SearchTests.PartialSeasonSearchTests
{
    [TestFixture]
    public class CheckReportFixture : TestBase
    {
        private Series _series;
        private List<Episode> _episodes;
        private EpisodeParseResult _episodeParseResult;
        private SearchHistoryItem _searchHistoryItem;
            
        [SetUp]
        public void Setup()
        {
            _series = Builder<Series>
                    .CreateNew()
                    .Build();

            _episodes = Builder<Episode>
                    .CreateListOfSize(10)
                    .All()
                    .With(e => e.SeriesId = _series.SeriesId)
                    .With(e => e.Series = _series)
                    .Build()
                    .ToList();

            _episodeParseResult = Builder<EpisodeParseResult>
                    .CreateNew()
                    .With(p => p.SeasonNumber = 1)
                    .Build();

            _searchHistoryItem = new SearchHistoryItem();
        }

        [Test]
        public void should_return_wrongSeason_when_season_does_not_match()
        {
            Mocker.Resolve<PartialSeasonSearch>()
                  .CheckReport(_series, new { SeasonNumber = 2, Episodes = _episodes }, _episodeParseResult, _searchHistoryItem)
                  .SearchError.Should().Be(ReportRejectionType.WrongSeason);
        }

        [Test]
        public void should_not_return_error_when_season_matches()
        {
            Mocker.Resolve<PartialSeasonSearch>()
                  .CheckReport(_series, new { SeasonNumber = 1, Episodes = _episodes }, _episodeParseResult, _searchHistoryItem)
                  .SearchError.Should().Be(ReportRejectionType.None);
        }
    }
}

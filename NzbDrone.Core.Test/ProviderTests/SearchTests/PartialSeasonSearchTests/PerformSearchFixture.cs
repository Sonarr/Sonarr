using System;
using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Model;
using NzbDrone.Core.Model.Notification;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Providers.Indexer;
using NzbDrone.Core.Providers.Search;
using NzbDrone.Core.Repository;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.ProviderTests.SearchTests.PartialSeasonSearchTests
{
    [TestFixture]
    public class PerformSearchFixture : PerformSearchTestBase
    {
        [Test]
        public void should_throw_if_season_number_is_less_than_zero()
        {
            Assert.Throws<ArgumentException>(() =>
                                                Mocker.Resolve<PartialSeasonSearch>()
                                                      .PerformSearch(_series, new
                                                          {
                                                                  SeasonNumber = -1, 
                                                                  Episodes = new List<Episode>{ new Episode() }
                                                          }, notification));
        }

        [Test]
        public void should_throw_if_episodes_is_empty()
        {
            Assert.Throws<ArgumentException>(() =>
                                                Mocker.Resolve<PartialSeasonSearch>()
                                                      .PerformSearch(_series, new { SeasonNumber = 10, Episodes = new List<Episode>() }, notification));
        }

        [Test]
        public void should_fetch_results_from_indexers()
        {
            WithValidIndexers();

            Mocker.Resolve<PartialSeasonSearch>()
                  .PerformSearch(_series, new { SeasonNumber = 1, Episodes = _episodes }, notification)
                  .Should()
                  .HaveCount(40);
        }

        [Test]
        public void should_log_error_when_fetching_from_indexer_fails()
        {
            WithInvalidIndexers();

            Mocker.Resolve<PartialSeasonSearch>()
                  .PerformSearch(_series, new { SeasonNumber = 1, Episodes = _episodes }, notification)
                  .Should()
                  .HaveCount(0);

            ExceptionVerification.ExpectedErrors(4);
        }

        [Test]
        public void should_hit_each_indexer_once_for_each_prefix()
        {
            WithValidIndexers();

            Mocker.Resolve<PartialSeasonSearch>()
                  .PerformSearch(_series, new { SeasonNumber = 1, Episodes = _episodes }, notification)
                  .Should()
                  .HaveCount(40);

            _indexer1.Verify(v => v.FetchPartialSeason(_series.Title, 1, 0), Times.Once());
            _indexer1.Verify(v => v.FetchPartialSeason(_series.Title, 1, 1), Times.Once());
            _indexer2.Verify(v => v.FetchPartialSeason(_series.Title, 1, 0), Times.Once());
            _indexer2.Verify(v => v.FetchPartialSeason(_series.Title, 1, 1), Times.Once());
        }
    }
}

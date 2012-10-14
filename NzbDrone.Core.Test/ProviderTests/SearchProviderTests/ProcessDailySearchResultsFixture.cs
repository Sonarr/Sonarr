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
using NzbDrone.Core.Providers.DecisionEngine;
using NzbDrone.Core.Providers.Indexer;
using NzbDrone.Core.Repository;
using NzbDrone.Core.Repository.Quality;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;
using NzbDrone.Test.Common.AutoMoq;

namespace NzbDrone.Core.Test.ProviderTests.SearchProviderTests
{
    [TestFixture]
    // ReSharper disable InconsistentNaming
    public class ProcessDailySearchResultsFixture : CoreTest
    {
        private Series _matchingSeries = null;
        private Series _mismatchedSeries = null;
        private Series _nullSeries = null;

        [SetUp]
        public void setup()
        {
            _matchingSeries = Builder<Series>.CreateNew()
                .With(s => s.SeriesId = 79488)
                .With(s => s.Title = "30 Rock")
                .Build();

            _mismatchedSeries = Builder<Series>.CreateNew()
                .With(s => s.SeriesId = 12345)
                .With(s => s.Title = "Not 30 Rock")
                .Build();
        }

        private void WithMatchingSeries()
        {
            Mocker.GetMock<SeriesProvider>()
                .Setup(s => s.FindSeries(It.IsAny<string>())).Returns(_matchingSeries);
        }

        private void WithMisMatchedSeries()
        {
            Mocker.GetMock<SeriesProvider>()
                .Setup(s => s.FindSeries(It.IsAny<string>())).Returns(_mismatchedSeries);
        }

        private void WithNullSeries()
        {
            Mocker.GetMock<SeriesProvider>()
                .Setup(s => s.FindSeries(It.IsAny<string>())).Returns(_nullSeries);
        }

        private void WithSuccessfulDownload()
        {
            Mocker.GetMock<DownloadProvider>()
                .Setup(s => s.DownloadReport(It.IsAny<EpisodeParseResult>()))
                .Returns(true);
        }

        private void WithFailingDownload()
        {
            Mocker.GetMock<DownloadProvider>()
                .Setup(s => s.DownloadReport(It.IsAny<EpisodeParseResult>()))
                .Returns(false);
        }

        private void WithQualityNeeded()
        {
            Mocker.GetMock<AllowedDownloadSpecification>()
                .Setup(s => s.IsSatisfiedBy(It.IsAny<EpisodeParseResult>()))
                .Returns(ReportRejectionType.None);
        }

        private void WithQualityNotNeeded()
        {
            Mocker.GetMock<AllowedDownloadSpecification>()
                .Setup(s => s.IsSatisfiedBy(It.IsAny<EpisodeParseResult>()))
                .Returns(ReportRejectionType.ExistingQualityIsEqualOrBetter);
        }

        [Test]
        public void processSearchResults_higher_quality_should_be_called_first()
        {
            var parseResults = Builder<EpisodeParseResult>.CreateListOfSize(5)
                .All()
                .With(c => c.AirDate = DateTime.Today)
                .With(c => c.Quality = new QualityModel(QualityTypes.DVD, true))
                .Random(1)
                .With(c => c.Quality = new QualityModel(QualityTypes.Bluray1080p, true))
                .Build();

            WithMatchingSeries();
            WithSuccessfulDownload();

            Mocker.GetMock<AllowedDownloadSpecification>()
                .Setup(s => s.IsSatisfiedBy(It.Is<EpisodeParseResult>(d => d.Quality.Quality == QualityTypes.Bluray1080p)))
                .Returns(ReportRejectionType.None);

            //Act
            var result = Mocker.Resolve<SearchProvider>().ProcessSearchResults(MockNotification, parseResults, _matchingSeries, DateTime.Today);

            //Assert
            result.Should().Contain(n => n.Success);

            Mocker.GetMock<AllowedDownloadSpecification>().Verify(c => c.IsSatisfiedBy(It.IsAny<EpisodeParseResult>()),
                                                       Times.Once());
            Mocker.GetMock<DownloadProvider>().Verify(c => c.DownloadReport(It.IsAny<EpisodeParseResult>()),
                                                      Times.Once());
        }

        [Test]
        public void processSearchResults_when_quality_is_not_needed_should_check_the_rest()
        {
            var parseResults = Builder<EpisodeParseResult>.CreateListOfSize(5)
                .All()
                .With(c => c.AirDate = DateTime.Today)
                .With(c => c.Quality = new QualityModel(QualityTypes.DVD, true))
                .Build();

            WithMatchingSeries();
            WithQualityNotNeeded();

            //Act
            var result = Mocker.Resolve<SearchProvider>().ProcessSearchResults(MockNotification, parseResults, _matchingSeries, DateTime.Today);

            //Assert
            result.Should().NotContain(n => n.Success);

            Mocker.GetMock<AllowedDownloadSpecification>().Verify(c => c.IsSatisfiedBy(It.IsAny<EpisodeParseResult>()),
                                                       Times.Exactly(5));
            Mocker.GetMock<DownloadProvider>().Verify(c => c.DownloadReport(It.IsAny<EpisodeParseResult>()),
                                                      Times.Never());
        }

        [Test]
        public void processSearchResults_should_skip_if_series_is_null()
        {
            var parseResults = Builder<EpisodeParseResult>.CreateListOfSize(5)
                .All()
                .With(e => e.AirDate = DateTime.Today)
                .With(e => e.Quality = new QualityModel(QualityTypes.HDTV, false))
                .Build();

            WithNullSeries();

            //Act
            var result = Mocker.Resolve<SearchProvider>().ProcessSearchResults(MockNotification, parseResults, _matchingSeries, DateTime.Today);

            //Assert
            result.Should().NotContain(n => n.Success);

            Mocker.GetMock<DownloadProvider>().Verify(c => c.DownloadReport(It.IsAny<EpisodeParseResult>()),
                                                      Times.Never());
        }

        [Test]
        public void processSearchResults_should_skip_if_series_is_mismatched()
        {
            var parseResults = Builder<EpisodeParseResult>.CreateListOfSize(5)
                .All()
                .With(e => e.AirDate = DateTime.Today)
                .With(e => e.Quality = new QualityModel(QualityTypes.HDTV, false))
                .Build();

            WithMisMatchedSeries();

            //Act
            var result = Mocker.Resolve<SearchProvider>().ProcessSearchResults(MockNotification, parseResults, _matchingSeries, DateTime.Today);

            //Assert
            result.Should().NotContain(n => n.Success);

            Mocker.GetMock<DownloadProvider>().Verify(c => c.DownloadReport(It.IsAny<EpisodeParseResult>()),
                                                      Times.Never());
        }

        [Test]
        public void processSearchResults_should_return_after_successful_download()
        {
            var parseResults = Builder<EpisodeParseResult>.CreateListOfSize(2)
                .All()
                .With(e => e.AirDate = DateTime.Today)
                .With(c => c.Quality = new QualityModel(QualityTypes.DVD, true))
                .Build();

            WithMatchingSeries();
            WithQualityNeeded();
            WithSuccessfulDownload();

            //Act
            var result = Mocker.Resolve<SearchProvider>().ProcessSearchResults(MockNotification, parseResults, _matchingSeries, DateTime.Today);

            //Assert
            result.Should().Contain(n => n.Success);

            Mocker.GetMock<DownloadProvider>().Verify(c => c.DownloadReport(It.IsAny<EpisodeParseResult>()),
                                                      Times.Once());
        }

        [Test]
        public void processSearchResults_should_try_next_if_download_fails()
        {
            var parseResults = Builder<EpisodeParseResult>.CreateListOfSize(2)
                .All()
                .With(e => e.AirDate = DateTime.Today)
                .With(c => c.Quality = new QualityModel(QualityTypes.DVD, true))
                .TheLast(1)
                .With(c => c.Quality = new QualityModel(QualityTypes.SDTV, true))
                .Build();

            WithMatchingSeries();
            WithQualityNeeded();

            Mocker.GetMock<DownloadProvider>()
                .Setup(s => s.DownloadReport(It.Is<EpisodeParseResult>(d => d.Quality.Quality == QualityTypes.DVD)))
                .Returns(false);

            Mocker.GetMock<DownloadProvider>()
                .Setup(s => s.DownloadReport(It.Is<EpisodeParseResult>(d => d.Quality.Quality == QualityTypes.SDTV)))
                .Returns(true);

            //Act
            var result = Mocker.Resolve<SearchProvider>().ProcessSearchResults(MockNotification, parseResults, _matchingSeries, DateTime.Today);

            //Assert
            result.Should().Contain(n => n.Success);

            Mocker.GetMock<DownloadProvider>().Verify(c => c.DownloadReport(It.IsAny<EpisodeParseResult>()),
                                                      Times.Exactly(2));
        }

        [Test]
        public void processSearchResults_should_skip_if_parseResult_does_not_have_airdate()
        {
            var parseResults = Builder<EpisodeParseResult>.CreateListOfSize(5)
                .All()
                .With(e => e.AirDate = null)
                .With(e => e.Quality = new QualityModel(QualityTypes.HDTV, false))
                .Build();

            WithMatchingSeries();

            //Act
            var result = Mocker.Resolve<SearchProvider>().ProcessSearchResults(MockNotification, parseResults, _matchingSeries, DateTime.Today);

            //Assert
            result.Should().NotContain(n => n.Success);

            Mocker.GetMock<DownloadProvider>().Verify(c => c.DownloadReport(It.IsAny<EpisodeParseResult>()),
                                                      Times.Never());
        }

        [Test]
        public void processSearchResults_should_skip_if_parseResult_airdate_does_not_match()
        {
            var parseResults = Builder<EpisodeParseResult>.CreateListOfSize(5)
                .All()
                .With(e => e.AirDate = DateTime.Today.AddDays(10))
                .With(e => e.Quality = new QualityModel(QualityTypes.HDTV, false))
                .Build();

            WithMatchingSeries();

            //Act
            var result = Mocker.Resolve<SearchProvider>().ProcessSearchResults(MockNotification, parseResults, _matchingSeries, DateTime.Today);

            //Assert
            result.Should().NotContain(n => n.Success);

            Mocker.GetMock<DownloadProvider>().Verify(c => c.DownloadReport(It.IsAny<EpisodeParseResult>()),
                                                      Times.Never());
        }
    }
}
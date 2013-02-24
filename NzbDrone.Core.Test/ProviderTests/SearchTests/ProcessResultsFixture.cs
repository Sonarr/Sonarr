using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Download;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Model;
using NzbDrone.Core.Model.Notification;
using NzbDrone.Core.Providers;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Providers.Search;
using NzbDrone.Core.Repository;
using NzbDrone.Core.Repository.Quality;
using NzbDrone.Core.Repository.Search;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.ProviderTests.SearchTests
{
    [TestFixture]
    public class ProcessResultsFixture : TestBase
    {
        private Series _matchingSeries;
        private Series _mismatchedSeries;
        private Series _nullSeries = null;

        private SearchHistory _searchHistory;
        private ProgressNotification _notification;

        private IList<Episode> _episodes;
            
        [SetUp]
        public void Setup()
        {
            _matchingSeries = Builder<Series>.CreateNew()
                .With(s => s.SeriesId = 79488)
                .With(s => s.Title = "30 Rock")
                .Build();

            _mismatchedSeries = Builder<Series>.CreateNew()
                .With(s => s.SeriesId = 12345)
                .With(s => s.Title = "Not 30 Rock")
                .Build();

            _searchHistory = new SearchHistory();
            _notification = new ProgressNotification("Test");

            _episodes = Builder<Episode>
                    .CreateListOfSize(1)
                    .Build();

            Mocker.GetMock<IEpisodeService>()
                  .Setup(s => s.GetEpisodesByParseResult(It.IsAny<EpisodeParseResult>()))
                  .Returns(_episodes);
        }

        private void WithMatchingSeries()
        {
            Mocker.GetMock<ISeriesRepository>()
                .Setup(s => s.GetByTitle(It.IsAny<string>())).Returns(_matchingSeries);
        }

        private void WithMisMatchedSeries()
        {
            Mocker.GetMock<ISeriesRepository>()
                .Setup(s => s.GetByTitle(It.IsAny<string>())).Returns(_mismatchedSeries);
        }

        private void WithNullSeries()
        {
            Mocker.GetMock<ISeriesRepository>()
                .Setup(s => s.GetByTitle(It.IsAny<string>())).Returns(_nullSeries);
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
        public void should_process_higher_quality_results_first()
        {
            WithMatchingSeries();
            WithSuccessfulDownload();
            
            var parseResults = Builder<EpisodeParseResult>.CreateListOfSize(5)
                .All()
                .With(e => e.SeasonNumber = 1)
                .With(e => e.EpisodeNumbers = new List<int> { 1 })
                .With(c => c.Quality = new QualityModel(QualityTypes.DVD, true))
                .With(c => c.Age = 10)
                .Random(1)
                .With(c => c.Quality = new QualityModel(QualityTypes.Bluray1080p, true))
                .With(c => c.Age = 100)
                .Build()
                .ToList();


            Mocker.GetMock<AllowedDownloadSpecification>()
                .Setup(s => s.IsSatisfiedBy(It.Is<EpisodeParseResult>(d => d.Quality.Quality == QualityTypes.Bluray1080p)))
                .Returns(ReportRejectionType.None);

            var result = Mocker.Resolve<TestSearch>().ProcessReports(_matchingSeries, new { }, parseResults, _searchHistory, _notification);

            result.SearchHistoryItems.Should().HaveCount(parseResults.Count);
            result.SearchHistoryItems.Should().Contain(s => s.Success);

            Mocker.GetMock<AllowedDownloadSpecification>().Verify(c => c.IsSatisfiedBy(It.IsAny<EpisodeParseResult>()),
                                                       Times.Once());
            Mocker.GetMock<DownloadProvider>().Verify(c => c.DownloadReport(It.IsAny<EpisodeParseResult>()),
                                                      Times.Once());
        }

        [Test]
        public void should_process_newer_reports_first()
        {
            WithMatchingSeries();
            WithSuccessfulDownload();

            var parseResults = Builder<EpisodeParseResult>.CreateListOfSize(5)
                .All()
                .With(e => e.SeasonNumber = 1)
                .With(e => e.EpisodeNumbers = new List<int> { 1 })
                .With(c => c.Quality = new QualityModel(QualityTypes.Bluray1080p, true))
                .With(c => c.Age = 300)
                .Build()
                .ToList();

            parseResults[2].Age = 100;

            Mocker.GetMock<AllowedDownloadSpecification>()
                .Setup(s => s.IsSatisfiedBy(It.IsAny<EpisodeParseResult>())).Returns(ReportRejectionType.None);

            var result = Mocker.Resolve<TestSearch>().ProcessReports(_matchingSeries, new { }, parseResults, _searchHistory, _notification);

            result.SearchHistoryItems.Should().HaveCount(parseResults.Count);
            result.SearchHistoryItems.Should().Contain(s => s.Success);


            Mocker.GetMock<DownloadProvider>().Verify(c => c.DownloadReport(It.Is<EpisodeParseResult>(d => d.Age != 100)), Times.Never());
            Mocker.GetMock<DownloadProvider>().Verify(c => c.DownloadReport(It.Is<EpisodeParseResult>(d => d.Age == 100)), Times.Once());
        }

        [Test]
        public void should_check_other_reports_when_quality_is_not_wanted()
        {
            WithMatchingSeries();
            WithQualityNotNeeded();

            var parseResults = Builder<EpisodeParseResult>.CreateListOfSize(5)
                .All()
                .With(e => e.SeasonNumber = 1)
                .With(e => e.EpisodeNumbers = new List<int> { 1 })
                .With(c => c.Quality = new QualityModel(QualityTypes.DVD, true))
                .Build()
                .ToList();

            var result = Mocker.Resolve<TestSearch>().ProcessReports(_matchingSeries, new { }, parseResults, _searchHistory, _notification);

            result.SearchHistoryItems.Should().HaveCount(parseResults.Count);
            result.SearchHistoryItems.Should().NotContain(s => s.Success);

            Mocker.GetMock<AllowedDownloadSpecification>().Verify(c => c.IsSatisfiedBy(It.IsAny<EpisodeParseResult>()),
                                                       Times.Exactly(5));
            Mocker.GetMock<DownloadProvider>().Verify(c => c.DownloadReport(It.IsAny<EpisodeParseResult>()),
                                                      Times.Never());
        }

        [Test]
        public void should_should_skip_if_series_is_not_watched()
        {
            var parseResults = Builder<EpisodeParseResult>.CreateListOfSize(5)
                .All()
                .With(e => e.SeasonNumber = 1)
                .With(e => e.EpisodeNumbers = new List<int> { 1 })
                .With(e => e.Quality = new QualityModel(QualityTypes.HDTV720p, false))
                .Build()
                .ToList();

            WithNullSeries();

            //Act
            var result = Mocker.Resolve<TestSearch>().ProcessReports(_matchingSeries, new { }, parseResults, _searchHistory, _notification);

            //Assert
            result.SearchHistoryItems.Should().HaveCount(parseResults.Count);
            result.SearchHistoryItems.Should().NotContain(s => s.Success);

            Mocker.GetMock<DownloadProvider>().Verify(c => c.DownloadReport(It.IsAny<EpisodeParseResult>()),
                                                      Times.Never());
        }

        [Test]
        public void should_skip_if_series_does_not_match_searched_series()
        {
            var parseResults = Builder<EpisodeParseResult>.CreateListOfSize(5)
                .All()
                .With(e => e.SeasonNumber = 1)
                .With(e => e.EpisodeNumbers = new List<int> { 1 })
                .With(e => e.Quality = new QualityModel(QualityTypes.HDTV720p, false))
                .Build()
                .ToList();

            WithMisMatchedSeries();

            //Act
            var result = Mocker.Resolve<TestSearch>().ProcessReports(_matchingSeries, new { }, parseResults, _searchHistory, _notification);

            //Assert
            result.SearchHistoryItems.Should().HaveCount(parseResults.Count);
            result.SearchHistoryItems.Should().NotContain(s => s.Success);

            Mocker.GetMock<DownloadProvider>().Verify(c => c.DownloadReport(It.IsAny<EpisodeParseResult>()),
                                                      Times.Never());
        }

        [Test]
        public void should_skip_if_episode_was_already_downloaded()
        {
            var parseResults = Builder<EpisodeParseResult>.CreateListOfSize(2)
                .All()
                .With(e => e.SeasonNumber = 1)
                .With(e => e.EpisodeNumbers = new List<int> { 5 })
                .With(c => c.Quality = new QualityModel(QualityTypes.DVD, true))
                .TheLast(1)
                .With(e => e.EpisodeNumbers = new List<int> { 1, 2, 3, 4, 5 })
                .Build()
                .ToList();

            WithMatchingSeries();
            WithQualityNeeded();
            WithSuccessfulDownload();

            //Act
            var result = Mocker.Resolve<TestSearch>().ProcessReports(_matchingSeries, new { }, parseResults, _searchHistory, _notification);

            //Assert
            result.SearchHistoryItems.Should().HaveCount(parseResults.Count);
            result.SearchHistoryItems.Should().Contain(s => s.Success);

            Mocker.GetMock<DownloadProvider>().Verify(c => c.DownloadReport(It.IsAny<EpisodeParseResult>()),
                                                      Times.Once());
        }

        [Test]
        public void should_try_next_report_if_download_fails()
        {
            var parseResults = Builder<EpisodeParseResult>.CreateListOfSize(2)
                .All()
                .With(e => e.SeasonNumber = 1)
                .With(e => e.EpisodeNumbers = new List<int> { 1 })
                .With(c => c.Quality = new QualityModel(QualityTypes.DVD, true))
                .TheLast(1)
                .With(c => c.Quality = new QualityModel(QualityTypes.SDTV, true))
                .Build()
                .ToList();

            WithMatchingSeries();
            WithQualityNeeded();

            Mocker.GetMock<DownloadProvider>()
                .Setup(s => s.DownloadReport(It.Is<EpisodeParseResult>(d => d.Quality.Quality == QualityTypes.DVD)))
                .Returns(false);

            Mocker.GetMock<DownloadProvider>()
                .Setup(s => s.DownloadReport(It.Is<EpisodeParseResult>(d => d.Quality.Quality == QualityTypes.SDTV)))
                .Returns(true);

            //Act
            var result = Mocker.Resolve<TestSearch>().ProcessReports(_matchingSeries, new { }, parseResults, _searchHistory, _notification);

            //Assert
            result.SearchHistoryItems.Should().HaveCount(parseResults.Count);
            result.SearchHistoryItems.Should().Contain(s => s.Success);

            Mocker.GetMock<DownloadProvider>().Verify(c => c.DownloadReport(It.IsAny<EpisodeParseResult>()),
                                                      Times.Exactly(2));
        }

        [Test]
        public void should_return_valid_successes_when_one_or_more_downloaded()
        {
            var parseResults = Builder<EpisodeParseResult>.CreateListOfSize(5)
                .All()
                .With(e => e.SeasonNumber = 1)
                .With(e => e.EpisodeNumbers = new List<int> { 1 })
                .With(c => c.Quality = new QualityModel(QualityTypes.DVD, true))
                .With(c => c.Age = 10)
                .Random(1)
                .With(c => c.Quality = new QualityModel(QualityTypes.Bluray1080p, true))
                .With(c => c.Age = 100)
                .Build()
                .ToList();

            WithMatchingSeries();
            WithSuccessfulDownload();

            Mocker.GetMock<AllowedDownloadSpecification>()
                .Setup(s => s.IsSatisfiedBy(It.Is<EpisodeParseResult>(d => d.Quality.Quality == QualityTypes.Bluray1080p)))
                .Returns(ReportRejectionType.None);

            //Act
            var result = Mocker.Resolve<TestSearch>().ProcessReports(_matchingSeries, new { }, parseResults, _searchHistory, _notification);

            //Assert
            result.Successes.Should().NotBeNull();
            result.Successes.Should().NotBeEmpty();

            Mocker.GetMock<AllowedDownloadSpecification>().Verify(c => c.IsSatisfiedBy(It.IsAny<EpisodeParseResult>()),
                                                       Times.Once());
            Mocker.GetMock<DownloadProvider>().Verify(c => c.DownloadReport(It.IsAny<EpisodeParseResult>()),
                                                      Times.Once());
        }
    }
}

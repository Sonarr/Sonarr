/*
using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Download;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Model;
using NzbDrone.Core.Model.Notification;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Repository.Search;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.ProviderTests.SearchTests
{
    [TestFixture]
    public class ProcessResultsFixture : CoreTest<TestSearch>
    {
        private Series _matchingSeries;
        private Series _mismatchedSeries;
        private Series _nullSeries = null;

        private EpisodeSearchResult _episodeSearchResult;
        private ProgressNotification _notification;

        private List<Episode> _episodes;

        [SetUp]
        public void Setup()
        {
            _matchingSeries = Builder<Series>.CreateNew()
                .With(s => s.Id = 79488)
                .With(s => s.Title = "30 Rock")
                .Build();

            _mismatchedSeries = Builder<Series>.CreateNew()
                .With(s => s.Id = 12345)
                .With(s => s.Title = "Not 30 Rock")
                .Build();

            _episodeSearchResult = new EpisodeSearchResult();
            _notification = new ProgressNotification("Test");

            _episodes = Builder<Episode>
                    .CreateListOfSize(1)
                    .Build().ToList();

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

        private void WithApprovedDecisions()
        {
            Mocker.GetMock<IDownloadDirector>()
                .Setup(s => s.GetDownloadDecision(It.IsAny<EpisodeParseResult>()))
                .Returns(new DownloadDecision(new string[0]));
        }

        private void WithDeclinedDecisions()
        {
            Mocker.GetMock<IDownloadDirector>()
                .Setup(s => s.GetDownloadDecision(It.IsAny<EpisodeParseResult>()))
                .Returns(new DownloadDecision(new[] { "Rejection reason" }));
        }

                                 Times.Once());
        }

     

       

        [Test]
        public void should_skip_if_series_does_not_match_searched_series()
        {
            var parseResults = Builder<EpisodeParseResult>.CreateListOfSize(5)
                .All()
                .With(e => e.SeasonNumber = 1)
                .With(e => e.EpisodeNumbers = new List<int> { 1 })
                .With(e => e.Quality = new QualityModel(Quality.HDTV720p, false))
                .Build()
                .ToList();

            WithMisMatchedSeries();

            //Act
            var result = Subject.ProcessReports(_matchingSeries, new { }, parseResults, _episodeSearchResult, _notification);

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
                .With(c => c.Quality = new QualityModel(Quality.DVD, true))
                .TheLast(1)
                .With(e => e.EpisodeNumbers = new List<int> { 1, 2, 3, 4, 5 })
                .Build()
                .ToList();

            WithMatchingSeries();
            WithQualityNeeded();
            WithSuccessfulDownload();

            //Act
            var result = Subject.ProcessReports(_matchingSeries, new { }, parseResults, _episodeSearchResult, _notification);

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
                .With(c => c.Quality = new QualityModel(Quality.DVD, true))
                .TheLast(1)
                .With(c => c.Quality = new QualityModel(Quality.SDTV, true))
                .Build()
                .ToList();

            WithMatchingSeries();
            WithQualityNeeded();

            Mocker.GetMock<DownloadProvider>()
                .Setup(s => s.DownloadReport(It.Is<EpisodeParseResult>(d => d.Quality.Quality == Quality.DVD)))
                .Returns(false);

            Mocker.GetMock<DownloadProvider>()
                .Setup(s => s.DownloadReport(It.Is<EpisodeParseResult>(d => d.Quality.Quality == Quality.SDTV)))
                .Returns(true);

            //Act
            var result = Subject.ProcessReports(_matchingSeries, new { }, parseResults, _episodeSearchResult, _notification);

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
                .With(c => c.Quality = new QualityModel(Quality.DVD, true))
                .With(c => c.Age = 10)
                .Random(1)
                .With(c => c.Quality = new QualityModel(Quality.Bluray1080p, true))
                .With(c => c.Age = 100)
                .Build()
                .ToList();

            WithMatchingSeries();
            WithSuccessfulDownload();

            Mocker.GetMock<DownloadDirector>()
                .Setup(s => s.IsDownloadPermitted(It.Is<EpisodeParseResult>(d => d.Quality.Quality == Quality.Bluray1080p)))
                .Returns(ReportRejectionReasons.None);

            //Act
            var result = Subject.ProcessReports(_matchingSeries, new { }, parseResults, _episodeSearchResult, _notification);

            //Assert
            result.Successes.Should().NotBeNull();
            result.Successes.Should().NotBeEmpty();

            Mocker.GetMock<DownloadDirector>().Verify(c => c.IsDownloadPermitted(It.IsAny<EpisodeParseResult>()),
                                                       Times.Once());
            Mocker.GetMock<DownloadProvider>().Verify(c => c.DownloadReport(It.IsAny<EpisodeParseResult>()),
                                                      Times.Once());
        }
    }
}
*/

using System;
using System.Collections.Generic;
using System.Linq;

using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Jobs;
using NzbDrone.Core.Model;
using NzbDrone.Core.Model.Notification;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Repository;
using NzbDrone.Core.Repository.Search;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common.AutoMoq;

namespace NzbDrone.Core.Test.JobTests
{
    [TestFixture]
    // ReSharper disable InconsistentNaming
    public class SeasonSearchJobTest : CoreTest
    {
        private ProgressNotification notification;

        [SetUp]
        public void Setup()
        {
             notification = new ProgressNotification("Search");
        }

        [Test]
        public void SeasonSearch_partial_season_success()
        {
            var resultItems = Builder<SearchHistoryItem>.CreateListOfSize(5)
                .All()
                .With(e => e.SearchError = ReportRejectionType.None)
                .With(e => e.Success = true)
                .Build();

            var episodes = Builder<Episode>.CreateListOfSize(5)
                .All()
                .With(e => e.SeasonNumber = 1)
                .With(e => e.SeriesId = 5)
                .Build();

            Mocker.GetMock<EpisodeProvider>()
                .Setup(c => c.GetEpisodesBySeason(1, 1)).Returns(episodes);

            Mocker.GetMock<SearchProvider>()
                .Setup(c => c.PartialSeasonSearch(notification, 1, 1))
                .Returns(episodes.Select(e => e.EpisodeNumber).ToList());

            //Act
            Mocker.Resolve<SeasonSearchJob>().Start(notification, new { SeriesId = 1, SeasonNumber = 1 });

            //Assert
            Mocker.VerifyAllMocks();
            Mocker.GetMock<SearchProvider>().Verify(c => c.PartialSeasonSearch(notification, 1, 1), Times.Once());
            Mocker.GetMock<EpisodeSearchJob>().Verify(c => c.Start(notification, new { EpisodeId =  It.IsAny<int>() }), Times.Never());
        }

        [Test]
        public void SeasonSearch_partial_season_failure()
        {
            var episodes = Builder<Episode>.CreateListOfSize(5)
                .All()
                .With(e => e.SeriesId = 1)
                .With(e => e.SeasonNumber = 1)
                .With(e => e.Ignored = false)
                .With(e => e.AirDate = DateTime.Today.AddDays(-1))
                .Build();

            Mocker.GetMock<SearchProvider>()
                .Setup(c => c.PartialSeasonSearch(notification, 1, 1))
                .Returns(new List<int>());

            //Act
            Mocker.Resolve<SeasonSearchJob>().Start(notification, new { SeriesId = 1, SeasonNumber = 1 });

            //Assert
            Mocker.VerifyAllMocks();
            Mocker.GetMock<SearchProvider>().Verify(c => c.PartialSeasonSearch(notification, 1, 1), Times.Once());
        }

        [Test]
        public void SeasonSearch_should_not_search_for_episodes_that_havent_aired_yet_or_air_tomorrow()
        {
            var episodes = Builder<Episode>.CreateListOfSize(5)
                .All()
                .With(e => e.SeriesId = 1)
                .With(e => e.SeasonNumber = 1)
                .With(e => e.Ignored = false)
                .With(e => e.AirDate = DateTime.Today.AddDays(-1))
                .TheLast(2)
                .With(e => e.AirDate = DateTime.Today.AddDays(2))
                .Build();

            Mocker.GetMock<EpisodeProvider>()
                .Setup(c => c.GetEpisodesBySeason(1, 1)).Returns(episodes);

            Mocker.GetMock<SearchProvider>()
                .Setup(c => c.PartialSeasonSearch(notification, 1, 1))
                .Returns(new List<int>{1});


            //Act
            Mocker.Resolve<SeasonSearchJob>().Start(notification, new { SeriesId = 1, SeasonNumber = 1 });

            //Assert
            Mocker.VerifyAllMocks();
            Mocker.GetMock<SearchProvider>().Verify(c => c.PartialSeasonSearch(notification, 1, 1), Times.Once());
        }

        [Test]
        public void SeasonSearch_should_allow_searching_of_season_zero()
        {
            Mocker.GetMock<SearchProvider>()
                .Setup(c => c.PartialSeasonSearch(notification, 1, 0)).Returns(new List<int>());

            //Act
            Mocker.Resolve<SeasonSearchJob>().Start(notification, new { SeriesId = 1, SeasonNumber = 0 });

            //Assert
            Mocker.GetMock<SearchProvider>().Verify(c => c.PartialSeasonSearch(notification, 1, 1), Times.Never());
            Mocker.GetMock<EpisodeSearchJob>().Verify(c => c.Start(notification, new { EpisodeId = It.IsAny<int>() }), Times.Never());
        }
    }
}
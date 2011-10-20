using System;
using System.Collections.Generic;
using System.Linq;
using AutoMoq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Model;
using NzbDrone.Core.Model.Notification;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Providers.Indexer;
using NzbDrone.Core.Providers.Jobs;
using NzbDrone.Core.Repository;
using NzbDrone.Core.Repository.Quality;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test
{
    [TestFixture]
    // ReSharper disable InconsistentNaming
    public class SeasonSearchJobTest : TestBase
    {
        [Test]
        public void SeasonSearch_full_season_success()
        {
            var mocker = new AutoMoqer(MockBehavior.Strict);

            var notification = new ProgressNotification("Season Search");

            mocker.GetMock<SearchProvider>()
                .Setup(c => c.SeasonSearch(notification, 1, 1)).Returns(true);

            //Act
            mocker.Resolve<SeasonSearchJob>().Start(notification, 1, 1);

            //Assert
            mocker.VerifyAllMocks();
            mocker.GetMock<SearchProvider>().Verify(c => c.SeasonSearch(notification, 1, 1), Times.Once());
            mocker.GetMock<SearchProvider>().Verify(c => c.PartialSeasonSearch(notification, 1, 1), Times.Never());
            mocker.GetMock<EpisodeSearchJob>().Verify(c => c.Start(notification, It.IsAny<int>(), 0), Times.Never());
        }

        [Test]
        public void SeasonSearch_partial_season_success()
        {
            var episodes = Builder<Episode>.CreateListOfSize(5)
                .All()
                .With(e => e.SeriesId = 1)
                .With(e => e.SeasonNumber = 1)
                .Build();

            var mocker = new AutoMoqer(MockBehavior.Strict);

            var notification = new ProgressNotification("Season Search");

            mocker.GetMock<SearchProvider>()
                .Setup(c => c.SeasonSearch(notification, 1, 1)).Returns(false);

            mocker.GetMock<EpisodeProvider>()
                .Setup(c => c.GetEpisodesBySeason(1, 1)).Returns(episodes);

            mocker.GetMock<SearchProvider>()
                .Setup(c => c.PartialSeasonSearch(notification, 1, 1))
                .Returns(episodes.Select(e => e.EpisodeNumber).ToList());

            //Act
            mocker.Resolve<SeasonSearchJob>().Start(notification, 1, 1);

            //Assert
            mocker.VerifyAllMocks();
            mocker.GetMock<SearchProvider>().Verify(c => c.SeasonSearch(notification, 1, 1), Times.Once());
            mocker.GetMock<SearchProvider>().Verify(c => c.PartialSeasonSearch(notification, 1, 1), Times.Once());
            mocker.GetMock<EpisodeSearchJob>().Verify(c => c.Start(notification, It.IsAny<int>(), 0), Times.Never());
        }

        [Test]
        public void SeasonSearch_partial_season_failure()
        {
            var episodes = Builder<Episode>.CreateListOfSize(5)
                .All()
                .With(e => e.SeriesId = 1)
                .With(e => e.SeasonNumber = 1)
                .With(e => e.Ignored = false)
                .Build();

            var mocker = new AutoMoqer(MockBehavior.Strict);

            var notification = new ProgressNotification("Season Search");

            mocker.GetMock<SearchProvider>()
                .Setup(c => c.SeasonSearch(notification, 1, 1)).Returns(false);

            mocker.GetMock<EpisodeProvider>()
                .Setup(c => c.GetEpisodesBySeason(1, 1)).Returns(episodes);

            mocker.GetMock<SearchProvider>()
                .Setup(c => c.PartialSeasonSearch(notification, 1, 1))
                .Returns(new List<int>{1});

            mocker.GetMock<EpisodeSearchJob>()
                .Setup(c => c.Start(notification, It.IsAny<int>(), 0)).Verifiable();

            //Act
            mocker.Resolve<SeasonSearchJob>().Start(notification, 1, 1);

            //Assert
            mocker.VerifyAllMocks();
            mocker.GetMock<SearchProvider>().Verify(c => c.SeasonSearch(notification, 1, 1), Times.Once());
            mocker.GetMock<SearchProvider>().Verify(c => c.PartialSeasonSearch(notification, 1, 1), Times.Once());
            mocker.GetMock<EpisodeSearchJob>().Verify(c => c.Start(notification, It.IsAny<int>(), 0), Times.Exactly(4));
        }
    }
}
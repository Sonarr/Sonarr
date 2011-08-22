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
        public void SeasonSearch_success()
        {
            var episodes = Builder<Episode>.CreateListOfSize(5)
                .WhereAll()
                .Have(e => e.SeriesId = 1)
                .Have(e => e.SeasonNumber = 1)
                .Build();

            var mocker = new AutoMoqer(MockBehavior.Strict);

            var notification = new ProgressNotification("Season Search");

            mocker.GetMock<EpisodeProvider>()
                .Setup(c => c.GetEpisodesBySeason(1, 1)).Returns(episodes);

            mocker.GetMock<EpisodeSearchJob>()
                .Setup(c => c.Start(notification, It.IsAny<int>(), 0)).Verifiable();

            //Act
            mocker.Resolve<SeasonSearchJob>().Start(notification, 1, 1);

            //Assert
            mocker.VerifyAllMocks();
            mocker.GetMock<EpisodeSearchJob>().Verify(c => c.Start(notification, It.IsAny<int>(), 0),
                                                       Times.Exactly(episodes.Count));
        }

        [Test]
        public void SeasonSearch_no_episodes()
        {
            var mocker = new AutoMoqer(MockBehavior.Strict);
            var notification = new ProgressNotification("Season Search");
            List<Episode> nullList = null;

            mocker.GetMock<EpisodeProvider>()
                .Setup(c => c.GetEpisodesBySeason(1, 1)).Returns(nullList);

            //Act
            mocker.Resolve<SeasonSearchJob>().Start(notification, 1, 1);

            //Assert
            mocker.VerifyAllMocks();
            mocker.GetMock<EpisodeSearchJob>().Verify(c => c.Start(notification, It.IsAny<int>(), 0),
                                                       Times.Never());
            ExceptionVerification.ExcpectedWarns(1);
        }
    }
}
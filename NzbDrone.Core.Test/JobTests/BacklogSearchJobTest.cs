using System.Collections.Generic;
using System.Linq;

using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Jobs;
using NzbDrone.Core.Model.Notification;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Repository;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common.AutoMoq;

namespace NzbDrone.Core.Test.JobTests
{
    [TestFixture]
    public class BacklogSearchJobTest : CoreTest
    {
        [Test]
        public void no_missing_epsiodes_should_not_trigger_any_search()
        {
            //Setup
            var notification = new ProgressNotification("Backlog Search Job Test");

            var episodes = new List<Episode>();

            WithStrictMocker();

            Mocker.GetMock<EpisodeProvider>()
                .Setup(s => s.EpisodesWithoutFiles(true)).Returns(episodes);

            //Act
            Mocker.Resolve<BacklogSearchJob>().Start(notification, 0, 0);

            //Assert
            Mocker.GetMock<SeasonSearchJob>().Verify(c => c.Start(notification, It.IsAny<int>(), It.IsAny<int>()),
                                                       Times.Never());

            Mocker.GetMock<EpisodeSearchJob>().Verify(c => c.Start(notification, It.IsAny<int>(), 0),
                                                       Times.Never());
        }

        [Test]
        public void individual_missing_episode()
        {
            //Setup
            var notification = new ProgressNotification("Backlog Search Job Test");

            var episodes = Builder<Episode>.CreateListOfSize(1).Build();

            WithStrictMocker();

            Mocker.GetMock<EpisodeProvider>()
                .Setup(s => s.EpisodesWithoutFiles(true)).Returns(episodes);

            Mocker.GetMock<EpisodeSearchJob>()
                .Setup(s => s.Start(notification, It.IsAny<int>(), 0)).Verifiable();

            //Act
            Mocker.Resolve<BacklogSearchJob>().Start(notification, 0, 0);

            //Assert
            Mocker.GetMock<SeasonSearchJob>().Verify(c => c.Start(notification, It.IsAny<int>(), It.IsAny<int>()),
                                                       Times.Never());

            Mocker.GetMock<EpisodeSearchJob>().Verify(c => c.Start(notification, It.IsAny<int>(), 0),
                                                       Times.Once());
        }

        [Test]
        public void individual_missing_episodes_only()
        {
            //Setup
            var notification = new ProgressNotification("Backlog Search Job Test");

            var episodes = Builder<Episode>.CreateListOfSize(5).Build();

            WithStrictMocker();

            Mocker.GetMock<EpisodeProvider>()
                .Setup(s => s.EpisodesWithoutFiles(true)).Returns(episodes);

            Mocker.GetMock<EpisodeSearchJob>()
                .Setup(s => s.Start(notification, It.IsAny<int>(), 0)).Verifiable();

            //Act
            Mocker.Resolve<BacklogSearchJob>().Start(notification, 0, 0);

            //Assert
            Mocker.GetMock<SeasonSearchJob>().Verify(c => c.Start(notification, It.IsAny<int>(), It.IsAny<int>()),
                                                       Times.Never());

            Mocker.GetMock<EpisodeSearchJob>().Verify(c => c.Start(notification, It.IsAny<int>(), 0),
                                                       Times.Exactly(episodes.Count));
        }

        [Test]
        public void series_season_missing_episodes_only_mismatch_count()
        {
            //Setup
            var notification = new ProgressNotification("Backlog Search Job Test");

            var episodes = Builder<Episode>.CreateListOfSize(5)
                .All()
                .With(e => e.SeriesId = 1)
                .With(e => e.SeasonNumber = 1)
                .Build();

            WithStrictMocker();

            Mocker.GetMock<EpisodeProvider>()
                .Setup(s => s.EpisodesWithoutFiles(true)).Returns(episodes);

            Mocker.GetMock<EpisodeSearchJob>()
                .Setup(s => s.Start(notification, It.IsAny<int>(), 0)).Verifiable();

            Mocker.GetMock<EpisodeProvider>()
                .Setup(s => s.GetEpisodeNumbersBySeason(1, 1)).Returns(new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 });

            //Act
            Mocker.Resolve<BacklogSearchJob>().Start(notification, 0, 0);

            //Assert
            Mocker.GetMock<SeasonSearchJob>().Verify(c => c.Start(notification, It.IsAny<int>(), It.IsAny<int>()),
                                                       Times.Never());

            Mocker.GetMock<EpisodeSearchJob>().Verify(c => c.Start(notification, It.IsAny<int>(), 0),
                                                       Times.Exactly(episodes.Count));
        }

        [Test]
        public void series_season_missing_episodes_only()
        {
            //Setup
            var notification = new ProgressNotification("Backlog Search Job Test");

            var episodes = Builder<Episode>.CreateListOfSize(5)
                .All()
                .With(e => e.SeriesId = 1)
                .With(e => e.SeasonNumber = 1)
                .Build();

            WithStrictMocker();

            Mocker.GetMock<EpisodeProvider>()
                .Setup(s => s.EpisodesWithoutFiles(true)).Returns(episodes);

            Mocker.GetMock<SeasonSearchJob>()
                .Setup(s => s.Start(notification, It.IsAny<int>(), It.IsAny<int>())).Verifiable();

            Mocker.GetMock<EpisodeProvider>()
                .Setup(s => s.GetEpisodeNumbersBySeason(1, 1)).Returns(episodes.Select(e => e.EpisodeNumber).ToList());

            //Act
            Mocker.Resolve<BacklogSearchJob>().Start(notification, 0, 0);

            //Assert
            Mocker.GetMock<SeasonSearchJob>().Verify(c => c.Start(notification, It.IsAny<int>(), It.IsAny<int>()),
                                                       Times.Once());

            Mocker.GetMock<EpisodeSearchJob>().Verify(c => c.Start(notification, It.IsAny<int>(), 0),
                                                       Times.Never());
        }

        [Test]
        public void multiple_missing_episodes()
        {
            //Setup
            var notification = new ProgressNotification("Backlog Search Job Test");

            var episodes = Builder<Episode>.CreateListOfSize(10)
                .TheFirst(5)
                .With(e => e.SeriesId = 1)
                .With(e => e.SeasonNumber = 1)
                .Build();

            WithStrictMocker();

            Mocker.GetMock<EpisodeProvider>()
                .Setup(s => s.EpisodesWithoutFiles(true)).Returns(episodes);

            Mocker.GetMock<SeasonSearchJob>()
                .Setup(s => s.Start(notification, It.IsAny<int>(), It.IsAny<int>())).Verifiable();

            Mocker.GetMock<EpisodeSearchJob>()
                .Setup(s => s.Start(notification, It.IsAny<int>(), 0)).Verifiable();

            Mocker.GetMock<EpisodeProvider>()
                .Setup(s => s.GetEpisodeNumbersBySeason(1, 1)).Returns(new List<int> { 1, 2, 3, 4, 5 });

            //Act
            Mocker.Resolve<BacklogSearchJob>().Start(notification, 0, 0);

            //Assert
            Mocker.GetMock<SeasonSearchJob>().Verify(c => c.Start(notification, It.IsAny<int>(), It.IsAny<int>()),
                                                       Times.Once());

            Mocker.GetMock<EpisodeSearchJob>().Verify(c => c.Start(notification, It.IsAny<int>(), 0),
                                                       Times.Exactly(5));
        }
    }
}

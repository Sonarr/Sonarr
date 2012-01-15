using System;
using System.Collections.Generic;
using System.Linq;

using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Jobs;
using NzbDrone.Core.Model.Notification;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Providers.Core;
using NzbDrone.Core.Repository;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common.AutoMoq;

namespace NzbDrone.Core.Test.JobTests
{
    [TestFixture]
    public class RecentBacklogSearchJobTest : CoreTest
    {
        private void WithEnableBacklogSearching()
        {
            Mocker.GetMock<ConfigProvider>().SetupGet(s => s.EnableBacklogSearching).Returns(true);
        }

        [SetUp]
        public void Setup()
        {
            
        }

        [Test]
        public void no_missing_epsiodes_should_not_trigger_any_search()
        {
            //Setup
            var episodes = new List<Episode>();

            Mocker.GetMock<EpisodeProvider>()
                .Setup(s => s.EpisodesWithoutFiles(true)).Returns(episodes);

            //Act
            Mocker.Resolve<RecentBacklogSearchJob>().Start(MockNotification, 0, 0);

            //Assert
            Mocker.GetMock<EpisodeSearchJob>().Verify(c => c.Start(MockNotification, It.IsAny<int>(), 0),
                                                       Times.Never());
        }

        [Test]
        public void should_only_process_missing_episodes_from_the_last_30_days()
        {
            WithEnableBacklogSearching();

            //Setup
            var episodes = Builder<Episode>.CreateListOfSize(50)
                .TheFirst(5)
                .With(e => e.AirDate = DateTime.Today)
                .TheNext(5)
                .With(e => e.AirDate = DateTime.Today.AddDays(-1)) //Today
                .TheNext(5)
                .With(e => e.AirDate = DateTime.Today.AddDays(-5)) //Yeserday
                .TheNext(5)
                .With(e => e.AirDate = DateTime.Today.AddDays(-10))
                .TheNext(5)
                .With(e => e.AirDate = DateTime.Today.AddDays(-15))
                .TheNext(5)
                .With(e => e.AirDate = DateTime.Today.AddDays(-20))
                .TheNext(5)
                .With(e => e.AirDate = DateTime.Today.AddDays(-25))
                .TheNext(5)
                .With(e => e.AirDate = DateTime.Today.AddDays(-30))
                .TheNext(5)
                .With(e => e.AirDate = DateTime.Today.AddDays(-31)) //31 Days
                .TheNext(5)
                .With(e => e.AirDate = DateTime.Today.AddDays(-35))
                .Build();

            Mocker.GetMock<EpisodeProvider>()
                .Setup(s => s.EpisodesWithoutFiles(true)).Returns(episodes);

            Mocker.GetMock<EpisodeSearchJob>().Setup(c => c.Start(It.IsAny<ProgressNotification>(), It.IsAny<int>(), 0));

            //Act
            Mocker.Resolve<RecentBacklogSearchJob>().Start(MockNotification, 0, 0);

            //Assert
            Mocker.GetMock<EpisodeSearchJob>().Verify(c => c.Start(It.IsAny<ProgressNotification>(), It.IsAny<int>(), 0),
                                                       Times.Exactly(40));
        }
    }
}

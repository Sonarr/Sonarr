using System.Collections.Generic;
using System.Linq;

using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Jobs;
using NzbDrone.Core.Model;
using NzbDrone.Core.Model.Notification;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Providers.Core;
using NzbDrone.Core.Repository;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common.AutoMoq;

namespace NzbDrone.Core.Test.JobTests
{
    [TestFixture]
    public class BacklogSearchJobTest : CoreTest
    {
        private void WithEnableBacklogSearching()
        {
            Mocker.GetMock<ConfigProvider>().SetupGet(s => s.EnableBacklogSearching).Returns(true);
        }

        [Test]
        public void no_missing_epsiodes_should_not_trigger_any_search()
        {
            //Setup
            var notification = new ProgressNotification("Backlog Search Job Test");

            var episodes = new List<Episode>();

            WithStrictMocker();
            WithEnableBacklogSearching();

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

            var series = Builder<Series>.CreateNew()
                    .With(s => s.Monitored = true)
                    .With(s => s.BacklogStatus = BacklogStatusType.Enable)
                    .Build();

            var episodes = Builder<Episode>.CreateListOfSize(1)
                .All()
                .With(e => e.Series = series)
                .Build();

            WithStrictMocker();
            WithEnableBacklogSearching();

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

            var series = Builder<Series>.CreateNew()
                    .With(s => s.Monitored = true)
                    .With(s => s.BacklogStatus = BacklogStatusType.Enable)
                    .Build();

            var episodes = Builder<Episode>.CreateListOfSize(5)
                .All()
                .With(e => e.Series = series)
                .Build();

            WithStrictMocker();
            WithEnableBacklogSearching();

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

            var series = Builder<Series>.CreateNew()
                    .With(s => s.Monitored = true)
                    .With(s => s.BacklogStatus = BacklogStatusType.Enable)
                    .Build();

            var episodes = Builder<Episode>.CreateListOfSize(5)
                .All()
                .With(e => e.Series = series)
                .With(e => e.SeasonNumber = 1)
                .Build();

            WithStrictMocker();
            WithEnableBacklogSearching();

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

            var series = Builder<Series>.CreateNew()
                    .With(s => s.Monitored = true)
                    .With(s => s.BacklogStatus = BacklogStatusType.Enable)
                    .Build();

            var episodes = Builder<Episode>.CreateListOfSize(5)
                .All()
                .With(e => e.Series = series)
                .With(e => e.SeriesId = series.SeriesId)
                .With(e => e.SeasonNumber = 1)
                .Build();

            WithStrictMocker();
            WithEnableBacklogSearching();

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

            var series = Builder<Series>.CreateNew()
                    .With(s => s.Monitored = true)
                    .With(s => s.BacklogStatus = BacklogStatusType.Enable)
                    .Build();

            var series2 = Builder<Series>.CreateNew()
                    .With(s => s.Monitored = true)
                    .With(s => s.BacklogStatus = BacklogStatusType.Enable)
                    .Build();

            var episodes = Builder<Episode>.CreateListOfSize(10)
                .TheFirst(5)
                .With(e => e.Series = series)
                .With(e => e.SeriesId = series.SeriesId)
                .With(e => e.SeasonNumber = 1)
                .TheNext(5)
                .With(e => e.Series = series2)
                .Build();

            WithStrictMocker();
            WithEnableBacklogSearching();

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

        [Test]
        public void GetMissingForEnabledSeries_should_only_return_episodes_for_monitored_series()
        {
            //Setup
            var series = Builder<Series>.CreateListOfSize(2)
                .TheFirst(1)
                .With(s => s.Monitored = false)
                .With(s => s.BacklogStatus = BacklogStatusType.Enable)
                .TheNext(1)
                .With(s => s.Monitored = true)
                .With(s => s.BacklogStatus = BacklogStatusType.Enable)
                .Build();

            var episodes = Builder<Episode>.CreateListOfSize(11)
                .TheFirst(5)
                .With(e => e.Series = series[0])
                .With(e => e.SeasonNumber = 1)
                .TheLast(6)
                .With(e => e.Series = series[1])
                .Build();

            WithEnableBacklogSearching();

            Mocker.GetMock<EpisodeProvider>()
                .Setup(s => s.EpisodesWithoutFiles(true)).Returns(episodes);

            //Act
            var result = Mocker.Resolve<BacklogSearchJob>().GetMissingForEnabledSeries();

            //Assert
            result.Should().NotBeEmpty();
            result.Should().Contain(s => s.Series.Monitored);
            result.Should().NotContain(s => !s.Series.Monitored);
        }

        [Test]
        public void GetMissingForEnabledSeries_should_only_return_explicity_enabled_series_when_backlog_searching_is_ignored()
        {
            //Setup
            var series = Builder<Series>.CreateListOfSize(3)
                .TheFirst(1)
                .With(s => s.Monitored = true)
                .With(s => s.BacklogStatus = BacklogStatusType.Disable)
                .TheNext(1)
                .With(s => s.Monitored = true)
                .With(s => s.BacklogStatus = BacklogStatusType.Enable)
                .TheNext(1)
                .With(s => s.Monitored = true)
                .With(s => s.BacklogStatus = BacklogStatusType.Inherit)
                .Build();

            var episodes = Builder<Episode>.CreateListOfSize(12)
                .TheFirst(3)
                .With(e => e.Series = series[0])
                .TheNext(4)
                .With(e => e.Series = series[1])
                .TheNext(5)
                .With(e => e.Series = series[2])
                .Build();

            //WithEnableBacklogSearching();

            Mocker.GetMock<EpisodeProvider>()
                .Setup(s => s.EpisodesWithoutFiles(true)).Returns(episodes);

            //Act
            var result = Mocker.Resolve<BacklogSearchJob>().GetMissingForEnabledSeries();

            //Assert
            result.Should().NotBeEmpty();
            result.Should().Contain(s => s.Series.BacklogStatus == BacklogStatusType.Enable);
            result.Should().NotContain(s => s.Series.BacklogStatus == BacklogStatusType.Disable);
            result.Should().NotContain(s => s.Series.BacklogStatus == BacklogStatusType.Inherit);
        }

        [Test]
        public void GetMissingForEnabledSeries_should_return_explicity_enabled_and_inherit_series_when_backlog_searching_is_enabled()
        {
            //Setup
            var series = Builder<Series>.CreateListOfSize(3)
                .TheFirst(1)
                .With(s => s.Monitored = true)
                .With(s => s.BacklogStatus = BacklogStatusType.Disable)
                .TheNext(1)
                .With(s => s.Monitored = true)
                .With(s => s.BacklogStatus = BacklogStatusType.Enable)
                .TheNext(1)
                .With(s => s.Monitored = true)
                .With(s => s.BacklogStatus = BacklogStatusType.Inherit)
                .Build();

            var episodes = Builder<Episode>.CreateListOfSize(12)
                .TheFirst(3)
                .With(e => e.Series = series[0])
                .TheNext(4)
                .With(e => e.Series = series[1])
                .TheNext(5)
                .With(e => e.Series = series[2])
                .Build();

            WithEnableBacklogSearching();

            Mocker.GetMock<EpisodeProvider>()
                .Setup(s => s.EpisodesWithoutFiles(true)).Returns(episodes);

            //Act
            var result = Mocker.Resolve<BacklogSearchJob>().GetMissingForEnabledSeries();

            //Assert
            result.Should().NotBeEmpty();
            result.Should().Contain(s => s.Series.BacklogStatus == BacklogStatusType.Enable);
            result.Should().NotContain(s => s.Series.BacklogStatus == BacklogStatusType.Disable);
            result.Should().Contain(s => s.Series.BacklogStatus == BacklogStatusType.Inherit);
        }
    }
}

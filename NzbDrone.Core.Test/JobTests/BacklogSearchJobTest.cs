using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Jobs.Implementations;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Jobs;
using NzbDrone.Core.Model;
using NzbDrone.Core.Model.Notification;
using NzbDrone.Core.Providers;

using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.JobTests
{
    [TestFixture]
    public class BacklogSearchJobTest : CoreTest<BacklogSearchJob>
    {
        private void WithEnableBacklogSearching()
        {
            Mocker.GetMock<IConfigService>().SetupGet(s => s.EnableBacklogSearching).Returns(true);
        }

        [Test]
        public void no_missing_epsiodes_should_not_trigger_any_search()
        {
            //Setup
            var notification = new ProgressNotification("Backlog Search Job Test");

            var episodes = new List<Episode>();

            WithStrictMocker();
            WithEnableBacklogSearching();

            Mocker.GetMock<IEpisodeService>()
                .Setup(s => s.EpisodesWithoutFiles(true)).Returns(episodes);

            //Act
            Subject.Start(notification, null);

            //Assert
            Mocker.GetMock<SeasonSearchJob>().Verify(c => c.Start(notification, new { SeriesId = It.IsAny<int>(), SeasonNumber = It.IsAny<int>() }),
                                                       Times.Never());

            Mocker.GetMock<EpisodeSearchJob>().Verify(c => c.Start(notification, new { SeriesId = It.IsAny<int>(), SeasonNumber = 0 }),
                                                       Times.Never());
        }

        [Test]
        public void individual_missing_episode()
        {
            //Setup
            var notification = new ProgressNotification("Backlog Search Job Test");

            var series = Builder<Series>.CreateNew()
                    .With(s => s.Monitored = true)
                    .With(s => s.BacklogSetting = BacklogSettingType.Enable)
                    .Build();

            var episodes = Builder<Episode>.CreateListOfSize(1)
                .All()
                .With(e => e.Series = series)
                .Build().ToList();

            WithEnableBacklogSearching();

            Mocker.GetMock<IEpisodeService>()
                .Setup(s => s.EpisodesWithoutFiles(true)).Returns(episodes);

            Mocker.GetMock<EpisodeSearchJob>()
                .Setup(s => s.Start(notification, It.Is<object>(d => d.GetPropertyValue<int>("EpisodeId") == 1)));

            //Act
            Subject.Start(notification, null);

            //Assert
            Mocker.GetMock<EpisodeSearchJob>().Verify(c => c.Start(notification, It.Is<object>(d => d.GetPropertyValue<int>("EpisodeId") >= 0)),
                                                       Times.Once());
        }

        [Test]
        public void individual_missing_episodes_only()
        {
            //Setup
            var notification = new ProgressNotification("Backlog Search Job Test");

            var series = Builder<Series>.CreateNew()
                    .With(s => s.Monitored = true)
                    .With(s => s.BacklogSetting = BacklogSettingType.Enable)
                    .Build();

            var episodes = Builder<Episode>.CreateListOfSize(5)
                .All()
                .With(e => e.Series = series)
                .Build().ToList();

            WithEnableBacklogSearching();

            Mocker.GetMock<IEpisodeService>()
                .Setup(s => s.EpisodesWithoutFiles(true)).Returns(episodes);

            //Act
            Subject.Start(notification, null);

            //Assert
            Mocker.GetMock<EpisodeSearchJob>().Verify(c => c.Start(notification, It.Is<object>(d => d.GetPropertyValue<int>("EpisodeId") >= 0)),
                                                       Times.Exactly(episodes.Count));
        }

        [Test]
        public void series_season_missing_episodes_only_mismatch_count()
        {
            //Setup
            var notification = new ProgressNotification("Backlog Search Job Test");

            var series = Builder<Series>.CreateNew()
                    .With(s => s.Monitored = true)
                    .With(s => s.BacklogSetting = BacklogSettingType.Enable)
                    .Build();

            var episodes = Builder<Episode>.CreateListOfSize(5)
                .All()
                .With(e => e.Series = series)
                .With(e => e.SeasonNumber = 1)
                .Build().ToList();

            WithEnableBacklogSearching();

            Mocker.GetMock<IEpisodeService>()
                .Setup(s => s.EpisodesWithoutFiles(true)).Returns(episodes);

            Mocker.GetMock<IEpisodeService>()
                .Setup(s => s.GetEpisodeNumbersBySeason(1, 1)).Returns(new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 });

            //Act
            Subject.Start(notification, null);

            //Assert
            Mocker.GetMock<EpisodeSearchJob>().Verify(c => c.Start(notification, It.Is<object>(d => d.GetPropertyValue<int>("EpisodeId") >= 0)),
                                       Times.Exactly(episodes.Count));
        }

        [Test]
        public void series_season_missing_episodes_only()
        {
            //Setup
            var notification = new ProgressNotification("Backlog Search Job Test");

            var series = Builder<Series>.CreateNew()
                    .With(s => s.Monitored = true)
                    .With(s => s.BacklogSetting = BacklogSettingType.Enable)
                    .Build();

            var episodes = Builder<Episode>.CreateListOfSize(5)
                .All()
                .With(e => e.Series = series)
                .With(e => e.SeriesId = series.Id)
                .With(e => e.SeasonNumber = 1)
                .Build().ToList();

            WithEnableBacklogSearching();

            Mocker.GetMock<IEpisodeService>()
                .Setup(s => s.EpisodesWithoutFiles(true)).Returns(episodes);

            Mocker.GetMock<IEpisodeService>()
                .Setup(s => s.GetEpisodeNumbersBySeason(1, 1)).Returns(episodes.Select(e => e.EpisodeNumber).ToList());

            //Act
            Subject.Start(notification, null);

            //Assert
            Mocker.GetMock<SeasonSearchJob>().Verify(c => c.Start(notification, It.Is<object>(d => d.GetPropertyValue<int>("SeriesId") >= 0 &&
                                                                                                   d.GetPropertyValue<int>("SeasonNumber") >= 0)),
                                                       Times.Once());
        }

        [Test]
        public void multiple_missing_episodes()
        {
            //Setup
            var notification = new ProgressNotification("Backlog Search Job Test");

            var series = Builder<Series>.CreateNew()
                    .With(s => s.Monitored = true)
                    .With(s => s.BacklogSetting = BacklogSettingType.Enable)
                    .Build();

            var series2 = Builder<Series>.CreateNew()
                    .With(s => s.Monitored = true)
                    .With(s => s.BacklogSetting = BacklogSettingType.Enable)
                    .Build();

            var episodes = Builder<Episode>.CreateListOfSize(10)
                .TheFirst(5)
                .With(e => e.Series = series)
                .With(e => e.SeriesId = series.Id)
                .With(e => e.SeasonNumber = 1)
                .TheNext(5)
                .With(e => e.Series = series2)
                .Build().ToList();

            WithEnableBacklogSearching();

            Mocker.GetMock<IEpisodeService>()
                .Setup(s => s.EpisodesWithoutFiles(true)).Returns(episodes);

            Mocker.GetMock<IEpisodeService>()
                .Setup(s => s.GetEpisodeNumbersBySeason(1, 1)).Returns(new List<int> { 1, 2, 3, 4, 5 });

            //Act
            Subject.Start(notification, null);

            //Assert
            Mocker.GetMock<SeasonSearchJob>().Verify(c => c.Start(notification, It.Is<object>(d => d.GetPropertyValue<int>("SeriesId") >= 0 &&
                                                                                                   d.GetPropertyValue<int>("SeasonNumber") >= 0)),
                                                       Times.Once());

            Mocker.GetMock<EpisodeSearchJob>().Verify(c => c.Start(notification, It.Is<object>(d => d.GetPropertyValue<int>("EpisodeId") >= 0)),
                                       Times.Exactly(5));
        }

        [Test]
        public void GetMissingForEnabledSeries_should_only_return_episodes_for_monitored_series()
        {
            //Setup
            var series = Builder<Series>.CreateListOfSize(2)
                .TheFirst(1)
                .With(s => s.Monitored = false)
                .With(s => s.BacklogSetting = BacklogSettingType.Enable)
                .TheNext(1)
                .With(s => s.Monitored = true)
                .With(s => s.BacklogSetting = BacklogSettingType.Enable)
                .Build();

            var episodes = Builder<Episode>.CreateListOfSize(11)
                .TheFirst(5)
                .With(e => e.Series = series[0])
                .With(e => e.SeasonNumber = 1)
                .TheLast(6)
                .With(e => e.Series = series[1])
                .Build().ToList();

            WithEnableBacklogSearching();

            Mocker.GetMock<IEpisodeService>()
                .Setup(s => s.EpisodesWithoutFiles(true)).Returns(episodes);

            //Act
            var result = Subject.GetMissingForEnabledSeries();

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
                .With(s => s.BacklogSetting = BacklogSettingType.Disable)
                .TheNext(1)
                .With(s => s.Monitored = true)
                .With(s => s.BacklogSetting = BacklogSettingType.Enable)
                .TheNext(1)
                .With(s => s.Monitored = true)
                .With(s => s.BacklogSetting = BacklogSettingType.Inherit)
                .Build();

            var episodes = Builder<Episode>.CreateListOfSize(12)
                .TheFirst(3)
                .With(e => e.Series = series[0])
                .TheNext(4)
                .With(e => e.Series = series[1])
                .TheNext(5)
                .With(e => e.Series = series[2])
                .Build().ToList();

            //WithEnableBacklogSearching();

            Mocker.GetMock<IEpisodeService>()
                .Setup(s => s.EpisodesWithoutFiles(true)).Returns(episodes);

            //Act
            var result = Subject.GetMissingForEnabledSeries();

            //Assert
            result.Should().NotBeEmpty();
            result.Should().Contain(s => s.Series.BacklogSetting == BacklogSettingType.Enable);
            result.Should().NotContain(s => s.Series.BacklogSetting == BacklogSettingType.Disable);
            result.Should().NotContain(s => s.Series.BacklogSetting == BacklogSettingType.Inherit);
        }

        [Test]
        public void GetMissingForEnabledSeries_should_return_explicity_enabled_and_inherit_series_when_backlog_searching_is_enabled()
        {
            //Setup
            var series = Builder<Series>.CreateListOfSize(3)
                .TheFirst(1)
                .With(s => s.Monitored = true)
                .With(s => s.BacklogSetting = BacklogSettingType.Disable)
                .TheNext(1)
                .With(s => s.Monitored = true)
                .With(s => s.BacklogSetting = BacklogSettingType.Enable)
                .TheNext(1)
                .With(s => s.Monitored = true)
                .With(s => s.BacklogSetting = BacklogSettingType.Inherit)
                .Build();

            var episodes = Builder<Episode>.CreateListOfSize(12)
                .TheFirst(3)
                .With(e => e.Series = series[0])
                .TheNext(4)
                .With(e => e.Series = series[1])
                .TheNext(5)
                .With(e => e.Series = series[2])
                .Build().ToList();

            WithEnableBacklogSearching();

            Mocker.GetMock<IEpisodeService>()
                .Setup(s => s.EpisodesWithoutFiles(true)).Returns(episodes);

            //Act
            var result = Subject.GetMissingForEnabledSeries();

            //Assert
            result.Should().NotBeEmpty();
            result.Should().Contain(s => s.Series.BacklogSetting == BacklogSettingType.Enable);
            result.Should().NotContain(s => s.Series.BacklogSetting == BacklogSettingType.Disable);
            result.Should().Contain(s => s.Series.BacklogSetting == BacklogSettingType.Inherit);
        }
    }
}

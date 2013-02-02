using System;
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
using NzbDrone.Test.Common;
using NzbDrone.Test.Common.AutoMoq;

namespace NzbDrone.Core.Test.JobTests
{
    [TestFixture]
    public class RecentBacklogSearchJobTest : SqlCeTest
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
            Mocker.Resolve<RecentBacklogSearchJob>().Start(MockNotification, null);

            //Assert
            Mocker.GetMock<EpisodeSearchJob>().Verify(c => c.Start(MockNotification, new { EpisodeId = It.IsAny<int>() }),
                                                       Times.Never());
        }

        [Test]
        public void should_only_process_missing_episodes_from_the_last_30_days()
        {
            WithEnableBacklogSearching();

            var series = Builder<Series>.CreateNew()
                    .With(s => s.Monitored = true)
                    .With(s => s.BacklogSetting = BacklogSettingType.Enable)
                    .Build();

            //Setup
            var episodes = Builder<Episode>.CreateListOfSize(50)
                .All()
                .With(e => e.Series = series)
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

            Mocker.GetMock<EpisodeSearchJob>().Setup(c => c.Start(It.IsAny<ProgressNotification>(), It.Is<object>(d => d.GetPropertyValue<int>("EpisodeId") >= 0)));

            //Act
            Mocker.Resolve<RecentBacklogSearchJob>().Start(MockNotification, null);

            //Assert
            Mocker.GetMock<EpisodeSearchJob>().Verify(c => c.Start(It.IsAny<ProgressNotification>(), It.Is<object>(d => d.GetPropertyValue<int>("EpisodeId") >= 0)),
                                                       Times.Exactly(40));
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
                .Build();

            WithEnableBacklogSearching();

            Mocker.GetMock<EpisodeProvider>()
                .Setup(s => s.EpisodesWithoutFiles(true)).Returns(episodes);

            //Act
            var result = Mocker.Resolve<RecentBacklogSearchJob>().GetMissingForEnabledSeries();

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
                .Build();

            //WithEnableBacklogSearching();

            Mocker.GetMock<EpisodeProvider>()
                .Setup(s => s.EpisodesWithoutFiles(true)).Returns(episodes);

            //Act
            var result = Mocker.Resolve<RecentBacklogSearchJob>().GetMissingForEnabledSeries();

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
                .Build();

            WithEnableBacklogSearching();

            Mocker.GetMock<EpisodeProvider>()
                .Setup(s => s.EpisodesWithoutFiles(true)).Returns(episodes);

            //Act
            var result = Mocker.Resolve<RecentBacklogSearchJob>().GetMissingForEnabledSeries();

            //Assert
            result.Should().NotBeEmpty();
            result.Should().Contain(s => s.Series.BacklogSetting == BacklogSettingType.Enable);
            result.Should().NotContain(s => s.Series.BacklogSetting == BacklogSettingType.Disable);
            result.Should().Contain(s => s.Series.BacklogSetting == BacklogSettingType.Inherit);
        }

    }
}

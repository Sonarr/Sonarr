using System.Collections.Generic;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.SeriesStats;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Tv.Commands;
using NzbDrone.Test.Common;
using Sonarr.Api.V5.Series;

namespace NzbDrone.Api.Test.Series
{
    [TestFixture]
    public class SeriesV5ControllerUpdateSeriesFixture : TestBase<SeriesController>
    {
        private NzbDrone.Core.Tv.Series _series;

        [SetUp]
        public void Setup()
        {
            _series = new NzbDrone.Core.Tv.Series
            {
                Id = 1,
                Path = "/source/path",
                NextPath = null,
                QualityProfileId = 1,
                Seasons = new List<Season>(),
                Images = new List<NzbDrone.Core.MediaCover.MediaCover>(),
                Tags = new HashSet<int>(),
                Genres = new List<string>(),
                Actors = new List<NzbDrone.Core.Tv.Actor>()
            };

            Mocker.GetMock<ISeriesService>()
                .Setup(s => s.GetSeries(1))
                .Returns(() => _series);

            Mocker.GetMock<ISeriesService>()
                .Setup(s => s.UpdateSeries(It.IsAny<NzbDrone.Core.Tv.Series>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .Returns((NzbDrone.Core.Tv.Series s, bool a, bool b) => s);

            Mocker.GetMock<ISeriesStatisticsService>()
                .Setup(s => s.SeriesStatistics(It.IsAny<int>(), It.IsAny<int>()))
                .Returns(new SeriesStatistics());

            Subject.Url = Mock.Of<IUrlHelper>();
        }

        private SeriesResource RequestFor(string path)
        {
            return new SeriesResource { Id = 1, Path = path, QualityProfileId = 1 };
        }

        [Test]
        public void should_defer_path_and_queue_move_when_no_move_pending()
        {
            Subject.UpdateSeries(RequestFor("/destination/path"), moveFiles: true);

            Mocker.GetMock<ISeriesService>().Verify(
                s => s.UpdateSeries(
                    It.Is<NzbDrone.Core.Tv.Series>(m => m.Path == "/source/path" && m.NextPath == "/destination/path"),
                    It.IsAny<bool>(),
                    It.IsAny<bool>()),
                Times.Once());

            Mocker.GetMock<IManageCommandQueue>().Verify(
                c => c.Push(
                    It.Is<MoveSeriesCommand>(cmd => cmd.SourcePath == "/source/path" && cmd.DestinationPath == "/destination/path"),
                    It.IsAny<CommandPriority>(),
                    CommandTrigger.Manual),
                Times.Once());
        }

        [Test]
        public void should_not_queue_second_move_when_one_already_pending()
        {
            _series.NextPath = "/pending/path";

            Subject.UpdateSeries(RequestFor("/new/destination"), moveFiles: true);

            Mocker.GetMock<IManageCommandQueue>().Verify(
                c => c.Push(It.IsAny<MoveSeriesCommand>(), It.IsAny<CommandPriority>(), It.IsAny<CommandTrigger>()),
                Times.Never());

            Mocker.GetMock<ISeriesService>().Verify(
                s => s.UpdateSeries(
                    It.Is<NzbDrone.Core.Tv.Series>(m => m.Path == "/source/path" && m.NextPath == "/pending/path"),
                    It.IsAny<bool>(),
                    It.IsAny<bool>()),
                Times.Once());
        }

        [Test]
        public void should_persist_before_queueing_move_command()
        {
            var callOrder = new List<string>();

            Mocker.GetMock<ISeriesService>()
                .Setup(s => s.UpdateSeries(It.IsAny<NzbDrone.Core.Tv.Series>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .Callback(() => callOrder.Add("persist"))
                .Returns((NzbDrone.Core.Tv.Series s, bool a, bool b) => s);

            Mocker.GetMock<IManageCommandQueue>()
                .Setup(c => c.Push(It.IsAny<MoveSeriesCommand>(), It.IsAny<CommandPriority>(), It.IsAny<CommandTrigger>()))
                .Callback(() => callOrder.Add("push"));

            Subject.UpdateSeries(RequestFor("/destination/path"), moveFiles: true);

            callOrder.Should().Equal("persist", "push");
        }
    }
}

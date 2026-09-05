using System.Collections.Generic;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.AutoTagging;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Tv.Commands;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.TvTests.SeriesServiceTests
{
    [TestFixture]
    public class UpdateSingleSeriesFixture : CoreTest<SeriesService>
    {
        private Series _series;

        [SetUp]
        public void Setup()
        {
            _series = Builder<Series>.CreateNew()
                .With(s => s.Path = @"C:\Test\source".AsOsAgnostic())
                .With(s => s.PendingPath = null)
                .Build();

            Mocker.GetMock<IAutoTaggingService>()
                .Setup(s => s.GetTagChanges(It.IsAny<Series>()))
                .Returns(new AutoTaggingChanges());

            Mocker.GetMock<ISeriesRepository>()
                .Setup(s => s.Get(It.IsAny<int>()))
                .Returns(_series);

            Mocker.GetMock<ISeriesRepository>()
                .Setup(s => s.Update(It.IsAny<Series>()))
                .Returns<Series>(r => r);

            Mocker.GetMock<IManageCommandQueue>()
                .Setup(s => s.All())
                .Returns(new List<CommandModel>());
        }

        [Test]
        public void should_defer_path_and_queue_move_when_no_move_pending()
        {
            var destinationPath = @"C:\Test\destination".AsOsAgnostic();

            var result = Subject.UpdateSeries(_series, _series.Path, destinationPath, true);

            Mocker.GetMock<IManageCommandQueue>()
                .Verify(v => v.Push(It.Is<MoveSeriesCommand>(c => c.SeriesId == _series.Id && c.SourcePath == _series.Path && c.DestinationPath == destinationPath), It.IsAny<CommandPriority>(), CommandTrigger.Manual), Times.Once());

            result.Path.Should().Be(@"C:\Test\source".AsOsAgnostic());
            result.PendingPath.Should().Be(destinationPath);
        }

        [Test]
        public void should_not_queue_second_move_when_one_already_pending()
        {
            var existingPendingPath = @"C:\Test\pending".AsOsAgnostic();
            _series.PendingPath = existingPendingPath;

            Mocker.GetMock<IManageCommandQueue>()
                .Setup(s => s.All())
                .Returns(new List<CommandModel>
                {
                    new CommandModel
                    {
                        Status = CommandStatus.Started,
                        Body = new MoveSeriesCommand { SeriesId = _series.Id }
                    }
                });

            var result = Subject.UpdateSeries(_series, _series.Path, @"C:\Test\new-destination".AsOsAgnostic(), true);

            Mocker.GetMock<IManageCommandQueue>()
                .Verify(v => v.Push(It.IsAny<MoveSeriesCommand>(), It.IsAny<CommandPriority>(), It.IsAny<CommandTrigger>()), Times.Never());

            result.Path.Should().Be(@"C:\Test\source".AsOsAgnostic());
            result.PendingPath.Should().Be(existingPendingPath);

            ExceptionVerification.ExpectedWarns(1);
        }

        [Test]
        public void should_queue_move_when_pending_path_is_stale()
        {
            var staleExistingPendingPath = @"C:\Test\pending".AsOsAgnostic();
            _series.PendingPath = staleExistingPendingPath;

            Mocker.GetMock<IManageCommandQueue>()
                .Setup(s => s.All())
                .Returns(new List<CommandModel>());

            var newDestinationPath = @"C:\Test\new-destination".AsOsAgnostic();
            var result = Subject.UpdateSeries(_series, _series.Path, newDestinationPath, true);

            result.PendingPath.Should().Be(newDestinationPath);

            Mocker.GetMock<IManageCommandQueue>()
                .Verify(v => v.Push(It.Is<MoveSeriesCommand>(c => c.SeriesId == _series.Id), It.IsAny<CommandPriority>(), CommandTrigger.Manual), Times.Once());
        }

        [Test]
        public void should_update_path_immediately_when_not_moving_files()
        {
            var destinationPath = @"C:\Test\destination".AsOsAgnostic();

            var result = Subject.UpdateSeries(_series, _series.Path, destinationPath, false);

            Mocker.GetMock<IManageCommandQueue>()
                .Verify(v => v.Push(It.IsAny<MoveSeriesCommand>(), It.IsAny<CommandPriority>(), It.IsAny<CommandTrigger>()), Times.Never());

            result.Path.Should().Be(destinationPath);
        }

        [Test]
        public void should_clear_stale_pending_path_when_updating_immediately()
        {
            var existingPendingPath = @"C:\Test\pending".AsOsAgnostic();
            _series.PendingPath = existingPendingPath;

            var result = Subject.UpdateSeries(_series, _series.Path, @"C:\Test\destination".AsOsAgnostic(), false);

            result.PendingPath.Should().BeNull();
        }

        [Test]
        public void should_not_overwrite_pending_path_when_a_move_is_already_pending_and_not_moving_files()
        {
            var existingPendingPath = @"C:\Test\pending".AsOsAgnostic();
            var originalPath = _series.Path;
            _series.PendingPath = existingPendingPath;

            Mocker.GetMock<IManageCommandQueue>()
                .Setup(s => s.All())
                .Returns(new List<CommandModel>
                {
                    new CommandModel
                    {
                        Status = CommandStatus.Started,
                        Body = new MoveSeriesCommand { SeriesId = _series.Id }
                    }
                });

            var result = Subject.UpdateSeries(_series, _series.Path, @"C:\Test\destination".AsOsAgnostic(), false);

            result.Path.Should().Be(originalPath);
            result.PendingPath.Should().Be(existingPendingPath);

            ExceptionVerification.ExpectedWarns(1);
        }
    }
}

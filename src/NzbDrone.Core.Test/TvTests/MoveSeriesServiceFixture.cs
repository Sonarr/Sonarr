using System.Collections.Generic;
using System.IO;
using System.Linq;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Disk;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Tv.Commands;
using NzbDrone.Core.Tv.Events;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.TvTests
{
    [TestFixture]
    public class MoveSeriesServiceFixture : CoreTest<MoveSeriesService>
    {
        private Series _series;
        private MoveSeriesCommand _command;
        private BulkMoveSeriesCommand _bulkCommand;

        [SetUp]
        public void Setup()
        {
            _series = Builder<Series>
                .CreateNew()
                .Build();

            _command = new MoveSeriesCommand
                       {
                           SeriesId = 1,
                           SourcePath = @"C:\Test\TV\Series".AsOsAgnostic(),
                           DestinationPath = @"C:\Test\TV2\Series".AsOsAgnostic()
                       };

            _bulkCommand = new BulkMoveSeriesCommand
                       {
                           Series = new List<BulkMoveSeries>
                                    {
                                        new BulkMoveSeries
                                        {
                                            SeriesId = 1,
                                            SourcePath = @"C:\Test\TV\Series".AsOsAgnostic()
                                        }
                                    },
                           DestinationRootFolder = @"C:\Test\TV2".AsOsAgnostic()
                       };

            Mocker.GetMock<ISeriesService>()
                  .Setup(s => s.GetSeries(It.IsAny<int>()))
                  .Returns(_series);

            Mocker.GetMock<IDiskProvider>()
                  .Setup(s => s.FolderExists(It.IsAny<string>()))
                  .Returns(true);
        }

        private void GivenFailedMove()
        {
            Mocker.GetMock<IDiskTransferService>()
                  .Setup(s => s.TransferFolder(It.IsAny<string>(), It.IsAny<string>(), TransferMode.Move))
                  .Throws<IOException>();
        }

        [Test]
        public void should_publish_the_persisted_series_after_a_folder_rename()
        {
            _command.DestinationPath = @"C:\Test\TV\Renamed Series".AsOsAgnostic();
            _series.Path = _command.SourcePath;
            var storedSeries = Builder<Series>.CreateNew()
                .With(s => s.Path = _command.SourcePath)
                .With(s => s.PendingPath = _command.DestinationPath)
                .Build();
            var savedSeries = Builder<Series>.CreateNew()
                .With(s => s.Path = _command.DestinationPath)
                .With(s => s.PendingPath = null)
                .Build();

            Mocker.GetMock<ISeriesService>()
                .SetupSequence(s => s.GetSeries(_command.SeriesId))
                .Returns(_series)
                .Returns(storedSeries);
            Mocker.GetMock<ISeriesService>()
                .Setup(s => s.UpdateSeries(storedSeries, true, true))
                .Returns(savedSeries);

            Subject.Execute(_command);

            Mocker.GetMock<IEventAggregator>()
                .Verify(v => v.PublishEvent(It.Is<SeriesMovedEvent>(e =>
                    e.Series == savedSeries && e.Series.Path == _command.DestinationPath &&
                    e.Series.PendingPath == null && e.SourcePath == _command.SourcePath &&
                    e.DestinationPath == _command.DestinationPath)),
                    Times.Once());
        }

        [TestCase("failed")]
        [TestCase("missing")]
        [TestCase("unchanged")]
        public void should_not_publish_a_move_when_no_folder_was_transferred(string outcome)
        {
            if (outcome == "failed")
            {
                GivenFailedMove();
            }
            else if (outcome == "missing")
            {
                Mocker.GetMock<IDiskProvider>().Setup(s => s.FolderExists(_command.SourcePath)).Returns(false);
            }
            else
            {
                _command.DestinationPath = _command.SourcePath;
            }

            Subject.Execute(_command);

            if (outcome == "failed")
            {
                ExceptionVerification.ExpectedErrors(1);
            }

            Mocker.GetMock<IEventAggregator>()
                .Verify(v => v.PublishEvent(It.IsAny<SeriesMovedEvent>()), Times.Never());
        }

        [Test]
        public void should_log_error_when_move_throws_an_exception()
        {
            GivenFailedMove();

            Subject.Execute(_command);

            ExceptionVerification.ExpectedErrors(1);
        }

        [Test]
        public void should_revert_series_path_on_error()
        {
            GivenFailedMove();

            Subject.Execute(_command);

            ExceptionVerification.ExpectedErrors(1);

            Mocker.GetMock<ISeriesService>()
                  .Verify(v => v.UpdateSeries(It.IsAny<Series>(), It.IsAny<bool>(), It.IsAny<bool>()), Times.Once());
        }

        [Test]
        public void should_update_series_path_after_successful_move()
        {
            Subject.Execute(_command);

            Mocker.GetMock<IDiskTransferService>()
                  .Verify(v => v.TransferFolder(_command.SourcePath, _command.DestinationPath, TransferMode.Move), Times.Once());

            Mocker.GetMock<ISeriesService>()
                  .Verify(v => v.UpdateSeries(It.Is<Series>(s => s.Path == _command.DestinationPath), It.IsAny<bool>(), It.IsAny<bool>()), Times.Once());
        }

        [Test]
        public void should_use_destination_path()
        {
            Subject.Execute(_command);

            Mocker.GetMock<IDiskTransferService>()
                  .Verify(v => v.TransferFolder(_command.SourcePath, _command.DestinationPath, TransferMode.Move), Times.Once());

            Mocker.GetMock<IBuildFileNames>()
                  .Verify(v => v.GetSeriesFolder(It.IsAny<Series>(), null), Times.Never());
        }

        [Test]
        public void should_build_new_path_when_root_folder_is_provided()
        {
            var seriesFolder = "Series";
            var expectedPath = Path.Combine(_bulkCommand.DestinationRootFolder, seriesFolder);

            Mocker.GetMock<IBuildFileNames>()
                    .Setup(s => s.GetSeriesFolder(It.IsAny<Series>(), null))
                    .Returns(seriesFolder);

            Subject.Execute(_bulkCommand);

            Mocker.GetMock<IDiskTransferService>()
                  .Verify(v => v.TransferFolder(_bulkCommand.Series.First().SourcePath, expectedPath, TransferMode.Move), Times.Once());
        }

        [Test]
        public void should_skip_series_folder_if_it_does_not_exist_and_update_path_when_destination_exists()
        {
            Mocker.GetMock<IDiskProvider>()
                  .Setup(s => s.FolderExists(_command.SourcePath))
                  .Returns(false);

            Mocker.GetMock<IDiskProvider>()
                  .Setup(s => s.FolderExists(_command.DestinationPath))
                  .Returns(true);

            Subject.Execute(_command);

            Mocker.GetMock<IDiskTransferService>()
                  .Verify(v => v.TransferFolder(_command.SourcePath, _command.DestinationPath, TransferMode.Move), Times.Never());

            Mocker.GetMock<IBuildFileNames>()
                  .Verify(v => v.GetSeriesFolder(It.IsAny<Series>(), null), Times.Never());

            Mocker.GetMock<ISeriesService>()
                  .Verify(v => v.UpdateSeries(It.Is<Series>(s => s.Path == _command.DestinationPath), It.IsAny<bool>(), It.IsAny<bool>()), Times.Once());
        }

        [Test]
        public void should_update_path_to_destination_if_neither_source_nor_destination_folder_exists()
        {
            Mocker.GetMock<IDiskProvider>()
                  .Setup(s => s.FolderExists(It.IsAny<string>()))
                  .Returns(false);

            Subject.Execute(_command);

            ExceptionVerification.ExpectedWarns(1);

            Mocker.GetMock<IDiskTransferService>()
                  .Verify(v => v.TransferFolder(_command.SourcePath, _command.DestinationPath, TransferMode.Move), Times.Never());

            Mocker.GetMock<ISeriesService>()
                  .Verify(v => v.UpdateSeries(It.Is<Series>(s => s.Path == _command.DestinationPath && s.PendingPath == null), It.IsAny<bool>(), It.IsAny<bool>()), Times.Once());
        }

        [Test]
        public void should_update_path_to_destination_when_source_path_is_invalid()
        {
            _command.SourcePath = string.Empty;

            Subject.Execute(_command);

            ExceptionVerification.ExpectedWarns(1);

            Mocker.GetMock<ISeriesService>()
                  .Verify(v => v.UpdateSeries(It.Is<Series>(s => s.Path == _command.DestinationPath), It.IsAny<bool>(), It.IsAny<bool>()), Times.Once());
        }

        [Test]
        public void should_clear_pending_path_after_successful_move()
        {
            _series.PendingPath = _command.DestinationPath;

            Subject.Execute(_command);

            Mocker.GetMock<ISeriesService>()
                  .Verify(v => v.UpdateSeries(It.Is<Series>(s => s.PendingPath == null), It.IsAny<bool>(), It.IsAny<bool>()), Times.Once());
        }

        [Test]
        public void should_clear_pending_path_after_failed_move()
        {
            GivenFailedMove();
            _series.PendingPath = _command.DestinationPath;

            Subject.Execute(_command);

            ExceptionVerification.ExpectedErrors(1);

            Mocker.GetMock<ISeriesService>()
                  .Verify(v => v.UpdateSeries(It.Is<Series>(s => s.PendingPath == null), It.IsAny<bool>(), It.IsAny<bool>()), Times.Once());
        }

        [Test]
        public void should_clear_pending_path_when_already_at_destination()
        {
            _series.PendingPath = _command.DestinationPath;

            Mocker.GetMock<IDiskProvider>()
                  .Setup(s => s.FolderExists(It.IsAny<string>()))
                  .Returns(true);

            Subject.Execute(new MoveSeriesCommand
            {
                SeriesId = 1,
                SourcePath = _command.SourcePath,
                DestinationPath = _command.SourcePath
            });

            Mocker.GetMock<ISeriesService>()
                  .Verify(v => v.UpdateSeries(It.Is<Series>(s => s.PendingPath == null), It.IsAny<bool>(), It.IsAny<bool>()), Times.Once());
        }
    }
}

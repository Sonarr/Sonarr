using System.Collections.Generic;
using System.IO;
using System.Linq;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Disk;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Tv.Commands;
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
                  .Setup(s => s.TransferFolder(It.IsAny<string>(), It.IsAny<string>(), TransferMode.Move, true))
                  .Throws<IOException>();
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
                  .Verify(v => v.UpdateSeries(It.IsAny<Series>(), It.IsAny<bool>()), Times.Once());
        }

        [Test]
        public void should_use_destination_path()
        {
            Subject.Execute(_command);

            Mocker.GetMock<IDiskTransferService>()
                  .Verify(v => v.TransferFolder(_command.SourcePath, _command.DestinationPath, TransferMode.Move, It.IsAny<bool>()), Times.Once());

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
                  .Verify(v => v.TransferFolder(_bulkCommand.Series.First().SourcePath, expectedPath, TransferMode.Move, It.IsAny<bool>()), Times.Once());
        }

        [Test]
        public void should_skip_series_folder_if_it_does_not_exist()
        {
            Mocker.GetMock<IDiskProvider>()
                  .Setup(s => s.FolderExists(It.IsAny<string>()))
                  .Returns(false);


            Subject.Execute(_command);
            
            Mocker.GetMock<IDiskTransferService>()
                  .Verify(v => v.TransferFolder(_command.SourcePath, _command.DestinationPath, TransferMode.Move, It.IsAny<bool>()), Times.Never());

            Mocker.GetMock<IBuildFileNames>()
                  .Verify(v => v.GetSeriesFolder(It.IsAny<Series>(), null), Times.Never());
        }
    }
}

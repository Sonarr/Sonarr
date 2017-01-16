using System.IO;
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

            Mocker.GetMock<ISeriesService>()
                  .Setup(s => s.GetSeries(It.IsAny<int>()))
                  .Returns(_series);
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

            Assert.Throws<IOException>(() => Subject.Execute(_command));

            ExceptionVerification.ExpectedErrors(1);
        }

        [Test]
        public void should_no_update_series_path_on_error()
        {
            GivenFailedMove();

            Assert.Throws<IOException>(() => Subject.Execute(_command));

            ExceptionVerification.ExpectedErrors(1);

            Mocker.GetMock<ISeriesService>()
                  .Verify(v => v.UpdateSeries(It.IsAny<Series>()), Times.Never());
        }

        [Test]
        public void should_build_new_path_when_root_folder_is_provided()
        {
            _command.DestinationPath = null;
            _command.DestinationRootFolder = @"C:\Test\TV3".AsOsAgnostic();
            
            var expectedPath = @"C:\Test\TV3\Series".AsOsAgnostic();

            Mocker.GetMock<IBuildFileNames>()
                  .Setup(s => s.GetSeriesFolder(It.IsAny<Series>(), null))
                  .Returns("Series");

            Subject.Execute(_command);

            Mocker.GetMock<ISeriesService>()
                  .Verify(v => v.UpdateSeries(It.Is<Series>(s => s.Path == expectedPath)), Times.Once());
        }

        [Test]
        public void should_use_destination_path_if_destination_root_folder_is_blank()
        {
            Subject.Execute(_command);

            Mocker.GetMock<ISeriesService>()
                  .Verify(v => v.UpdateSeries(It.Is<Series>(s => s.Path == _command.DestinationPath)), Times.Once());

            Mocker.GetMock<IBuildFileNames>()
                  .Verify(v => v.GetSeriesFolder(It.IsAny<Series>(), null), Times.Never());
        }
    }
}

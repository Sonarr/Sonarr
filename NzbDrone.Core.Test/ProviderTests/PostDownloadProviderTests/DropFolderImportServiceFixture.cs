using System;
using System.IO;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using NzbDrone.Common;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.ProviderTests.PostDownloadProviderTests
{
    [TestFixture]
    public class DropFolderImportServiceFixture : CoreTest<DropFolderImportService>
    {
        private EpisodeFile _fakeEpisodeFile;


        private string[] _subFolders = new[] { "c:\\root\\foldername" };
        private string[] _videoFiles = new[] { "c:\\root\\foldername\\video.ext" };

        [SetUp]
        public void Setup()
        {
            _fakeEpisodeFile = Builder<EpisodeFile>.CreateNew().Build();


            Mocker.GetMock<DiskScanProvider>().Setup(c => c.GetVideoFiles(It.IsAny<string>(), It.IsAny<bool>()))
                  .Returns(_videoFiles);

            Mocker.GetMock<DiskProvider>().Setup(c => c.GetDirectories(It.IsAny<string>()))
                  .Returns(_subFolders);
        }

        private void WithOldWrite()
        {
            Mocker.GetMock<DiskProvider>()
                .Setup(c => c.GetLastFolderWrite(It.IsAny<String>()))
                .Returns(DateTime.Now.AddDays(-5));
        }

        private void WithRecentFolderWrite()
        {
            Mocker.GetMock<DiskProvider>()
                .Setup(c => c.GetLastFolderWrite(It.IsAny<String>()))
                .Returns(DateTime.UtcNow);
        }

        [Test]
        public void should_import_file()
        {
            Subject.ProcessDropFolder("c:\\drop\\");

            VerifyImport();
        }

        [Test]
        public void should_skip_if_folder_is_too_fresh()
        {
            WithRecentFolderWrite();

            Subject.ProcessDropFolder("c:\\drop\\");

            VerifyNoImport();
        }


        [Test]
        public void should_search_for_series_using_folder_name()
        {
            WithOldWrite();

            Subject.ProcessDropFolder("c:\\drop\\");

            Mocker.GetMock<ISeriesService>().Verify(c => c.FindByTitle("foldername"), Times.Once());

        }

        [Test]
        public void should_search_for_series_using_file_name()
        {
            /*WithOldWrite();
            WithValidSeries();
            WithImportableFiles();

            var droppedFolder = new DirectoryInfo(@"C:\Test\Unsorted TV\The Office - S01E01 - Episode Title");
            Subject.ProcessDownload(droppedFolder);

            Mocker.GetMock<DiskScanProvider>()
                .Verify(c => c.Scan(_fakeSeries, It.IsAny<string>()));*/

        }


        [Test]
        public void all_imported_files_should_be_moved()
        {
            Mocker.GetMock<DiskScanProvider>().Setup(c => c.ImportFile(It.IsAny<Series>(), It.IsAny<string>()))
                  .Returns(_fakeEpisodeFile);

            Subject.ProcessDropFolder("c:\\drop\\");

            Mocker.GetMock<IMoveEpisodeFiles>().Verify(c => c.MoveEpisodeFile(_fakeEpisodeFile, true), Times.Once());
        }

        [Test]
        public void should_not_attempt_move_if_nothing_is_imported()
        {
            Mocker.GetMock<DiskScanProvider>().Setup(c => c.ImportFile(It.IsAny<Series>(), It.IsAny<string>()))
                 .Returns<EpisodeFile>(null);

            Subject.ProcessDropFolder("c:\\drop\\");

            Mocker.GetMock<IMoveEpisodeFiles>().Verify(c => c.MoveEpisodeFile(It.IsAny<EpisodeFile>(), It.IsAny<bool>()), Times.Never());
        }


        [Test]
        public void should_skip_if_folder_is_in_use_by_another_process()
        {

            Mocker.GetMock<DiskProvider>().Setup(c => c.IsFileLocked(It.IsAny<FileInfo>()))
                  .Returns(true);

            Subject.ProcessDropFolder("c:\\drop\\");
            VerifyNoImport();
        }



        private void VerifyNoImport()
        {
            Mocker.GetMock<DiskScanProvider>().Verify(c => c.ImportFile(It.IsAny<Series>(), It.IsAny<string>()),
                Times.Never());
        }


        private void VerifyImport()
        {
            Mocker.GetMock<DiskScanProvider>().Verify(c => c.ImportFile(It.IsAny<Series>(), It.IsAny<string>()),
                Times.Once());
        }
    }
}
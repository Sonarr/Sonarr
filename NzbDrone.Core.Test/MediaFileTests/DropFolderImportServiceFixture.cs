using System;
using System.Collections.Generic;
using System.IO;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using NzbDrone.Common;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.EpisodeImport;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.MediaFileTests
{
    [TestFixture]
    public class DropFolderImportServiceFixture : CoreTest<DownloadedEpisodesImportService>
    {
        private EpisodeFile _fakeEpisodeFile;

        private string[] _subFolders = new[] { "c:\\root\\foldername" };
        private string[] _videoFiles = new[] { "c:\\root\\foldername\\video.ext" };

        [SetUp]
        public void Setup()
        {
            _fakeEpisodeFile = Builder<EpisodeFile>.CreateNew().Build();

            Mocker.GetMock<IDiskScanService>().Setup(c => c.GetVideoFiles(It.IsAny<string>(), It.IsAny<bool>()))
                  .Returns(_videoFiles);

            Mocker.GetMock<IDiskProvider>().Setup(c => c.GetDirectories(It.IsAny<string>()))
                  .Returns(_subFolders);

            Mocker.GetMock<IConfigService>().SetupGet(c => c.DownloadedEpisodesFolder)
                  .Returns("c:\\drop\\");
        }

        [Test]
        public void should_import_file()
        {
            Subject.ProcessDownloadedEpisodesFolder();

            VerifyImport();
        }

        [Test]
        public void should_search_for_series_using_folder_name()
        {
            Subject.ProcessDownloadedEpisodesFolder();

            Mocker.GetMock<IParsingService>().Verify(c => c.GetSeries("foldername"), Times.Once());
        }

        [Test]
        public void should_skip_if_file_is_in_use_by_another_process()
        {
            Mocker.GetMock<IDiskProvider>().Setup(c => c.IsFileLocked(It.IsAny<FileInfo>()))
                  .Returns(true);

            Subject.ProcessDownloadedEpisodesFolder();
            VerifyNoImport();
        }

        private void VerifyNoImport()
        {
            Mocker.GetMock<IImportApprovedEpisodes>().Verify(c => c.Import(It.IsAny<List<ImportDecision>>(), true),
                Times.Never());
        }

        private void VerifyImport()
        {
            Mocker.GetMock<IImportApprovedEpisodes>().Verify(c => c.Import(It.IsAny<List<ImportDecision>>(), true),
                Times.Once());
        }
    }
}
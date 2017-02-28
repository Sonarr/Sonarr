using System.Collections.Generic;
using System.IO;
using System.Linq;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Disk;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.EpisodeImport;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.MediaFiles.DiskScanServiceTests
{
    [TestFixture]
    public class ScanFixture : CoreTest<DiskScanService>
    {
        private Series _series;

        [SetUp]
        public void Setup()
        {
            _series = Builder<Series>.CreateNew()
                                     .With(s => s.Path = @"C:\Test\TV\Series".AsOsAgnostic())
                                     .Build();

            Mocker.GetMock<IDiskProvider>()
                  .Setup(s => s.GetParentFolder(It.IsAny<string>()))
                  .Returns((string path) => Directory.GetParent(path).FullName);
        }

        private void GivenParentFolderExists()
        {
            Mocker.GetMock<IDiskProvider>()
                  .Setup(s => s.FolderExists(It.IsAny<string>()))
                  .Returns(true);

            Mocker.GetMock<IDiskProvider>()
                  .Setup(s => s.GetDirectories(It.IsAny<string>()))
                  .Returns(new[] { @"C:\Test\TV\Series2".AsOsAgnostic() });
        }

        private void GivenFiles(IEnumerable<string> files)
        {
            Mocker.GetMock<IDiskProvider>()
                  .Setup(s => s.GetFiles(It.IsAny<string>(), SearchOption.AllDirectories))
                  .Returns(files.ToArray());
        }

        [Test]
        public void should_not_scan_if_series_root_folder_does_not_exist()
        {
            Subject.Scan(_series);

            ExceptionVerification.ExpectedWarns(1);

            Mocker.GetMock<IMediaFileTableCleanupService>()
                  .Verify(v => v.Clean(It.IsAny<Series>(), It.IsAny<List<string>>()), Times.Never());
        }

        [Test]
        public void should_not_scan_if_series_root_folder_is_empty()
        {
            Mocker.GetMock<IDiskProvider>()
                  .Setup(s => s.FolderExists(It.IsAny<string>()))
                  .Returns(true);

            Mocker.GetMock<IDiskProvider>()
                  .Setup(s => s.GetDirectories(It.IsAny<string>()))
                  .Returns(new string[0]);

            Subject.Scan(_series);

            ExceptionVerification.ExpectedWarns(1);

            Mocker.GetMock<IMediaFileTableCleanupService>()
                  .Verify(v => v.Clean(It.IsAny<Series>(), new List<string>()), Times.Never());
        }

        [Test]
        public void should_clean_but_not_import_if_series_folder_does_not_exist()
        {
            GivenParentFolderExists();
            
            Mocker.GetMock<IDiskProvider>()
                  .Setup(s => s.FolderExists(@"C:\Test\TV\Series"))
                  .Returns(false);

            Subject.Scan(_series);

            Mocker.GetMock<IMediaFileTableCleanupService>()
                  .Verify(v => v.Clean(It.IsAny<Series>(), It.IsAny<List<string>>()), Times.Once());

            Mocker.GetMock<IMakeImportDecision>()
                  .Verify(v => v.GetImportDecisions(It.IsAny<List<string>>(), _series), Times.Never());
        }

        [Test]
        public void should_create_and_clean_but_not_import_if_series_folder_does_not_exist_but_create_folder_enabled()
        {
            GivenParentFolderExists();
            
            Mocker.GetMock<IConfigService>()
                  .Setup(s => s.CreateEmptySeriesFolders)
                  .Returns(true);

            Mocker.GetMock<IDiskProvider>()
                  .Setup(s => s.FolderExists(@"C:\Test\TV\Series"))
                  .Returns(false);

            Subject.Scan(_series);

            Mocker.GetMock<IMediaFileTableCleanupService>()
                  .Verify(v => v.Clean(It.IsAny<Series>(), It.IsAny<List<string>>()), Times.Once());

            Mocker.GetMock<IMakeImportDecision>()
                  .Verify(v => v.GetImportDecisions(It.IsAny<List<string>>(), _series), Times.Never());
        }

        [Test]
        public void should_find_files_at_root_of_series_folder()
        {
            GivenParentFolderExists();

            GivenFiles(new List<string>
                       {
                           Path.Combine(_series.Path, "file1.mkv").AsOsAgnostic(),
                           Path.Combine(_series.Path, "s01e01.mkv").AsOsAgnostic()
                       });

            Subject.Scan(_series);

            Mocker.GetMock<IMakeImportDecision>()
                  .Verify(v => v.GetImportDecisions(It.Is<List<string>>(l => l.Count == 2), _series), Times.Once());
        }

        [Test]
        public void should_not_scan_extras_subfolder()
        {
            GivenParentFolderExists();

            GivenFiles(new List<string>
                       {
                           Path.Combine(_series.Path, "EXTRAS", "file1.mkv").AsOsAgnostic(),
                           Path.Combine(_series.Path, "Extras", "file2.mkv").AsOsAgnostic(),
                           Path.Combine(_series.Path, "EXTRAs", "file3.mkv").AsOsAgnostic(),
                           Path.Combine(_series.Path, "ExTrAs", "file4.mkv").AsOsAgnostic(),
                           Path.Combine(_series.Path, "Season 1", "s01e01.mkv").AsOsAgnostic()
                       });

            Subject.Scan(_series);

            Mocker.GetMock<IDiskProvider>()
                  .Verify(v => v.GetFiles(It.IsAny<string>(), It.IsAny<SearchOption>()), Times.Once());

            Mocker.GetMock<IMakeImportDecision>()
                  .Verify(v => v.GetImportDecisions(It.Is<List<string>>(l => l.Count == 1), _series), Times.Once());
        }

        [Test]
        public void should_not_scan_AppleDouble_subfolder()
        {
            GivenParentFolderExists();

            GivenFiles(new List<string>
                       {
                           Path.Combine(_series.Path, ".AppleDouble", "file1.mkv").AsOsAgnostic(),
                           Path.Combine(_series.Path, ".appledouble", "file2.mkv").AsOsAgnostic(),
                           Path.Combine(_series.Path, "Season 1", "s01e01.mkv").AsOsAgnostic()
                       });

            Subject.Scan(_series);

            Mocker.GetMock<IMakeImportDecision>()
                  .Verify(v => v.GetImportDecisions(It.Is<List<string>>(l => l.Count == 1), _series), Times.Once());
        }

        [Test]
        public void should_scan_extras_series_and_subfolders()
        {
            GivenParentFolderExists();
            _series.Path = @"C:\Test\TV\Extras".AsOsAgnostic();

            GivenFiles(new List<string>
                       {
                           Path.Combine(_series.Path, "Extras", "file1.mkv").AsOsAgnostic(),
                           Path.Combine(_series.Path, ".AppleDouble", "file2.mkv").AsOsAgnostic(),
                           Path.Combine(_series.Path, "Season 1", "s01e01.mkv").AsOsAgnostic(),
                           Path.Combine(_series.Path, "Season 1", "s01e02.mkv").AsOsAgnostic(),
                           Path.Combine(_series.Path, "Season 2", "s02e01.mkv").AsOsAgnostic(),
                           Path.Combine(_series.Path, "Season 2", "s02e02.mkv").AsOsAgnostic(),
                       });

            Subject.Scan(_series);

            Mocker.GetMock<IMakeImportDecision>()
                  .Verify(v => v.GetImportDecisions(It.Is<List<string>>(l => l.Count == 4), _series), Times.Once());
        }

        [Test]
        public void should_not_scan_subfolders_that_start_with_period()
        {
            GivenParentFolderExists();

            GivenFiles(new List<string>
                       {
                           Path.Combine(_series.Path, ".@__thumb", "file1.mkv").AsOsAgnostic(),
                           Path.Combine(_series.Path, ".@__THUMB", "file2.mkv").AsOsAgnostic(),
                           Path.Combine(_series.Path, ".hidden", "file2.mkv").AsOsAgnostic(),
                           Path.Combine(_series.Path, "Season 1", "s01e01.mkv").AsOsAgnostic()
                       });

            Subject.Scan(_series);

            Mocker.GetMock<IMakeImportDecision>()
                  .Verify(v => v.GetImportDecisions(It.Is<List<string>>(l => l.Count == 1), _series), Times.Once());
        }

        [Test]
        public void should_not_scan_subfolder_of_season_folder_that_starts_with_a_period()
        {
            GivenParentFolderExists();

            GivenFiles(new List<string>
                       {
                           Path.Combine(_series.Path, "Season 1", ".@__thumb", "file1.mkv").AsOsAgnostic(),
                           Path.Combine(_series.Path, "Season 1", ".@__THUMB", "file2.mkv").AsOsAgnostic(),
                           Path.Combine(_series.Path, "Season 1", ".hidden", "file2.mkv").AsOsAgnostic(),
                           Path.Combine(_series.Path, "Season 1", ".AppleDouble", "s01e01.mkv").AsOsAgnostic(),
                           Path.Combine(_series.Path, "Season 1", "s01e01.mkv").AsOsAgnostic()
                       });

            Subject.Scan(_series);

            Mocker.GetMock<IMakeImportDecision>()
                  .Verify(v => v.GetImportDecisions(It.Is<List<string>>(l => l.Count == 1), _series), Times.Once());
        }

        [Test]
        public void should_not_scan_Synology_eaDir()
        {
            GivenParentFolderExists();

            GivenFiles(new List<string>
                       {
                           Path.Combine(_series.Path, "@eaDir", "file1.mkv").AsOsAgnostic(),
                           Path.Combine(_series.Path, "Season 1", "s01e01.mkv").AsOsAgnostic()
                       });

            Subject.Scan(_series);

            Mocker.GetMock<IMakeImportDecision>()
                  .Verify(v => v.GetImportDecisions(It.Is<List<string>>(l => l.Count == 1), _series), Times.Once());
        }

        [Test]
        public void should_not_scan_thumb_folder()
        {
            GivenParentFolderExists();

            GivenFiles(new List<string>
                       {
                           Path.Combine(_series.Path, ".@__thumb", "file1.mkv").AsOsAgnostic(),
                           Path.Combine(_series.Path, "Season 1", "s01e01.mkv").AsOsAgnostic()
                       });

            Subject.Scan(_series);

            Mocker.GetMock<IMakeImportDecision>()
                  .Verify(v => v.GetImportDecisions(It.Is<List<string>>(l => l.Count == 1), _series), Times.Once());
        }

        [Test]
        public void should_scan_dotHack_folder()
        {
            GivenParentFolderExists();
            _series.Path = @"C:\Test\TV\.hack".AsOsAgnostic();

            GivenFiles(new List<string>
                       {
                           Path.Combine(_series.Path, "Season 1", "file1.mkv").AsOsAgnostic(),
                           Path.Combine(_series.Path, "Season 1", "s01e01.mkv").AsOsAgnostic()
                       });

            Subject.Scan(_series);

            Mocker.GetMock<IMakeImportDecision>()
                  .Verify(v => v.GetImportDecisions(It.Is<List<string>>(l => l.Count == 2), _series), Times.Once());
        }

        [Test]
        public void should_exclude_osx_metadata_files()
        {
            GivenParentFolderExists();

            GivenFiles(new List<string>
                       {
                           Path.Combine(_series.Path, "._24 The Status Quo Combustion.mp4").AsOsAgnostic(),
                           Path.Combine(_series.Path, "24 The Status Quo Combustion.mkv").AsOsAgnostic()
                       });

            Subject.Scan(_series);

            Mocker.GetMock<IMakeImportDecision>()
                  .Verify(v => v.GetImportDecisions(It.Is<List<string>>(l => l.Count == 1), _series), Times.Once());
        }
    }
}

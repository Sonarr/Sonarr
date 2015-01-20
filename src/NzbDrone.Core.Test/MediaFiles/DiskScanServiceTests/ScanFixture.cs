using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Disk;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.Commands;
using NzbDrone.Core.MediaFiles.EpisodeImport;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Qualities;
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
                  .Returns(new string[] { @"C:\Test\TV\Series2".AsOsAgnostic() });
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

            Mocker.GetMock<ICommandExecutor>()
                .Verify(v => v.PublishCommand(It.IsAny<CleanMediaFileDb>()), Times.Never());
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

            Mocker.GetMock<ICommandExecutor>()
                .Verify(v => v.PublishCommand(It.IsAny<CleanMediaFileDb>()), Times.Never());
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

            Mocker.GetMock<IMakeImportDecision>()
                  .Verify(v => v.GetImportDecisions(It.Is<List<string>>(l => l.Count == 1), _series, false, (QualityModel)null), Times.Once());
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
                  .Verify(v => v.GetImportDecisions(It.Is<List<string>>(l => l.Count == 1), _series, false, (QualityModel)null), Times.Once());
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
                  .Verify(v => v.GetImportDecisions(It.Is<List<string>>(l => l.Count == 4), _series, false, (QualityModel)null), Times.Once());
        }
    }
}

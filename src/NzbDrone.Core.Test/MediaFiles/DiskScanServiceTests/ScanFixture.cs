using System;
using System.Collections;
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
                                     .With(s => s.Path = @"C:\Test\TV\Series")
                                     .Build();

            Mocker.GetMock<IDiskProvider>()
                  .Setup(s => s.GetParentFolder(It.IsAny<String>()))
                  .Returns((String path) => Directory.GetParent(path).FullName);

            
        }

        private void GivenParentFolderExists()
        {
            Mocker.GetMock<IDiskProvider>()
                  .Setup(s => s.FolderExists(It.IsAny<String>()))
                  .Returns(true);
        }

        private void GivenFiles(IEnumerable<String> files)
        {
            Mocker.GetMock<IDiskProvider>()
                  .Setup(s => s.GetFiles(It.IsAny<String>(), SearchOption.AllDirectories))
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
        public void should_not_scan_extras_folder()
        {
            GivenParentFolderExists();
            GivenFiles(new List<String>
                       {
                           Path.Combine(_series.Path, "EXTRAS", "file1.mkv").AsOsAgnostic(),
                           Path.Combine(_series.Path, "Extras", "file2.mkv").AsOsAgnostic(),
                           Path.Combine(_series.Path, "EXTRAs", "file3.mkv").AsOsAgnostic(),
                           Path.Combine(_series.Path, "ExTrAs", "file4.mkv").AsOsAgnostic(),
                           Path.Combine(_series.Path, "Season 1", "s01e01.mkv").AsOsAgnostic()
                       });

//            Mocker.GetMock<IMakeImportDecision>()
//                  .Setup(s => s.GetImportDecisions(It.IsAny<List<String>>(), _series, false, (QualityModel) null))
//                  .Returns(new List<ImportDecision>());

            Subject.Scan(_series);

            Mocker.GetMock<IMakeImportDecision>()
                  .Verify(v => v.GetImportDecisions(It.Is<List<String>>(l => l.Count == 1), _series, false, (QualityModel)null), Times.Once());
        }
    }
}

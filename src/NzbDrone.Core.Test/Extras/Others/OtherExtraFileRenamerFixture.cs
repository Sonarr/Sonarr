using System.IO;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Disk;
using NzbDrone.Core.Extras.Others;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.Extras.Others
{
    [TestFixture]
    public class OtherExtraFileRenamerFixture : CoreTest<OtherExtraFileRenamer>
    {
        private Series _series;
        private string _nfoPath;
        private string _origPath;
        private string _nfoRelativePath;
        private string _origRelativePath;
        private OtherExtraFile _nfoFile;
        private OtherExtraFile _origFile;

        [SetUp]
        public void Setup()
        {
            _series = Builder<Series>.CreateNew()
                                     .With(s => s.Id = 1)
                                     .With(s => s.Path = @"C:\Test\TV\Series Title".AsOsAgnostic())
                                     .Build();

            _nfoRelativePath = Path.Combine("Season 1", "Series Title - S01E01.nfo");
            _origRelativePath = _nfoRelativePath + "-orig";
            _nfoPath = Path.Combine(_series.Path, _nfoRelativePath);
            _origPath = _nfoPath + "-orig";

            _nfoFile = new OtherExtraFile { Id = 1, SeriesId = 1, RelativePath = _nfoRelativePath, Extension = ".nfo" };
            _origFile = new OtherExtraFile { Id = 2, SeriesId = 1, RelativePath = _origRelativePath, Extension = ".nfo-orig" };

            Mocker.GetMock<IDiskProvider>()
                  .Setup(s => s.FileExists(_nfoPath))
                  .Returns(true);

            Mocker.GetMock<IOtherExtraFileService>()
                  .Setup(s => s.FindByPath(_series.Id, _nfoRelativePath))
                  .Returns(_nfoFile);
        }

        private void GivenExistingOrigFile()
        {
            Mocker.GetMock<IDiskProvider>()
                  .Setup(s => s.FileExists(_origPath))
                  .Returns(true);

            Mocker.GetMock<IOtherExtraFileService>()
                  .Setup(s => s.FindByPath(_series.Id, _origRelativePath))
                  .Returns(_origFile);
        }

        [Test]
        public void should_rename_extra_file_to_orig()
        {
            Subject.RenameOtherExtraFile(_series, _nfoPath);

            Mocker.GetMock<IDiskProvider>()
                  .Verify(v => v.MoveFile(_nfoPath, _origPath, false), Times.Once());

            Mocker.GetMock<IOtherExtraFileService>()
                  .Verify(v => v.Upsert(It.Is<OtherExtraFile>(f => f.Id == _nfoFile.Id && f.RelativePath == _origRelativePath)), Times.Once());
        }

        [Test]
        public void should_delete_existing_orig_file_and_its_extra_file()
        {
            GivenExistingOrigFile();

            Subject.RenameOtherExtraFile(_series, _nfoPath);

            Mocker.GetMock<IRecycleBinProvider>()
                  .Verify(v => v.DeleteFile(_origPath, "Season 1"), Times.Once());

            Mocker.GetMock<IOtherExtraFileService>()
                  .Verify(v => v.Delete(_origFile.Id), Times.Once());
        }

        [Test]
        public void should_not_delete_extra_file_when_there_is_no_existing_orig_file()
        {
            Subject.RenameOtherExtraFile(_series, _nfoPath);

            Mocker.GetMock<IOtherExtraFileService>()
                  .Verify(v => v.Delete(It.IsAny<int>()), Times.Never());
        }
    }
}

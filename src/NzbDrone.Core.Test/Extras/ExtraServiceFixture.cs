using System.Collections.Generic;
using System.IO;
using System.Linq;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Extras;
using NzbDrone.Core.Extras.Files;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.Extras
{
    [TestFixture]
    public class ExtraServiceFixture : CoreTest<ExtraService>
    {
        private Series _series;
        private EpisodeFile _episodeFile;
        private LocalEpisode _localEpisode;

        private string _seriesFolder;
        private string _episodeFolder;

        private Mock<IManageExtraFiles> _subtitleService;
        private Mock<IManageExtraFiles> _otherExtraService;

        [SetUp]
        public void Setup()
        {
            _seriesFolder = @"C:\Test\TV\Series Title".AsOsAgnostic();
            _episodeFolder = @"C:\Test\Unsorted TV\Series.Title.S01".AsOsAgnostic();

            _series = Builder<Series>.CreateNew()
                                     .With(s => s.Path = _seriesFolder)
                                     .Build();

            var episodes = Builder<Episode>.CreateListOfSize(1)
                                           .All()
                                           .With(e => e.SeasonNumber = 1)
                                           .Build()
                                           .ToList();

            _episodeFile = Builder<EpisodeFile>.CreateNew()
                                               .With(f => f.Path = Path.Combine(_series.Path, "Season 1", "Series Title - S01E01.mkv").AsOsAgnostic())
                                               .With(f => f.RelativePath = @"Season 1\Series Title - S01E01.mkv".AsOsAgnostic())
                                               .Build();

            _localEpisode = Builder<LocalEpisode>.CreateNew()
                                                 .With(l => l.Series = _series)
                                                 .With(l => l.Episodes = episodes)
                                                 .With(l => l.Path = Path.Combine(_episodeFolder, "Series.Title.S01E01.mkv").AsOsAgnostic())
                                                 .Build();

            _subtitleService = new Mock<IManageExtraFiles>();
            _subtitleService.SetupGet(s => s.Order).Returns(0);
            _subtitleService.Setup(s => s.CanImportFile(It.IsAny<LocalEpisode>(), It.IsAny<EpisodeFile>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                .Returns(false);
            _subtitleService.Setup(s => s.CanImportFile(It.IsAny<LocalEpisode>(), It.IsAny<EpisodeFile>(), It.IsAny<string>(), ".srt", It.IsAny<bool>()))
                .Returns(true);

            _otherExtraService = new Mock<IManageExtraFiles>();
            _otherExtraService.SetupGet(s => s.Order).Returns(1);
            _otherExtraService.Setup(s => s.CanImportFile(It.IsAny<LocalEpisode>(), It.IsAny<EpisodeFile>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                .Returns(true);

            Mocker.SetConstant<IEnumerable<IManageExtraFiles>>(new[]
            {
                _subtitleService.Object,
                _otherExtraService.Object
            });

            Mocker.GetMock<IDiskProvider>().Setup(s => s.FolderExists(It.IsAny<string>()))
                  .Returns(false);

            Mocker.GetMock<IDiskProvider>().Setup(s => s.GetParentFolder(It.IsAny<string>()))
                  .Returns((string path) => Directory.GetParent(path).FullName);

            WithExistingFolder(_series.Path);
            WithExistingFile(_episodeFile.Path);
            WithExistingFile(_localEpisode.Path);

            Mocker.GetMock<IConfigService>().Setup(v => v.ImportExtraFiles).Returns(true);
            Mocker.GetMock<IConfigService>().Setup(v => v.ExtraFileExtensions).Returns("nfo,srt");
        }

        private void WithExistingFolder(string path, bool exists = true)
        {
            var dir = Path.GetDirectoryName(path);

            if (exists && dir.IsNotNullOrWhiteSpace())
            {
                WithExistingFolder(dir);
            }

            Mocker.GetMock<IDiskProvider>().Setup(v => v.FolderExists(path)).Returns(exists);
        }

        private void WithExistingFile(string path, bool exists = true, int size = 1000)
        {
            var dir = Path.GetDirectoryName(path);

            if (exists && dir.IsNotNullOrWhiteSpace())
            {
                WithExistingFolder(dir);
            }

            Mocker.GetMock<IDiskProvider>().Setup(v => v.FileExists(path)).Returns(exists);
            Mocker.GetMock<IDiskProvider>().Setup(v => v.GetFileSize(path)).Returns(size);
        }

        private void WithExistingFiles(List<string> files)
        {
            foreach (string file in files)
            {
                WithExistingFile(file);
            }

            Mocker.GetMock<IDiskProvider>().Setup(s => s.GetFiles(_episodeFolder, It.IsAny<SearchOption>()))
                  .Returns(files.ToArray());
        }

        [Test]
        public void should_not_pass_file_if_import_disabled()
        {
            Mocker.GetMock<IConfigService>().Setup(v => v.ImportExtraFiles).Returns(false);

            var nfofile = Path.Combine(_episodeFolder, "Series.Title.S01E01.nfo").AsOsAgnostic();

            var files = new List<string>
            {
                _localEpisode.Path,
                nfofile
            };

            WithExistingFiles(files);

            Subject.ImportEpisode(_localEpisode, _episodeFile, true);

            _subtitleService.Verify(v => v.CanImportFile(_localEpisode, _episodeFile, It.IsAny<string>(), It.IsAny<string>(), true), Times.Never());
            _otherExtraService.Verify(v => v.CanImportFile(_localEpisode, _episodeFile, It.IsAny<string>(), It.IsAny<string>(), true), Times.Never());
        }

        [Test]
        [TestCase("Series Title - S01E01.sub")]
        [TestCase("Series Title - S01E01.ass")]
        public void should_not_pass_unwanted_file(string filePath)
        {
            Mocker.GetMock<IConfigService>().Setup(v => v.ImportExtraFiles).Returns(false);

            var nfofile = Path.Combine(_episodeFolder, filePath).AsOsAgnostic();

            var files = new List<string>
            {
                _localEpisode.Path,
                nfofile
            };

            WithExistingFiles(files);

            Subject.ImportEpisode(_localEpisode, _episodeFile, true);

            _subtitleService.Verify(v => v.CanImportFile(_localEpisode, _episodeFile, It.IsAny<string>(), It.IsAny<string>(), true), Times.Never());
            _otherExtraService.Verify(v => v.CanImportFile(_localEpisode, _episodeFile, It.IsAny<string>(), It.IsAny<string>(), true), Times.Never());
        }

        [Test]
        public void should_pass_subtitle_file_to_subtitle_service()
        {
            var subtitleFile = Path.Combine(_episodeFolder, "Series.Title.S01E01.en.srt").AsOsAgnostic();

            var files = new List<string>
            {
                _localEpisode.Path,
                subtitleFile
            };

            WithExistingFiles(files);

            Subject.ImportEpisode(_localEpisode, _episodeFile, true);

            _subtitleService.Verify(v => v.ImportFiles(_localEpisode, _episodeFile, new List<string> { subtitleFile }, true), Times.Once());
            _otherExtraService.Verify(v => v.ImportFiles(_localEpisode, _episodeFile, new List<string> { subtitleFile }, true), Times.Never());
        }

        [Test]
        public void should_pass_nfo_file_to_other_service()
        {
            var nfofile = Path.Combine(_episodeFolder, "Series.Title.S01E01.nfo").AsOsAgnostic();

            var files = new List<string>
            {
                _localEpisode.Path,
                nfofile
            };

            WithExistingFiles(files);

            Subject.ImportEpisode(_localEpisode, _episodeFile, true);

            _subtitleService.Verify(v => v.ImportFiles(_localEpisode, _episodeFile, new List<string> { nfofile }, true), Times.Never());
            _otherExtraService.Verify(v => v.ImportFiles(_localEpisode, _episodeFile, new List<string> { nfofile }, true), Times.Once());
        }

        [Test]
        public void should_search_subtitles_when_importing_from_job_folder()
        {
            _localEpisode.FolderEpisodeInfo = new ParsedEpisodeInfo();

            var subtitleFile = Path.Combine(_episodeFolder, "Series.Title.S01E01.en.srt").AsOsAgnostic();

            var files = new List<string>
            {
                _localEpisode.Path,
                subtitleFile
            };

            WithExistingFiles(files);

            Subject.ImportEpisode(_localEpisode, _episodeFile, true);

            Mocker.GetMock<IDiskProvider>().Verify(v => v.GetFiles(_episodeFolder, SearchOption.AllDirectories), Times.Once);
            Mocker.GetMock<IDiskProvider>().Verify(v => v.GetFiles(_episodeFolder, SearchOption.TopDirectoryOnly), Times.Never);
        }

        [Test]
        public void should_not_search_subtitles_when_not_importing_from_job_folder()
        {
            _localEpisode.FolderEpisodeInfo = null;

            var subtitleFile = Path.Combine(_episodeFolder, "Series.Title.S01E01.en.srt").AsOsAgnostic();

            var files = new List<string>
            {
                _localEpisode.Path,
                subtitleFile
            };

            WithExistingFiles(files);

            Subject.ImportEpisode(_localEpisode, _episodeFile, true);

            Mocker.GetMock<IDiskProvider>().Verify(v => v.GetFiles(_episodeFolder, SearchOption.AllDirectories), Times.Never);
            Mocker.GetMock<IDiskProvider>().Verify(v => v.GetFiles(_episodeFolder, SearchOption.TopDirectoryOnly), Times.Once);
        }
    }
}

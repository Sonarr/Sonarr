using System.Collections.Generic;
using System.IO;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Disk;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Test.ProviderTests.DiskScanProviderTests
{
    public class GetVideoFilesFixture : CoreTest<DiskScanService>
    {
        private string[] _fileNames;

        [SetUp]
        public void Setup()
        {
            _fileNames = new[]
                        {
                            @"30 Rock1.mkv",
                            @"30 Rock2.avi",
                            @"30 Rock3.MP4",
                            @"30 Rock4.wMv",
                            @"movie.exe",
                            @"movie"
                        };
        }

        private IEnumerable<string> GetFiles(string folder, string subFolder = "")
        {
            return _fileNames.Select(f => Path.Combine(folder, subFolder, f));
        }

        private void GivenFiles(IEnumerable<string> files)
        {
            var filesToReturn = files.ToArray();
            Mocker.GetMock<IDiskProvider>()
                  .Setup(s => s.GetFiles(It.IsAny<string>(), SearchOption.AllDirectories))
                  .Returns(filesToReturn);
        }

        [Test]
        public void should_check_all_directories()
        {
            var path = @"C:\Test\";

            Subject.GetVideoFiles(path);

            Mocker.GetMock<IDiskProvider>().Verify(s => s.GetFiles(path, SearchOption.AllDirectories), Times.Once());
            Mocker.GetMock<IDiskProvider>().Verify(s => s.GetFiles(path, SearchOption.TopDirectoryOnly), Times.Never());
        }

        [Test]
        public void should_check_all_directories_when_allDirectories_is_true()
        {
            var path = @"C:\Test\";

            Subject.GetVideoFiles(path, true);

            Mocker.GetMock<IDiskProvider>().Verify(s => s.GetFiles(path, SearchOption.AllDirectories), Times.Once());
            Mocker.GetMock<IDiskProvider>().Verify(s => s.GetFiles(path, SearchOption.TopDirectoryOnly), Times.Never());
        }

        [Test]
        public void should_check_top_level_directory_only_when_allDirectories_is_false()
        {
            var path = @"C:\Test\";

            Subject.GetVideoFiles(path, false);

            Mocker.GetMock<IDiskProvider>().Verify(s => s.GetFiles(path, SearchOption.AllDirectories), Times.Never());
            Mocker.GetMock<IDiskProvider>().Verify(s => s.GetFiles(path, SearchOption.TopDirectoryOnly), Times.Once());
        }

        [Test]
        public void should_return_video_files_only()
        {
            var path = @"C:\Test\";
            GivenFiles(GetFiles(path));

            Subject.GetVideoFiles(path).Should().HaveCount(4);
        }

        [TestCase("Extras")]
        [TestCase("@eadir")]
        [TestCase("extrafanart")]
        [TestCase("Plex Versions")]
        [TestCase(".secret")]
        [TestCase(".hidden")]
        [TestCase(".unwanted")]
        public void should_filter_certain_sub_folders(string subFolder)
        {
            var path = @"C:\Test\";
            var files = GetFiles(path).ToList();
            var specialFiles = GetFiles(path, subFolder).ToList();
            var allFiles = files.Concat(specialFiles);

            var filteredFiles = Subject.FilterPaths(path, allFiles);
            filteredFiles.Should().NotContain(specialFiles);
            filteredFiles.Count.Should().BeGreaterThan(0);
        }
    }
}

using System;
using System.IO;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.ProviderTests.DiskScanProviderTests
{
    
    public class GetVideoFilesFixture : CoreTest<DiskScanService>
    {
        private string[] _files;

        [SetUp]
        public void Setup()
        {
            _files = new[]
                        {
                            @"C:\Test\30 Rock1.mkv",
                            @"C:\Test\30 Rock2.avi",
                            @"C:\Test\30 Rock3.mp4",
                            @"C:\Test\30 Rock4.wmv",
                            @"C:\Test\movie.exe"
                        };

            Mocker.GetMock<IDiskProvider>()
                .Setup(s => s.GetFiles(It.IsAny<String>(), SearchOption.AllDirectories))
                .Returns(_files);
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

            Subject.GetVideoFiles(path).Should().HaveCount(4);
        }
    }
}

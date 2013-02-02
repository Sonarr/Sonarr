using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common;
using NzbDrone.Core.Model;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Providers.Core;
using NzbDrone.Core.Repository;
using NzbDrone.Core.Repository.Quality;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;
using NzbDrone.Test.Common.AutoMoq;

namespace NzbDrone.Core.Test.ProviderTests.DiskScanProviderTests
{
    // ReSharper disable InconsistentNaming
    public class GetVideoFilesFixture : SqlCeTest
    {
        private string[] _files;

        [SetUp]
        public void Setup()
        {
            _files = new string[]
                        {
                            @"C:\Test\30 Rock1.mkv",
                            @"C:\Test\30 Rock2.avi",
                            @"C:\Test\30 Rock3.mp4",
                            @"C:\Test\30 Rock4.wmv",
                            @"C:\Test\movie.exe"
                        };

            Mocker.GetMock<DiskProvider>()
                .Setup(s => s.GetFiles(It.IsAny<String>(), SearchOption.AllDirectories))
                .Returns(_files);
        }

        [Test]
        public void should_check_all_directories()
        {
            var path = @"C:\Test\";

            Mocker.Resolve<DiskScanProvider>().GetVideoFiles(path);

            Mocker.GetMock<DiskProvider>().Verify(s => s.GetFiles(path, SearchOption.AllDirectories), Times.Once());
            Mocker.GetMock<DiskProvider>().Verify(s => s.GetFiles(path, SearchOption.TopDirectoryOnly), Times.Never());
        }

        [Test]
        public void should_check_all_directories_when_allDirectories_is_true()
        {
            var path = @"C:\Test\";

            Mocker.Resolve<DiskScanProvider>().GetVideoFiles(path, true);

            Mocker.GetMock<DiskProvider>().Verify(s => s.GetFiles(path, SearchOption.AllDirectories), Times.Once());
            Mocker.GetMock<DiskProvider>().Verify(s => s.GetFiles(path, SearchOption.TopDirectoryOnly), Times.Never());
        }

        [Test]
        public void should_check_top_level_directory_only_when_allDirectories_is_false()
        {
            var path = @"C:\Test\";

            Mocker.Resolve<DiskScanProvider>().GetVideoFiles(path, false);

            Mocker.GetMock<DiskProvider>().Verify(s => s.GetFiles(path, SearchOption.AllDirectories), Times.Never());
            Mocker.GetMock<DiskProvider>().Verify(s => s.GetFiles(path, SearchOption.TopDirectoryOnly), Times.Once());
        }

        [Test]
        public void should_return_video_files_only()
        {
            var path = @"C:\Test\";

            Mocker.Resolve<DiskScanProvider>().GetVideoFiles(path).Should().HaveCount(4);
        }
    }
}

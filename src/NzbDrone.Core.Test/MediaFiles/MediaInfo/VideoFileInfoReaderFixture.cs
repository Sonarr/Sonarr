using System.IO;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Disk;
using NzbDrone.Core.MediaFiles.MediaInfo;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common.Categories;

namespace NzbDrone.Core.Test.MediaFiles.MediaInfo
{
    [TestFixture]
    [DiskAccessTest]
    public class VideoFileInfoReaderFixture : CoreTest<VideoFileInfoReader>
    {
        [SetUp]
        public void Setup()
        {
            Mocker.GetMock<IDiskProvider>()
                  .Setup(s => s.FileExists(It.IsAny<string>()))
                  .Returns(true);
        }

        [Test]
        public void get_runtime()
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Files", "Media", "H264_sample.mp4");

            Subject.GetRunTime(path).Seconds.Should().Be(10);

        }


        [Test]
        public void get_info()
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Files", "Media", "H264_sample.mp4");

            var info = Subject.GetMediaInfo(path);


            info.AudioBitrate.Should().Be(128000);
            info.AudioChannels.Should().Be(2);
            info.AudioFormat.Should().Be("AAC");
            info.AudioLanguages.Should().Be("English");
            info.AudioProfile.Should().Be("LC");
            info.Height.Should().Be(320);
            info.RunTime.Seconds.Should().Be(10);
            info.ScanType.Should().Be("Progressive");
            info.Subtitles.Should().Be("");
            info.VideoBitrate.Should().Be(193329);
            info.VideoCodec.Should().Be("AVC");
            info.VideoFps.Should().Be(24);
            info.Width.Should().Be(480);

        }
    }
}
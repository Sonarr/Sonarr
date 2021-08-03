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

            Mocker.GetMock<IDiskProvider>()
                  .Setup(s => s.OpenReadStream(It.IsAny<string>()))
                  .Returns<string>(s => new FileStream(s, FileMode.Open, FileAccess.Read));
        }

        [Test]
        public void get_runtime()
        {
            var path = Path.Combine(TestContext.CurrentContext.TestDirectory, "Files", "Media", "H264_sample.mp4");

            Subject.GetRunTime(path).Value.Seconds.Should().Be(10);
        }

        [Test]
        public void get_info()
        {
            var path = Path.Combine(TestContext.CurrentContext.TestDirectory, "Files", "Media", "H264_sample.mp4");

            var info = Subject.GetMediaInfo(path);

            info.VideoCodec.Should().BeNull();
            info.VideoFormat.Should().Be("AVC");
            info.VideoCodecID.Should().Be("avc1");
            info.VideoProfile.Should().Be("Baseline@L2.1");
            info.VideoCodecLibrary.Should().Be("");
            info.AudioFormat.Should().Be("AAC");
            info.AudioCodecID.Should().BeOneOf("40", "mp4a-40-2");
            info.AudioProfile.Should().BeOneOf("", "LC");
            info.AudioCodecLibrary.Should().Be("");
            info.AudioBitrate.Should().Be(128000);
            info.AudioChannelsContainer.Should().Be(2);
            info.AudioChannelsStream.Should().Be(0);
            info.AudioChannelPositionsTextContainer.Should().Be("Front: L R");
            info.AudioChannelPositionsTextStream.Should().Be("");
            info.AudioLanguages.Should().Be("English");
            info.Height.Should().Be(320);
            info.RunTime.Seconds.Should().Be(10);
            info.ScanType.Should().Be("Progressive");
            info.Subtitles.Should().Be("");
            info.VideoBitrate.Should().Be(193329);
            info.VideoFps.Should().Be(24);
            info.Width.Should().Be(480);
            info.VideoColourPrimaries.Should().Be("BT.601 NTSC");
            info.VideoTransferCharacteristics.Should().Be("BT.709");
            info.AudioAdditionalFeatures.Should().BeOneOf("", "LC");
            info.VideoHdrFormat.Should().BeEmpty();
            info.VideoHdrFormatCompatibility.Should().BeEmpty();
        }

        [Test]
        public void get_info_unicode()
        {
            var srcPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "Files", "Media", "H264_sample.mp4");

            var tempPath = GetTempFilePath();
            Directory.CreateDirectory(tempPath);

            var path = Path.Combine(tempPath, "H264_Pok\u00E9mon.mkv");

            File.Copy(srcPath, path);

            var info = Subject.GetMediaInfo(path);

            info.VideoCodec.Should().BeNull();
            info.VideoFormat.Should().Be("AVC");
            info.VideoCodecID.Should().Be("avc1");
            info.VideoProfile.Should().Be("Baseline@L2.1");
            info.VideoCodecLibrary.Should().Be("");
            info.AudioFormat.Should().Be("AAC");
            info.AudioCodecID.Should().BeOneOf("40", "mp4a-40-2");
            info.AudioProfile.Should().BeOneOf("", "LC");
            info.AudioCodecLibrary.Should().Be("");
            info.AudioBitrate.Should().Be(128000);
            info.AudioChannelsContainer.Should().Be(2);
            info.AudioChannelsStream.Should().Be(0);
            info.AudioChannelPositionsTextContainer.Should().Be("Front: L R");
            info.AudioChannelPositionsTextStream.Should().Be("");
            info.AudioLanguages.Should().Be("English");
            info.Height.Should().Be(320);
            info.RunTime.Seconds.Should().Be(10);
            info.ScanType.Should().Be("Progressive");
            info.Subtitles.Should().Be("");
            info.VideoBitrate.Should().Be(193329);
            info.VideoFps.Should().Be(24);
            info.Width.Should().Be(480);
            info.VideoColourPrimaries.Should().Be("BT.601 NTSC");
            info.VideoTransferCharacteristics.Should().Be("BT.709");
            info.AudioAdditionalFeatures.Should().BeOneOf("", "LC");
            info.VideoHdrFormat.Should().BeEmpty();
            info.VideoHdrFormatCompatibility.Should().BeEmpty();
        }

        [Test]
        public void should_dispose_file_after_scanning_mediainfo()
        {
            var path = Path.Combine(TestContext.CurrentContext.TestDirectory, "Files", "Media", "H264_sample.mp4");

            var info = Subject.GetMediaInfo(path);

            var stream = new FileStream(path, FileMode.Open, FileAccess.Write);

            stream.Close();
        }
    }
}

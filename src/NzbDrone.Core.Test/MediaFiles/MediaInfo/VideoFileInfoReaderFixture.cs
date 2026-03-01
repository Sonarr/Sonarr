using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json.Nodes;
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

            info.VideoFormat.Should().Be("h264");
            info.VideoCodecID.Should().Be("avc1");
            info.VideoProfile.Should().Be("Constrained Baseline");
            info.PrimaryAudioStream.Format.Should().Be("aac");
            info.PrimaryAudioStream.CodecId.Should().Be("mp4a");
            info.PrimaryAudioStream.Profile.Should().Be("LC");
            info.PrimaryAudioStream.Bitrate.Should().Be(125509);
            info.PrimaryAudioStream.Channels.Should().Be(2);
            info.PrimaryAudioStream.ChannelPositions.Should().Be("stereo");
            info.AudioStreams?.Select(l => l.Language).Should().BeEquivalentTo("eng");
            info.Height.Should().Be(320);
            info.RunTime.Seconds.Should().Be(10);
            info.ScanType.Should().Be("Progressive");
            info.SubtitleStreams?.Select(l => l.Language).Should().BeEmpty();
            info.VideoBitrate.Should().Be(193694);
            info.VideoFps.Should().Be(24);
            info.Width.Should().Be(480);
            info.VideoBitDepth.Should().Be(8);
            info.VideoColourPrimaries.Should().Be("smpte170m");
            info.VideoTransferCharacteristics.Should().Be("bt709");
            info.Title.Should().Be("Sample Title");
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

            info.VideoFormat.Should().Be("h264");
            info.VideoCodecID.Should().Be("avc1");
            info.VideoProfile.Should().Be("Constrained Baseline");
            info.PrimaryAudioStream.Format.Should().Be("aac");
            info.PrimaryAudioStream.CodecId.Should().Be("mp4a");
            info.PrimaryAudioStream.Profile.Should().Be("LC");
            info.PrimaryAudioStream.Bitrate.Should().Be(125509);
            info.PrimaryAudioStream.Channels.Should().Be(2);
            info.PrimaryAudioStream.ChannelPositions.Should().Be("stereo");
            info.AudioStreams?.Select(l => l.Language).Should().BeEquivalentTo("eng");
            info.Height.Should().Be(320);
            info.RunTime.Seconds.Should().Be(10);
            info.ScanType.Should().Be("Progressive");
            info.SubtitleStreams?.Select(l => l.Language).Should().BeEmpty();
            info.VideoBitrate.Should().Be(193694);
            info.VideoFps.Should().Be(24);
            info.Width.Should().Be(480);
            info.VideoColourPrimaries.Should().Be("smpte170m");
            info.VideoTransferCharacteristics.Should().Be("bt709");
            info.Title.Should().Be("Sample Title");
        }

        [TestCase(8, "", "", null, null, HdrFormat.None)]
        [TestCase(10, "", "", null, null, HdrFormat.None)]
        [TestCase(10, "bt709", "bt709", null, null, HdrFormat.None)]
        [TestCase(8, "bt2020", "smpte2084", null, null, HdrFormat.None)]
        [TestCase(10, "bt2020", "bt2020-10", null, null, HdrFormat.None)]
        [TestCase(10, "bt2020", "arib-std-b67", null, null, HdrFormat.Hlg10)]
        [TestCase(10, "bt2020", "smpte2084", null, null, HdrFormat.Pq10)]
        [TestCase(10, "bt2020", "smpte2084", new[] { "" }, null, HdrFormat.Pq10)]
        [TestCase(10, "bt2020", "smpte2084", new[] { FFMpegCoreSideDataTypes.MasteringDisplayMetadata }, null, HdrFormat.Hdr10)]
        [TestCase(10, "bt2020", "smpte2084", new[] { FFMpegCoreSideDataTypes.ContentLightLevelMetadata }, null, HdrFormat.Hdr10)]
        [TestCase(10, "bt2020", "smpte2084", new[] { FFMpegCoreSideDataTypes.HdrDynamicMetadataSpmte2094 }, null, HdrFormat.Hdr10Plus)]
        [TestCase(10, "bt2020", "smpte2084", new[] { FFMpegCoreSideDataTypes.DoviConfigurationRecordSideData }, null, HdrFormat.DolbyVision)]
        [TestCase(10, "bt2020", "smpte2084", new[] { FFMpegCoreSideDataTypes.DoviConfigurationRecordSideData }, 1, HdrFormat.DolbyVisionHdr10)]
        [TestCase(10, "bt2020", "smpte2084", new[] { FFMpegCoreSideDataTypes.DoviConfigurationRecordSideData, FFMpegCoreSideDataTypes.HdrDynamicMetadataSpmte2094 }, 1, HdrFormat.DolbyVisionHdr10Plus)]
        [TestCase(10, "bt2020", "smpte2084", new[] { FFMpegCoreSideDataTypes.DoviConfigurationRecordSideData, FFMpegCoreSideDataTypes.HdrDynamicMetadataSpmte2094 }, 6, HdrFormat.DolbyVisionHdr10Plus)]
        [TestCase(10, "bt2020", "smpte2084", new[] { FFMpegCoreSideDataTypes.DoviConfigurationRecordSideData }, 2, HdrFormat.DolbyVisionSdr)]
        [TestCase(10, "bt2020", "smpte2084", new[] { FFMpegCoreSideDataTypes.DoviConfigurationRecordSideData }, 4, HdrFormat.DolbyVisionHlg)]
        public void should_detect_hdr_correctly(int bitDepth, string colourPrimaries, string transferFunction, string[] sideDataTypes, int? doviConfigId, HdrFormat expected)
        {
            var sideData = sideDataTypes?.Select(sideDataType =>
            {
                var sideData = new Dictionary<string, JsonNode>
                {
                    { "side_data_type", JsonValue.Create(sideDataType) }
                };

                if (doviConfigId.HasValue)
                {
                    sideData.Add("dv_bl_signal_compatibility_id", JsonValue.Create(doviConfigId.Value));
                }

                return sideData;
            }).ToList();

            var result = VideoFileInfoReader.GetHdrFormat(bitDepth, colourPrimaries, transferFunction, sideData);

            result.Should().Be(expected);
        }
    }
}

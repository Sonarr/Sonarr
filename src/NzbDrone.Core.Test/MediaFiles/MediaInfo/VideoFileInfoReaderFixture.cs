using System;
using System.IO;
using System.Linq;
using System.Reflection;
using FFMpegCore;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
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
            info.AudioFormat.Should().Be("aac");
            info.AudioCodecID.Should().Be("mp4a");
            info.AudioProfile.Should().Be("LC");
            info.AudioBitrate.Should().Be(125488);
            info.AudioChannels.Should().Be(2);
            info.AudioChannelPositions.Should().Be("stereo");
            info.AudioLanguages.Should().BeEquivalentTo("eng");
            info.Height.Should().Be(320);
            info.RunTime.Seconds.Should().Be(10);
            info.ScanType.Should().Be("Progressive");
            info.Subtitles.Should().BeEmpty();
            info.VideoBitrate.Should().Be(193328);
            info.VideoFps.Should().Be(24);
            info.Width.Should().Be(480);
            info.VideoBitDepth.Should().Be(8);
            info.VideoColourPrimaries.Should().Be("smpte170m");
            info.VideoTransferCharacteristics.Should().Be("bt709");
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
            info.AudioFormat.Should().Be("aac");
            info.AudioCodecID.Should().Be("mp4a");
            info.AudioProfile.Should().Be("LC");
            info.AudioBitrate.Should().Be(125488);
            info.AudioChannels.Should().Be(2);
            info.AudioChannelPositions.Should().Be("stereo");
            info.AudioLanguages.Should().BeEquivalentTo("eng");
            info.Height.Should().Be(320);
            info.RunTime.Seconds.Should().Be(10);
            info.ScanType.Should().Be("Progressive");
            info.Subtitles.Should().BeEmpty();
            info.VideoBitrate.Should().Be(193328);
            info.VideoFps.Should().Be(24);
            info.Width.Should().Be(480);
            info.VideoColourPrimaries.Should().Be("smpte170m");
            info.VideoTransferCharacteristics.Should().Be("bt709");
        }

        [TestCase(8, "", "", "", null, HdrFormat.None)]
        [TestCase(10, "", "", "", null, HdrFormat.None)]
        [TestCase(10, "bt709", "bt709", "", null, HdrFormat.None)]
        [TestCase(8, "bt2020", "smpte2084", "", null, HdrFormat.None)]
        [TestCase(10, "bt2020", "bt2020-10", "", null, HdrFormat.Hlg10)]
        [TestCase(10, "bt2020", "arib-std-b67", "", null, HdrFormat.Hlg10)]
        [TestCase(10, "bt2020", "smpte2084", "", null, HdrFormat.Pq10)]
        [TestCase(10, "bt2020", "smpte2084", "FFMpegCore.SideData", null, HdrFormat.Pq10)]
        [TestCase(10, "bt2020", "smpte2084", "FFMpegCore.MasteringDisplayMetadata", null, HdrFormat.Hdr10)]
        [TestCase(10, "bt2020", "smpte2084", "FFMpegCore.ContentLightLevelMetadata", null, HdrFormat.Hdr10)]
        [TestCase(10, "bt2020", "smpte2084", "FFMpegCore.HdrDynamicMetadataSpmte2094", null, HdrFormat.Hdr10Plus)]
        [TestCase(10, "bt2020", "smpte2084", "FFMpegCore.DoviConfigurationRecordSideData", null, HdrFormat.DolbyVision)]
        [TestCase(10, "bt2020", "smpte2084", "FFMpegCore.DoviConfigurationRecordSideData", 1, HdrFormat.DolbyVisionHdr10)]
        [TestCase(10, "bt2020", "smpte2084", "FFMpegCore.DoviConfigurationRecordSideData", 2, HdrFormat.DolbyVisionSdr)]
        [TestCase(10, "bt2020", "smpte2084", "FFMpegCore.DoviConfigurationRecordSideData", 4, HdrFormat.DolbyVisionHlg)]
        public void should_detect_hdr_correctly(int bitDepth, string colourPrimaries, string transferFunction, string sideDataTypes, int? doviConfigId, HdrFormat expected)
        {
            var assembly = Assembly.GetAssembly(typeof(FFProbe));
            var types = sideDataTypes.Split(",").Select(x => x.Trim()).ToList();
            var sideData = types.Where(x => x.IsNotNullOrWhiteSpace()).Select(x => assembly.CreateInstance(x)).Cast<SideData>().ToList();

            if (doviConfigId.HasValue)
            {
                sideData.ForEach(x =>
                {
                    if (x.GetType().Name == "DoviConfigurationRecordSideData")
                    {
                        ((DoviConfigurationRecordSideData)x).DvBlSignalCompatibilityId = doviConfigId.Value;
                    }
                });
            }

            var result = VideoFileInfoReader.GetHdrFormat(bitDepth, colourPrimaries, transferFunction, sideData);

            result.Should().Be(expected);
        }
    }
}

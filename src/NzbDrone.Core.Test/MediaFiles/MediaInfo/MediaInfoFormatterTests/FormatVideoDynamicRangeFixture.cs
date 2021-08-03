using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.MediaFiles.MediaInfo;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.MediaFiles.MediaInfo.MediaInfoFormatterTests
{
    [TestFixture]
    public class FormatVideoDynamicRangeFixture : TestBase
    {
        [TestCase(8, "", "", "", "", "")]
        [TestCase(8, "BT.601 NTSC", "BT.709", "", "", "")]
        [TestCase(10, "BT.2020", "PQ", "", "", "HDR")]
        [TestCase(8, "BT.2020", "PQ", "", "", "")]
        [TestCase(10, "BT.601 NTSC", "PQ", "", "", "")]
        [TestCase(10, "BT.2020", "BT.709", "", "", "")]
        [TestCase(10, "BT.2020", "HLG", "", "", "HDR")]
        [TestCase(10, "", "", "Dolby Vision", "", "HDR")]
        [TestCase(10, "", "", "SMPTE ST 2086", "HDR10", "HDR")]
        [TestCase(8, "", "", "Dolby Vision", "", "")]
        [TestCase(8, "", "", "SMPTE ST 2086", "HDR10", "")]
        [TestCase(10, "BT.2020", "PQ", "Dolby Vision / SMPTE ST 2086", "Blu-ray / HDR10", "HDR")]
        public void should_format_video_dynamic_range(int bitDepth, string colourPrimaries, string transferCharacteristics, string hdrFormat, string hdrFormatCompatibility, string expectedVideoDynamicRange)
        {
            var mediaInfo = new MediaInfoModel
            {
                VideoBitDepth = bitDepth,
                VideoColourPrimaries = colourPrimaries,
                VideoTransferCharacteristics = transferCharacteristics,
                VideoHdrFormat = hdrFormat,
                VideoHdrFormatCompatibility = hdrFormatCompatibility,
                SchemaRevision = 7
            };

            MediaInfoFormatter.FormatVideoDynamicRange(mediaInfo).Should().Be(expectedVideoDynamicRange);
        }

        [TestCase(8, "", "", "", "")]
        [TestCase(8, "BT.601 NTSC", "BT.709", "", "")]
        [TestCase(8, "BT.2020", "PQ", "", "")]
        [TestCase(8, "", "", "Dolby Vision", "")]
        [TestCase(8, "", "", "SMPTE ST 2086", "")]
        [TestCase(10, "BT.601 NTSC", "PQ", "", "")]
        [TestCase(10, "BT.2020", "BT.709", "", "")]
        [TestCase(10, "BT.2020", "PQ", "", "PQ")]
        [TestCase(10, "BT.2020", "HLG", "", "HLG")]
        [TestCase(10, "", "", "SMPTE ST 2086", "HDR10")]
        [TestCase(10, "", "", "HDR10", "HDR10")]
        [TestCase(10, "", "", "SMPTE ST 2094 App 4", "HDR10Plus")]
        [TestCase(10, "", "", "Dolby Vision", "DV")]
        [TestCase(10, "BT.2020", "PQ", "Dolby Vision / SMPTE ST 2086", "DV HDR10")]
        [TestCase(10, "", "", "SL-HDR1", "HDR")]
        [TestCase(10, "", "", "SL-HDR2", "HDR")]
        [TestCase(10, "", "", "SL-HDR3", "HDR")]
        [TestCase(10, "", "", "Technicolor Advanced HDR", "HDR")]
        public void should_format_video_dynamic_range_type(int bitDepth, string colourPrimaries, string transferCharacteristics, string hdrFormat, string expectedVideoDynamicRange)
        {
            var mediaInfo = new MediaInfoModel
            {
                VideoBitDepth = bitDepth,
                VideoColourPrimaries = colourPrimaries,
                VideoTransferCharacteristics = transferCharacteristics,
                VideoHdrFormat = hdrFormat,
                SchemaRevision = 7
            };

            MediaInfoFormatter.FormatVideoDynamicRangeType(mediaInfo).Should().Be(expectedVideoDynamicRange);
        }
    }
}

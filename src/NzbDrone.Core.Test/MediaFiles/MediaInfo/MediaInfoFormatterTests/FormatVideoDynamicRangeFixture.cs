using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.MediaFiles.MediaInfo;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.MediaFiles.MediaInfo.MediaInfoFormatterTests
{
    [TestFixture]
    public class FormatVideoDynamicRangeFixture : TestBase
    {
        [TestCase(8, "BT.601 NTSC", "BT.709", "")]
        [TestCase(10, "BT.2020", "PQ", "HDR")]
        [TestCase(8, "BT.2020", "PQ", "")]
        [TestCase(10, "BT.601 NTSC", "PQ", "")]
        [TestCase(10, "BT.2020", "BT.709", "")]
        [TestCase(10, "BT.2020", "HLG", "HDR")]
        public void should_format_video_dynamic_range(int bitDepth, string colourPrimaries, string transferCharacteristics, string expectedVideoDynamicRange)
        {
            var mediaInfo = new MediaInfoModel
            {
                VideoBitDepth = bitDepth,
                VideoColourPrimaries = colourPrimaries,
                VideoTransferCharacteristics = transferCharacteristics,
                SchemaRevision = 5
            };

            MediaInfoFormatter.FormatVideoDynamicRange(mediaInfo).Should().Be(expectedVideoDynamicRange);
        }
    }
}
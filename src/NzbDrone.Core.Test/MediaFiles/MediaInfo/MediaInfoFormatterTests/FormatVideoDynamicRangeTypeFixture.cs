using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.MediaFiles.MediaInfo;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.MediaFiles.MediaInfo.MediaInfoFormatterTests
{
    [TestFixture]
    public class FormatVideoDynamicRangeTypeFixture : TestBase
    {
        [TestCase(HdrFormat.None, "")]
        [TestCase(HdrFormat.Hlg10, "HLG")]
        [TestCase(HdrFormat.Pq10, "PQ")]
        [TestCase(HdrFormat.Hdr10, "HDR10")]
        [TestCase(HdrFormat.Hdr10Plus, "HDR10Plus")]
        [TestCase(HdrFormat.DolbyVision, "DV")]
        [TestCase(HdrFormat.DolbyVisionHdr10, "DV HDR10")]
        [TestCase(HdrFormat.DolbyVisionHlg, "DV HLG")]
        [TestCase(HdrFormat.DolbyVisionSdr, "DV SDR")]
        public void should_format_video_dynamic_range_type(HdrFormat format, string expectedVideoDynamicRangeType)
        {
            var mediaInfo = new MediaInfoModel
            {
                VideoHdrFormat = format,
                SchemaRevision = 9
            };

            MediaInfoFormatter.FormatVideoDynamicRangeType(mediaInfo).Should().Be(expectedVideoDynamicRangeType);
        }
    }
}

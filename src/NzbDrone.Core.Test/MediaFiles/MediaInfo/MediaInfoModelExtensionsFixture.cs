using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.MediaFiles.MediaInfo;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.MediaFiles.MediaInfo
{
    [TestFixture]
    public class MediaInfoModelExtensionsFixture : TestBase
    {
        [TestCase(8, "", "", "", HdrFormat.None)]
        [TestCase(8, "BT.601 NTSC", "BT.709", "", HdrFormat.None)]
        [TestCase(8, "BT.2020", "PQ", "", HdrFormat.None)]
        [TestCase(8, "", "", "Dolby Vision", HdrFormat.None)]
        [TestCase(8, "", "", "SMPTE ST 2086", HdrFormat.None)]
        [TestCase(10, "BT.601 NTSC", "PQ", "", HdrFormat.None)]
        [TestCase(10, "BT.2020", "BT.709", "", HdrFormat.None)]
        [TestCase(10, "BT.2020", "PQ", "", HdrFormat.Pq10)]
        [TestCase(10, "BT.2020", "HLG", "", HdrFormat.Hlg10)]
        [TestCase(10, "", "", "SMPTE ST 2086", HdrFormat.Hdr10)]
        [TestCase(10, "", "", "HDR10", HdrFormat.Hdr10)]
        [TestCase(10, "", "", "SMPTE ST 2094 App 4", HdrFormat.Hdr10Plus)]
        [TestCase(10, "", "", "Dolby Vision", HdrFormat.DolbyVision)]
        [TestCase(10, "BT.2020", "PQ", "Dolby Vision / SMPTE ST 2086", HdrFormat.DolbyVisionHdr10)]
        [TestCase(10, "", "", "SL-HDR1", HdrFormat.UnknownHdr)]
        [TestCase(10, "", "", "SL-HDR2", HdrFormat.UnknownHdr)]
        [TestCase(10, "", "", "SL-HDR3", HdrFormat.UnknownHdr)]
        [TestCase(10, "", "", "Technicolor Advanced HDR", HdrFormat.UnknownHdr)]
        public void should_get_hdr_format(int bitDepth, string colourPrimaries, string transferCharacteristics, string hdrFormat, HdrFormat expectedVideoDynamicRange)
        {
            var mediaInfo = new MediaInfoModel
            {
                VideoBitDepth = bitDepth,
                VideoColourPrimaries = colourPrimaries,
                VideoTransferCharacteristics = transferCharacteristics,
                VideoHdrFormat = hdrFormat,
                SchemaRevision = 7
            };

            MediaInfoModelExtensions.GetHdrFormat(mediaInfo).Should().Be(expectedVideoDynamicRange);
        }
    }
}

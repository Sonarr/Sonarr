using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.MediaFiles.MediaInfo;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.MediaFiles.MediaInfo.MediaInfoFormatterTests
{
    [TestFixture]
    public class FormatVideoDynamicRangeFixture : TestBase
    {
        [TestCase(8, "BT.601 NTSC", "BT.709", "SDR")]
        [TestCase(10, "BT.2020", "PQ", "HDR")]
        [TestCase(8, "BT.2020", "PQ", "SDR")]
        [TestCase(10, "BT.601 NTSC", "PQ", "SDR")]
        [TestCase(10, "BT.2020", "BT.709", "SDR")]
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

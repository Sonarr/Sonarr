using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.MediaFiles.MediaInfo;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.MediaFiles.MediaInfo.MediaInfoFormatterTests
{
    [TestFixture]
    public class FormatVideoCodecFixture : TestBase
    {
        [TestCase("AVC", null, "x264")]
        [TestCase("AVC", "source.title.x264.720p-Sonarr", "x264")]
        [TestCase("AVC", "source.title.h264.720p-Sonarr", "h264")]
        [TestCase("V_MPEGH/ISO/HEVC", null, "x265")]
        [TestCase("V_MPEGH/ISO/HEVC", "source.title.x265.720p-Sonarr", "x265")]
        [TestCase("V_MPEGH/ISO/HEVC", "source.title.h265.720p-Sonarr", "h265")]
        [TestCase("MPEG-2 Video", null, "MPEG2")]
        public void should_format_video_codec_with_source_title(string videoCodec, string sceneName, string expectedFormat)
        {
            var mediaInfoModel = new MediaInfoModel
            {
                VideoCodec = videoCodec
            };

            MediaInfoFormatter.FormatVideoCodec(mediaInfoModel, sceneName).Should().Be(expectedFormat);
        }

        [Test]
        public void should_return_VideoCodec_by_default()
        {
            var mediaInfoModel = new MediaInfoModel
            {
                VideoCodec = "VideoCodec"
            };

            MediaInfoFormatter.FormatVideoCodec(mediaInfoModel, null).Should().Be(mediaInfoModel.VideoCodec);
            ExceptionVerification.ExpectedErrors(1);
        }
    }
}

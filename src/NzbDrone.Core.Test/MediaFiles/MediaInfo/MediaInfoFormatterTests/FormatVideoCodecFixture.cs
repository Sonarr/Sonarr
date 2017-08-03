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

        [TestCase("MPEG Video, 2, Main@High, ", "Droned.S01E02.1080i.HDTV.DD5.1.MPEG2-NTb", "MPEG2")]
        [TestCase("MPEG Video, V_MPEG2, Main@High, ", "", "MPEG2")]
        [TestCase("MPEG Video, , , ", "The.Simpsons.S13E04.INTERNAL-ANiVCD.mpg", "MPEG")]
        [TestCase("VC-1, WVC1, Advanced@L4, ", "B.N.S04E18.720p.WEB-DL", "VC1")]
        [TestCase("VC-1, V_MS/VFW/FOURCC / WVC1, Advanced@L3, ", "", "VC1")]
        [TestCase("VC-1, WMV3, MP@LL, ", "It's Always Sunny S07E13 The Gang's RevengeHDTV.XviD-2HD.avi", "VC1")]
        [TestCase("V.MPEG4/ISO/AVC, V.MPEG4/ISO/AVC, , ", "pd.2015.S03E08.720p.iP.WEBRip.AAC2.0.H264-BTW", "h264")]
        [TestCase("WMV2, WMV2, , ", "Droned.wmv", "WMV")]
        [TestCase("xvid, xvid, , ", "", "XviD")]
        [TestCase("div3, div3, , ", "spsm.dvdrip.divx.avi'.", "DivX")]
        [TestCase("VP6, 4, , ", "Top Gear - S12E01 - Lorries - SD TV.flv", "VP6")]
        [TestCase("VP7, VP70, General, ", "Sweet Seymour.avi", "VP7")]
        [TestCase("VP8, V_VP8, , ", "Dick.mkv", "VP8")]
        [TestCase("VP9, V_VP9, , ", "Roadkill Ep3x11 - YouTube.webm", "VP9")]
        public void should_format_video_format(string videoFormatPack, string sceneName, string expectedFormat)
        {
            var split = videoFormatPack.Split(new string[] { ", " }, System.StringSplitOptions.None);
            var mediaInfoModel = new MediaInfoModel
            {
                VideoFormat = split[0],
                VideoCodecID = split[1],
                VideoProfile = split[2],
                VideoCodecLibrary = split[3]
            };

            MediaInfoFormatter.FormatVideoCodec(mediaInfoModel, sceneName).Should().Be(expectedFormat);
        }

        [Test]
        public void should_return_VideoFormat_by_default()
        {
            var mediaInfoModel = new MediaInfoModel
            {
                VideoFormat = "VideoCodec"
            };

            MediaInfoFormatter.FormatVideoCodec(mediaInfoModel, null).Should().Be(mediaInfoModel.VideoFormat);
            ExceptionVerification.ExpectedWarns(1);
        }
    }
}

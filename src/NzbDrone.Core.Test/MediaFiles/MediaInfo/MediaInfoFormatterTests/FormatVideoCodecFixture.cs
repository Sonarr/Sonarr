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
        public void should_format_video_codec_with_source_title_legacy(string videoCodec, string sceneName, string expectedFormat)
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
        [TestCase("AVC / AVC, V_MPEG4/ISO/AVC, High@L4, ", "Resistance.2019.S01E03.1080p.RTE.WEB-DL.AAC2.0.x264-RTN", "x264")]
        [TestCase("WMV1, WMV1, , ", "Droned.wmv", "WMV")]
        [TestCase("WMV2, WMV2, , ", "Droned.wmv", "WMV")]
        [TestCase("xvid, xvid, , ", "", "XviD")]
        [TestCase("div3, div3, , ", "spsm.dvdrip.divx.avi'.", "DivX")]
        [TestCase("VP6, 4, , ", "Top Gear - S12E01 - Lorries - SD TV.flv", "VP6")]
        [TestCase("VP7, VP70, General, ", "Sweet Seymour.avi", "VP7")]
        [TestCase("VP8, V_VP8, , ", "Dick.mkv", "VP8")]
        [TestCase("VP9, V_VP9, , ", "Roadkill Ep3x11 - YouTube.webm", "VP9")]
        [TestCase("x264, x264, , ", "Ghost Advent - S04E05 - Stanley Hotel SDTV.avi", "x264")]
        [TestCase("V_MPEGH/ISO/HEVC, V_MPEGH/ISO/HEVC, , ", "The BBT S11E12 The Matrimonial Metric 1080p 10bit AMZN WEB-DL", "h265")]
        [TestCase("MPEG-4 Visual, 20, Simple@L1, Lavc52.29.0", "Will.And.Grace.S08E14.WS.DVDrip.XviD.I.Love.L.Gay-Obfuscated", "XviD")]
        [TestCase("MPEG-4 Visual, 20, Advanced Simple@L5, XviD0046", "", "XviD")]
        [TestCase("MPEG-4 Visual, 20, , ", "", "")]
        [TestCase("MPEG-4 Visual, mp4v-20, Simple@L1, Lavc57.48.101", "", "")]
        [TestCase("mp4v, mp4v, , ", "American.Chopper.S06E07.Mountain.Creek.Bike.DSR.XviD-KRS", "XviD")]
        [TestCase("V_QUICKTIME, V_QUICKTIME, , ", "Custom", "")]
        [TestCase("MPEG-4 Visual, FMP4, , ", "", "")]
        [TestCase("MPEG-4 Visual, MP42, , ", "", "")]
        [TestCase("mp43, V_MS/VFW/FOURCC / mp43, , ", "Bubble.Guppies.S01E13.480p.WEB-DL.H.264-BTN-Custom", "")]
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

        [TestCase("AVC, AVC, , x264", "Some.Video.S01E01.h264", "x264")] // Force mediainfo tag
        [TestCase("HEVC, HEVC, , x265", "Some.Video.S01E01.h265", "x265")] // Force mediainfo tag
        [TestCase("AVC, AVC, , ", "Some.Video.S01E01.x264", "x264")] // Not seen in practice, but honor tag if otherwise unknown
        [TestCase("HEVC, HEVC, , ", "Some.Video.S01E01.x265", "x265")] // Not seen in practice, but honor tag if otherwise unknown
        [TestCase("AVC, AVC, , ", "Some.Video.S01E01", "h264")] // Default value
        [TestCase("HEVC, HEVC, , ", "Some.Video.S01E01", "h265")] // Default value
        public void should_format_video_format_fallbacks(string videoFormatPack, string sceneName, string expectedFormat)
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

        [TestCase("MPEG-4 Visual, 20, , Intel(R) MPEG-4 encoder based on Intel(R) IPP 6.1 build 137.20[6.1.137.763]", "", "")]
        public void should_warn_on_unknown_video_format(string videoFormatPack, string sceneName, string expectedFormat)
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
        }
    }
}

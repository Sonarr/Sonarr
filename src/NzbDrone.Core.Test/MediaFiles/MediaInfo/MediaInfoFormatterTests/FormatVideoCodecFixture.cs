using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.MediaFiles.MediaInfo;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.MediaFiles.MediaInfo.MediaInfoFormatterTests
{
    [TestFixture]
    public class FormatVideoCodecFixture : TestBase
    {
        [TestCase("mpeg2video, ", "Droned.S01E02.1080i.HDTV.DD5.1.MPEG2-NTb", "MPEG2")]
        [TestCase("mpeg2video, ", "", "MPEG2")]
        [TestCase("mpeg1video, ", "The.Simpsons.S13E04.INTERNAL-ANiVCD.mpg", "MPEG")]
        [TestCase("vc1, WVC1", "B.N.S04E18.720p.WEB-DL", "VC1")]
        [TestCase("vc1, V_MS/VFW/FOURCC/WVC1", "", "VC1")]
        [TestCase("vc1, WMV3", "It's Always Sunny S07E13 The Gang's RevengeHDTV.XviD-2HD.avi", "VC1")]
        [TestCase("h264, V.MPEG4/ISO/AVC", "pd.2015.S03E08.720p.iP.WEBRip.AAC2.0.H264-BTW", "h264")]
        [TestCase("h264, V_MPEG4/ISO/AVC", "Resistance.2019.S01E03.1080p.RTE.WEB-DL.AAC2.0.x264-RTN", "x264")]
        [TestCase("wmv1, WMV1", "Droned.wmv", "WMV")]
        [TestCase("wmv2, WMV2", "Droned.wmv", "WMV")]
        [TestCase("mpeg4, XVID", "", "XviD")]
        [TestCase("mpeg4, DIVX", "", "DivX")]
        [TestCase("mpeg4, divx", "", "DivX")]
        [TestCase("mpeg4, DIV3", "spsm.dvdrip.divx.avi'.", "DivX")]
        [TestCase("msmpeg4, DIV3", "Exit the Dragon, Enter the Tiger (1976) 360p MPEG Audio.avi", "DivX")]
        [TestCase("msmpeg4v2, DIV3", "Exit the Dragon, Enter the Tiger (1976) 360p MPEG Audio.avi", "DivX")]
        [TestCase("msmpeg4v3, DIV3", "Exit the Dragon, Enter the Tiger (1976) 360p MPEG Audio.avi", "DivX")]
        [TestCase("vp6, 4", "Top Gear - S12E01 - Lorries - SD TV.flv", "VP6")]
        [TestCase("vp7, VP70", "Sweet Seymour.avi", "VP7")]
        [TestCase("vp8, V_VP8", "Dick.mkv", "VP8")]
        [TestCase("vp9, V_VP9", "Roadkill Ep3x11 - YouTube.webm", "VP9")]
        [TestCase("h264, x264", "Ghost Advent - S04E05 - Stanley Hotel SDTV.avi", "x264")]
        [TestCase("hevc, V_MPEGH/ISO/HEVC", "The BBT S11E12 The Matrimonial Metric 1080p 10bit AMZN WEB-DL", "h265")]
        [TestCase("mpeg4, mp4v-20", "", "")]
        [TestCase("mpeg4, XVID", "American.Chopper.S06E07.Mountain.Creek.Bike.DSR.XviD-KRS", "XviD")]
        [TestCase("rpza, V_QUICKTIME", "Custom", "")]
        [TestCase("mpeg4, FMP4", "", "")]
        [TestCase("mpeg4, MP42", "", "")]
        [TestCase("mpeg4, mp43", "Bubble.Guppies.S01E13.480p.WEB-DL.H.264-BTN-Custom", "")]
        public void should_format_video_format(string videoFormatPack, string sceneName, string expectedFormat)
        {
            var split = videoFormatPack.Split(new string[] { ", " }, System.StringSplitOptions.None);
            var mediaInfoModel = new MediaInfoModel
            {
                VideoFormat = split[0],
                VideoCodecID = split[1]
            };

            MediaInfoFormatter.FormatVideoCodec(mediaInfoModel, sceneName).Should().Be(expectedFormat);
        }

        [TestCase("h264, x264", "Some.Video.S01E01.h264", "x264")] // Force mediainfo tag
        [TestCase("hevc, x265", "Some.Video.S01E01.h265", "x265")] // Force mediainfo tag
        [TestCase("h264, ", "Some.Video.S01E01.x264", "x264")] // Not seen in practice, but honor tag if otherwise unknown
        [TestCase("hevc, ", "Some.Video.S01E01.x265", "x265")] // Not seen in practice, but honor tag if otherwise unknown
        [TestCase("h264, ", "Some.Video.S01E01", "h264")] // Default value
        [TestCase("hevc, ", "Some.Video.S01E01", "h265")] // Default value
        public void should_format_video_format_fallbacks(string videoFormatPack, string sceneName, string expectedFormat)
        {
            var split = videoFormatPack.Split(new string[] { ", " }, System.StringSplitOptions.None);
            var mediaInfoModel = new MediaInfoModel
            {
                VideoFormat = split[0],
                VideoCodecID = split[1]
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

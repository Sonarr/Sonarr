using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.MediaFiles.MediaInfo;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.MediaFiles.MediaInfo.MediaInfoFormatterTests
{
    [TestFixture]
    public class FormatAudioCodecFixture : TestBase
    {
        private static string sceneName = "My.Series.S01E01-Sonarr";

        [TestCase("AC-3", "AC3")]
        [TestCase("E-AC-3", "EAC3")]
        [TestCase("MPEG Audio", "MPEG Audio")]
        [TestCase("DTS", "DTS")]
        public void should_format_audio_format_legacy(string audioFormat, string expectedFormat)
        {
            var mediaInfoModel = new MediaInfoModel
            {
                AudioFormat = audioFormat
            };

            MediaInfoFormatter.FormatAudioCodec(mediaInfoModel, sceneName).Should().Be(expectedFormat);
        }

        [TestCase("MPEG Audio, A_MPEG/L2, , ", "droned.s01e03.swedish.720p.hdtv.x264-prince", "MP2")]
        [TestCase("Vorbis, A_VORBIS, , Xiph.Org libVorbis I 20101101 (Schaufenugget)", "DB Super HDTV", "Vorbis")]
        [TestCase("PCM, 1, , ", "DW DVDRip XviD-idTV", "PCM")] // Dubbed most likely
        [TestCase("TrueHD, A_TRUEHD, , ", "", "TrueHD")]
        [TestCase("WMA, 161, , ", "Droned.wmv", "WMA")]
        [TestCase("WMA, 162, Pro, ", "B.N.S04E18.720p.WEB-DL", "WMA")]
        [TestCase("Opus, A_OPUS, , ", "Roadkill Ep3x11 - YouTube.webm", "Opus")]
        [TestCase("mp3 , 0, , ", "climbing.mp4", "MP3")]
        public void should_format_audio_format(string audioFormatPack, string sceneName, string expectedFormat)
        {
            var split = audioFormatPack.Split(new string[] { ", " }, System.StringSplitOptions.None);
            var mediaInfoModel = new MediaInfoModel
            {
                AudioFormat = split[0],
                AudioCodecID = split[1],
                AudioProfile = split[2],
                AudioCodecLibrary = split[3]
            };

            MediaInfoFormatter.FormatAudioCodec(mediaInfoModel, sceneName).Should().Be(expectedFormat);
        }

        [Test]
        public void should_return_MP3_for_MPEG_Audio_with_Layer_3_for_the_profile()
        {
            var mediaInfoModel = new MediaInfoModel
            {
                AudioFormat = "MPEG Audio",
                AudioProfile = "Layer 3"
            };

            MediaInfoFormatter.FormatAudioCodec(mediaInfoModel, sceneName).Should().Be("MP3");
        }

        [Test]
        public void should_return_AudioFormat_by_default()
        {
            var mediaInfoModel = new MediaInfoModel
            {
                AudioFormat = "Other Audio Format",
                AudioCodecID = "Other Audio Codec"
            };

            MediaInfoFormatter.FormatAudioCodec(mediaInfoModel, sceneName).Should().Be(mediaInfoModel.AudioFormat);
            ExceptionVerification.ExpectedWarns(1);
        }
    }
}

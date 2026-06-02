using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.MediaFiles.MediaInfo;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.MediaFiles.MediaInfo.MediaInfoFormatterTests
{
    [TestFixture]
    public class FormatAudioCodecFixture : TestBase
    {
        private const string SceneName = "My.Series.S01E01-Sonarr";

        [TestCase(new[] { "mp2", "", "" }, "droned.s01e03.swedish.720p.hdtv.x264-prince", "MP2")]
        [TestCase(new[] { "vorbis", "", "" }, "DB Super HDTV", "Vorbis")]
        [TestCase(new[] { "pcm_s16le", "", "" }, "DW DVDRip XviD-idTV, ", "PCM")]
        [TestCase(new[] { "truehd", "", "" }, "", "TrueHD")]
        [TestCase(new[] { "truehd", "", "" }, "TrueHD", "TrueHD")]
        [TestCase(new[] { "truehd", "thd+", "" }, "Atmos", "TrueHD Atmos")]
        [TestCase(new[] { "truehd", "thd+", "" }, "TrueHD.Atmos.7.1", "TrueHD Atmos")]
        [TestCase(new[] { "truehd", "thd+", "" }, "", "TrueHD Atmos")]
        [TestCase(new[] { "truehd", "", "Dolby TrueHD + Dolby Atmos" }, "Atmos", "TrueHD Atmos")]
        [TestCase(new[] { "truehd", "", "Dolby TrueHD + Dolby Atmos" }, "TrueHD.Atmos.7.1", "TrueHD Atmos")]
        [TestCase(new[] { "truehd", "", "Dolby TrueHD + Dolby Atmos" }, "", "TrueHD Atmos")]
        [TestCase(new[] { "wmav1", "", "" }, "Droned.wmv", "WMA")]
        [TestCase(new[] { "wmav2", "", "" }, "B.N.S04E18.720p.WEB-DL", "WMA")]
        [TestCase(new[] { "opus", "", "" }, "Roadkill Ep3x11 - YouTube.webm", "Opus")]
        [TestCase(new[] { "mp3", "", "" }, "climbing.mp4", "MP3")]
        [TestCase(new[] { "dts", "", "DTS-HD MA" }, "DTS-HD.MA", "DTS-HD MA")]
        [TestCase(new[] { "dts", "", "DTS:X" }, "DTS-X", "DTS-X")]
        [TestCase(new[] { "dts", "", "DTS-HD MA + DTS:X IMAX" }, "DTS-X", "DTS-X")]
        [TestCase(new[] { "dts", "", "DTS-HD MA" }, "DTS-HD.MA", "DTS-HD MA")]
        [TestCase(new[] { "dts", "", "DTS-ES" }, "DTS-ES", "DTS-ES")]
        [TestCase(new[] { "dts", "", "DTS-ES" }, "DTS", "DTS-ES")]
        [TestCase(new[] { "dts", "", "DTS-HD HRA" }, "DTSHD-HRA", "DTS-HD HRA")]
        [TestCase(new[] { "dts", "", "DTS" }, "DTS", "DTS")]
        [TestCase(new[] { "eac3", "ec+3", "" }, "EAC3.Atmos", "EAC3 Atmos")]
        [TestCase(new[] { "eac3", "", "Dolby Digital Plus + Dolby Atmos" }, "EAC3.Atmos", "EAC3 Atmos")]
        [TestCase(new[] { "eac3", "", "" }, "DDP5.1", "EAC3")]
        [TestCase(new[] { "ac3", "", "" }, "DD5.1", "AC3")]
        [TestCase(new[] { "aac", "", "" }, "AAC2.0", "AAC")]
        [TestCase(new[] { "aac", "", "HE-AAC" }, "AAC2.0", "HE-AAC")]
        [TestCase(new[] { "aac", "", "xHE-AAC" }, "AAC2.0", "xHE-AAC")]
        [TestCase(new[] { "adpcm_ima_qt", "", "" }, "Custom?", "PCM")]
        [TestCase(new[] { "adpcm_ms", "", "" }, "Custom", "PCM")]
        public void should_format_audio_format(string[] audioFormat, string sceneName, string expectedFormat)
        {
            var audioStreamModel = new MediaInfoAudioStreamModel
            {
                Format = audioFormat[0],
                CodecId = audioFormat[1],
                Profile = audioFormat[2],
            };

            MediaInfoFormatter.FormatAudioCodec(audioStreamModel, sceneName).Should().Be(expectedFormat);
        }

        [Test]
        public void should_return_audio_format_by_default()
        {
            var audioStreamModel = new MediaInfoAudioStreamModel
            {
                Format = "Other Audio Format",
                CodecId = "Other Audio Codec",
            };

            MediaInfoFormatter.FormatAudioCodec(audioStreamModel, SceneName).Should().Be(audioStreamModel.Format);
        }

        [Test]
        public void should_return_null_if_audio_stream_is_null()
        {
            MediaInfoFormatter.FormatAudioCodec(null, SceneName).Should().Be(string.Empty);
        }
    }
}

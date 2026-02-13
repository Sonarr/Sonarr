using System;
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

        [TestCase("mp2, ,  ", "droned.s01e03.swedish.720p.hdtv.x264-prince", "MP2")]
        [TestCase("vorbis, ,  ", "DB Super HDTV", "Vorbis")]
        [TestCase("pcm_s16le, ,  ", "DW DVDRip XviD-idTV, ", "PCM")]
        [TestCase("truehd, ,  ", "", "TrueHD")]
        [TestCase("truehd, ,  ", "TrueHD", "TrueHD")]
        [TestCase("truehd, thd+,  ", "Atmos", "TrueHD Atmos")]
        [TestCase("truehd, thd+,  ", "TrueHD.Atmos.7.1", "TrueHD Atmos")]
        [TestCase("truehd, thd+,  ", "", "TrueHD Atmos")]
        [TestCase("truehd, , Dolby TrueHD + Dolby Atmos", "Atmos", "TrueHD Atmos")]
        [TestCase("truehd, , Dolby TrueHD + Dolby Atmos", "TrueHD.Atmos.7.1", "TrueHD Atmos")]
        [TestCase("truehd, , Dolby TrueHD + Dolby Atmos", "", "TrueHD Atmos")]
        [TestCase("wmav1, , ", "Droned.wmv", "WMA")]
        [TestCase("wmav2, , ", "B.N.S04E18.720p.WEB-DL", "WMA")]
        [TestCase("opus, ,  ", "Roadkill Ep3x11 - YouTube.webm", "Opus")]
        [TestCase("mp3, ,  ", "climbing.mp4", "MP3")]
        [TestCase("dts, , DTS-HD MA", "DTS-HD.MA", "DTS-HD MA")]
        [TestCase("dts, , DTS:X", "DTS-X", "DTS-X")]
        [TestCase("dts, , DTS-HD MA + DTS:X IMAX", "DTS-X", "DTS-X")]
        [TestCase("dts, , DTS-HD MA", "DTS-HD.MA", "DTS-HD MA")]
        [TestCase("dts, , DTS-ES", "DTS-ES", "DTS-ES")]
        [TestCase("dts, , DTS-ES", "DTS", "DTS-ES")]
        [TestCase("dts, , DTS-HD HRA", "DTSHD-HRA", "DTS-HD HRA")]
        [TestCase("dts, , DTS", "DTS", "DTS")]
        [TestCase("eac3, ec+3,  ", "EAC3.Atmos", "EAC3 Atmos")]
        [TestCase("eac3, , Dolby Digital Plus + Dolby Atmos", "EAC3.Atmos", "EAC3 Atmos")]
        [TestCase("eac3, ,  ", "DDP5.1", "EAC3")]
        [TestCase("ac3, ,  ", "DD5.1", "AC3")]
        [TestCase("aac, ,  ", "AAC2.0", "AAC")]
        [TestCase("aac, , HE-AAC", "AAC2.0", "HE-AAC")]
        [TestCase("aac, , xHE-AAC", "AAC2.0", "xHE-AAC")]
        [TestCase("adpcm_ima_qt, ,  ", "Custom?", "PCM")]
        [TestCase("adpcm_ms, ,  ", "Custom", "PCM")]
        public void should_format_audio_format(string audioFormatPack, string sceneName, string expectedFormat)
        {
            var split = audioFormatPack.Split(',', StringSplitOptions.TrimEntries);
            var audioStreamModel = new MediaInfoAudioStreamModel
            {
                Format = split[0],
                CodecId = split[1],
                Profile = split[2],
            };

            MediaInfoFormatter.FormatAudioCodec(audioStreamModel, sceneName).Should().Be(expectedFormat);
        }

        [Test]
        public void should_return_AudioFormat_by_default()
        {
            var audioStreamModel = new MediaInfoAudioStreamModel
            {
                Format = "Other Audio Format",
                CodecId = "Other Audio Codec",
            };

            MediaInfoFormatter.FormatAudioCodec(audioStreamModel, SceneName).Should().Be(audioStreamModel.Format);
        }
    }
}

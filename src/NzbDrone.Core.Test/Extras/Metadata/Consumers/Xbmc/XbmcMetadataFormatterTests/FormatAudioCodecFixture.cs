using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Extras.Metadata.Consumers.Xbmc;
using NzbDrone.Core.MediaFiles.MediaInfo;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.Extras.Metadata.Consumers.Xbmc.XbmcMetadataFormatterTests
{
    [TestFixture]
    public class FormatAudioCodecFixture : TestBase
    {
        [TestCase(new[] { "dts", "DTS-HD MA" }, "dtshd_ma")]
        [TestCase(new[] { "dts", "DTS" }, "dts")]
        [TestCase(new[] { "truehd", "Dolby TrueHD + Dolby Atmos" }, "truehd_atmos")]
        [TestCase(new[] { "truehd", "" }, "truehd")]
        [TestCase(new[] { "eac3", "Dolby Digital Plus + Dolby Atmos" }, "eac3_ddp_atmos")]
        [TestCase(new[] { "eac3", "" }, "eac3")]
        [TestCase(new[] { "aac", "" }, "aac")]
        [TestCase(new[] { "aac", "HE-AAC" }, "he_aac")]
        public void should_format_audio_format(string[] audioFormat, string expectedFormat)
        {
            var mediaInfo = new MediaInfoAudioStreamModel
            {
                Format = audioFormat[0],
                Profile = audioFormat[1]
            };

            XbmcMetadataFormatter.FormatAudioCodec(mediaInfo).Should().Be(expectedFormat);
        }

        [Test]
        public void should_return_audio_format_by_default()
        {
            var mediaInfo = new MediaInfoAudioStreamModel
            {
                Format = "Other Audio Format",
            };

            XbmcMetadataFormatter.FormatAudioCodec(mediaInfo).Should().Be(mediaInfo.Format);
        }

        [Test]
        public void should_return_empty_if_audio_stream_is_null()
        {
            XbmcMetadataFormatter.FormatAudioCodec(null).Should().Be(string.Empty);
        }
    }
}

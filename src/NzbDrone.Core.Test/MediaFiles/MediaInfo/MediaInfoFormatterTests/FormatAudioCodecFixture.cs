using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.MediaFiles.MediaInfo;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.MediaFiles.MediaInfo.MediaInfoFormatterTests
{
    [TestFixture]
    public class FormatAudioCodecFixture : TestBase
    {
        [TestCase("AC-3", "AC3")]
        [TestCase("E-AC-3", "EAC3")]
        [TestCase("MPEG Audio", "MPEG Audio")]
        [TestCase("DTS", "DTS")]
        public void should_format_audio_format(string audioFormat, string expectedFormat)
        {
            var mediaInfoModel = new MediaInfoModel
            {
                AudioFormat = audioFormat
            };

            MediaInfoFormatter.FormatAudioCodec(mediaInfoModel).Should().Be(expectedFormat);
        }

        [Test]
        public void should_return_MP3_for_MPEG_Audio_with_Layer_3_for_the_profile()
        {
            var mediaInfoModel = new MediaInfoModel
            {
                AudioFormat = "MPEG Audio",
                AudioProfile = "Layer 3"
            };

            MediaInfoFormatter.FormatAudioCodec(mediaInfoModel).Should().Be("MP3");
        }

        [Test]
        public void should_return_AudioFormat_by_default()
        {
            var mediaInfoModel = new MediaInfoModel
            {
                AudioFormat = "Other Audio Format"
            };

            MediaInfoFormatter.FormatAudioCodec(mediaInfoModel).Should().Be(mediaInfoModel.AudioFormat);
            ExceptionVerification.ExpectedErrors(1);
        }
    }
}

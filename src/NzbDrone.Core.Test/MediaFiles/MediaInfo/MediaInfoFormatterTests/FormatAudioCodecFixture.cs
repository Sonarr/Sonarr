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
        public void should_format_audio_format(string audioFormat, string expectedFormat)
        {
            var mediaInfoModel = new MediaInfoModel
            {
                AudioFormat = audioFormat
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
                AudioFormat = "Other Audio Format"
            };

            MediaInfoFormatter.FormatAudioCodec(mediaInfoModel, sceneName).Should().Be(mediaInfoModel.AudioFormat);
            ExceptionVerification.ExpectedErrors(1);
        }
    }
}

using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.MediaFiles.MediaInfo;

namespace NzbDrone.Core.Test.MediaFiles.MediaInfo.MediaInfoFormatterTests
{
    [TestFixture]
    public class FormatAudioCodecFixture
    {
        [Test]
        public void should_return_AC3_for_AC_3()
        {
            var mediaInfoModel = new MediaInfoModel
            {
                AudioFormat = "AC-3"
            };

            MediaInfoFormatter.FormatAudioCodec(mediaInfoModel).Should().Be("AC3");
        }

        [Test]
        public void should_return_EAC3_for_E_AC_3()
        {
            var mediaInfoModel = new MediaInfoModel
            {
                AudioFormat = "E-AC-3"
            };

            MediaInfoFormatter.FormatAudioCodec(mediaInfoModel).Should().Be("EAC3");
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
        public void should_return_MPEG_Audio_for_MPEG_Audio_without_Layer_3_for_the_profile()
        {
            var mediaInfoModel = new MediaInfoModel
            {
                AudioFormat = "MPEG Audio"
            };

            MediaInfoFormatter.FormatAudioCodec(mediaInfoModel).Should().Be("MPEG Audio");
        }

        [Test]
        public void should_return_DTS_for_DTS()
        {
            var mediaInfoModel = new MediaInfoModel
            {
                AudioFormat = "DTS"
            };

            MediaInfoFormatter.FormatAudioCodec(mediaInfoModel).Should().Be("DTS");
        }

        [Test]
        public void should_default_to_AudioFormat()
        {
            var mediaInfoModel = new MediaInfoModel
            {
                AudioFormat = "Other Audio Format"
            };

            MediaInfoFormatter.FormatAudioCodec(mediaInfoModel).Should().Be(mediaInfoModel.AudioFormat);
        }
    }
}

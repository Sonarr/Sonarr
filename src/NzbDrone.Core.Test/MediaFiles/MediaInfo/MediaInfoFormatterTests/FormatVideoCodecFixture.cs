using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.MediaFiles.MediaInfo;

namespace NzbDrone.Core.Test.MediaFiles.MediaInfo.MediaInfoFormatterTests
{
    [TestFixture]
    public class FormatVideoCodecFixture
    {
        [Test]
        public void should_default_to_x264_if_codec_is_AVC()
        {
            var mediaInfoModel = new MediaInfoModel
            {
                VideoCodec = "AVC"
            };

            MediaInfoFormatter.FormatVideoCodec(mediaInfoModel, null).Should().Be("x264");
        }

        [Test]
        public void should_return_to_x264_if_codec_is_AVC_and_source_title_does_not_contain_h264()
        {
            var mediaInfoModel = new MediaInfoModel
            {
                VideoCodec = "AVC"
            };

            MediaInfoFormatter.FormatVideoCodec(mediaInfoModel, "source.title.x264.720p-Sonarr").Should().Be("x264");
        }

        [Test]
        public void should_return_to_h264_if_codec_is_AVC_and_source_title_contains_h264()
        {
            var mediaInfoModel = new MediaInfoModel
            {
                VideoCodec = "AVC"
            };

            MediaInfoFormatter.FormatVideoCodec(mediaInfoModel, "source.title.h264.720p-Sonarr").Should().Be("h264");
        }

        [Test]
        public void should_default_to_x265_if_codec_is_HEVC()
        {
            var mediaInfoModel = new MediaInfoModel
            {
                VideoCodec = "V_MPEGH/ISO/HEVC"
            };

            MediaInfoFormatter.FormatVideoCodec(mediaInfoModel, null).Should().Be("x265");
        }

        [Test]
        public void should_return_to_x265_if_codec_is_HEVC_and_source_title_does_not_contain_h265()
        {
            var mediaInfoModel = new MediaInfoModel
            {
                VideoCodec = "V_MPEGH/ISO/HEVC"
            };

            MediaInfoFormatter.FormatVideoCodec(mediaInfoModel, "source.title.x265.720p-Sonarr").Should().Be("x265");
        }

        [Test]
        public void should_return_to_h265_if_codec_is_HEVC_and_source_title_contains_h265()
        {
            var mediaInfoModel = new MediaInfoModel
            {
                VideoCodec = "V_MPEGH/ISO/HEVC"
            };

            MediaInfoFormatter.FormatVideoCodec(mediaInfoModel, "source.title.h265.720p-Sonarr").Should().Be("h265");
        }

        [Test]
        public void should_return_to_MPEG2_if_video_codec_is_MPEG_2_Video()
        {
            var mediaInfoModel = new MediaInfoModel
            {
                VideoCodec = "MPEG-2 Video"
            };

            MediaInfoFormatter.FormatVideoCodec(mediaInfoModel, null).Should().Be("MPEG2");
        }

        [Test]
        public void should_return_to_video_codec_by_default()
        {
            var mediaInfoModel = new MediaInfoModel
            {
                VideoCodec = "VideoCodec"
            };

            MediaInfoFormatter.FormatVideoCodec(mediaInfoModel, null).Should().Be(mediaInfoModel.VideoCodec);
        }
    }
}

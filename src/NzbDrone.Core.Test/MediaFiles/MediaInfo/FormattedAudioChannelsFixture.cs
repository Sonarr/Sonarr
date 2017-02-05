using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.MediaFiles.MediaInfo;

namespace NzbDrone.Core.Test.MediaFiles.MediaInfo
{
    [TestFixture]
    public class FormattedAudioChannelsFixture
    {
        [Test]
        public void should_subtract_one_from_AudioChannels_as_total_channels_if_LFE_in_AudioChannelPositionsText()
        {
            var mediaInfoModel = new MediaInfoModel
            {
                AudioChannels = 6,
                AudioChannelPositions = null,
                AudioChannelPositionsText = "Front: L C R, Side: L R, LFE"
            };

            mediaInfoModel.FormattedAudioChannels.Should().Be(5.1m);
        }

        [Test]
        public void should_use_AudioChannels_as_total_channels_if_LFE_not_in_AudioChannelPositionsText()
        {
            var mediaInfoModel = new MediaInfoModel
            {
                AudioChannels = 2,
                AudioChannelPositions = null,
                AudioChannelPositionsText = "Front: L R"
            };

            mediaInfoModel.FormattedAudioChannels.Should().Be(2);
        }

        [Test]
        public void should_return_0_if_schema_revision_is_less_than_3_and_other_properties_are_null()
        {
            var mediaInfoModel = new MediaInfoModel
            {
                AudioChannels = 2,
                AudioChannelPositions = null,
                AudioChannelPositionsText = null,
                SchemaRevision = 2
            };

            mediaInfoModel.FormattedAudioChannels.Should().Be(0);
        }

        [Test]
        public void should_use_AudioChannels_if_schema_revision_is_3_and_other_properties_are_null()
        {
            var mediaInfoModel = new MediaInfoModel
            {
                AudioChannels = 2,
                AudioChannelPositions = null,
                AudioChannelPositionsText = null,
                SchemaRevision = 3
            };

            mediaInfoModel.FormattedAudioChannels.Should().Be(2);
        }

        [Test]
        public void should_sum_AudioChannelPositions()
        {
            var mediaInfoModel = new MediaInfoModel
            {
                AudioChannels = 2,
                AudioChannelPositions = "2/0/0",
                AudioChannelPositionsText = null,
                SchemaRevision = 3
            };

            mediaInfoModel.FormattedAudioChannels.Should().Be(2);
        }

        [Test]
        public void should_sum_AudioChannelPositions_including_decimal()
        {
            var mediaInfoModel = new MediaInfoModel
            {
                AudioChannels = 2,
                AudioChannelPositions = "3/2/0.1",
                AudioChannelPositionsText = null,
                SchemaRevision = 3
            };

            mediaInfoModel.FormattedAudioChannels.Should().Be(5.1m);
        }

        [Test]
        public void should_cleanup_extraneous_text_from_AudioChannelPositions()
        {
            var mediaInfoModel = new MediaInfoModel
            {
                AudioChannels = 2,
                AudioChannelPositions = "Object Based / 3/2/2.1",
                AudioChannelPositionsText = null,
                SchemaRevision = 3
            };

            mediaInfoModel.FormattedAudioChannels.Should().Be(7.1m);
        }

        [Test]
        public void should_sum_first_series_of_numbers_from_AudioChannelPositions()
        {
            var mediaInfoModel = new MediaInfoModel
            {
                AudioChannels = 2,
                AudioChannelPositions = "3/2/2.1 / 3/2/2.1",
                AudioChannelPositionsText = null,
                SchemaRevision = 3
            };

            mediaInfoModel.FormattedAudioChannels.Should().Be(7.1m);
        }
    }
}

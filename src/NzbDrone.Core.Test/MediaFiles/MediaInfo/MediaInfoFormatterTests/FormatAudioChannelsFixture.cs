using System.Globalization;
using System.Threading;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.MediaFiles.MediaInfo;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.MediaFiles.MediaInfo.MediaInfoFormatterTests
{
    [TestFixture]
    public class FormatAudioChannelsFixture : TestBase
    {
        [Test]
        public void should_subtract_one_from_AudioChannels_as_total_channels_if_LFE_in_AudioChannelPositionsText()
        {
            var mediaInfoModel = new MediaInfoModel
            {
                AudioChannelsContainer = 6,
                AudioChannelPositions = null,
                AudioChannelPositionsTextContainer = "Front: L C R, Side: L R, LFE"
            };

            MediaInfoFormatter.FormatAudioChannels(mediaInfoModel).Should().Be(5.1m);
        }

        [Test]
        public void should_use_AudioChannels_as_total_channels_if_LFE_not_in_AudioChannelPositionsText()
        {
            var mediaInfoModel = new MediaInfoModel
            {
                AudioChannelsContainer = 2,
                AudioChannelPositions = null,
                AudioChannelPositionsTextContainer = "Front: L R"
            };

            MediaInfoFormatter.FormatAudioChannels(mediaInfoModel).Should().Be(2);
        }

        [Test]
        public void should_return_0_if_schema_revision_is_less_than_3_and_other_properties_are_null()
        {
            var mediaInfoModel = new MediaInfoModel
            {
                AudioChannelsContainer = 2,
                AudioChannelPositions = null,
                AudioChannelPositionsTextContainer = null,
                SchemaRevision = 2
            };

            MediaInfoFormatter.FormatAudioChannels(mediaInfoModel).Should().Be(0);
        }

        [Test]
        public void should_use_AudioChannels_if_schema_revision_is_3_and_other_properties_are_null()
        {
            var mediaInfoModel = new MediaInfoModel
            {
                AudioChannelsContainer = 2,
                AudioChannelPositions = null,
                AudioChannelPositionsTextContainer = null,
                SchemaRevision = 3
            };

            MediaInfoFormatter.FormatAudioChannels(mediaInfoModel).Should().Be(2);
        }

        [Test]
        public void should_sum_AudioChannelPositions()
        {
            var mediaInfoModel = new MediaInfoModel
            {
                AudioChannelsContainer = 2,
                AudioChannelPositions = "2/0/0",
                AudioChannelPositionsTextContainer = null,
                SchemaRevision = 3
            };

            MediaInfoFormatter.FormatAudioChannels(mediaInfoModel).Should().Be(2);
        }

        [Test]
        public void should_sum_AudioChannelPositions_including_decimal()
        {
            var mediaInfoModel = new MediaInfoModel
            {
                AudioChannelsContainer = 2,
                AudioChannelPositions = "3/2/0.1",
                AudioChannelPositionsTextContainer = null,
                SchemaRevision = 3
            };

            MediaInfoFormatter.FormatAudioChannels(mediaInfoModel).Should().Be(5.1m);
        }

        [Test]
        public void should_format_8_channel_object_based_as_71_if_dtsx()
        {
            var mediaInfoModel = new MediaInfoModel
            {
                AudioChannelsContainer = 8,
                AudioChannelsStream = 0,
                AudioFormat = "DTS",
                AudioAdditionalFeatures = "XLL X",
                AudioChannelPositions = "Object Based",
                AudioChannelPositionsTextContainer = "Object Based",
                AudioChannelPositionsTextStream = "Object Based",
                SchemaRevision = 3
            };

            MediaInfoFormatter.FormatAudioChannels(mediaInfoModel).Should().Be(7.1m);
        }

        [Test]
        public void should_format_8_channel_blank_as_71_if_dtsx()
        {
            var mediaInfoModel = new MediaInfoModel
            {
                AudioChannelsContainer = 8,
                AudioChannelsStream = 0,
                AudioFormat = "DTS",
                AudioAdditionalFeatures = "XLL X",
                AudioChannelPositions = "",
                AudioChannelPositionsTextContainer = "",
                AudioChannelPositionsTextStream = "Object Based",
                SchemaRevision = 3
            };

            MediaInfoFormatter.FormatAudioChannels(mediaInfoModel).Should().Be(7.1m);
        }

        [Test]
        public void should_ignore_culture_on_channel_summary()
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("fr-FR");

            var mediaInfoModel = new MediaInfoModel
            {
                AudioChannelsContainer = 2,
                AudioChannelPositions = "3/2/0.1",
                AudioChannelPositionsTextContainer = null,
                SchemaRevision = 3
            };

            MediaInfoFormatter.FormatAudioChannels(mediaInfoModel).Should().Be(5.1m);
        }

        [Test]
        public void should_handle_AudioChannelPositions_three_digits()
        {
            var mediaInfoModel = new MediaInfoModel
            {
                AudioChannelsContainer = 2,
                AudioChannelPositions = "3/2/0.2.1",
                AudioChannelPositionsTextContainer = null,
                SchemaRevision = 3
            };

            MediaInfoFormatter.FormatAudioChannels(mediaInfoModel).Should().Be(7.1m);
        }

        [Test]
        public void should_cleanup_extraneous_text_from_AudioChannelPositions()
        {
            var mediaInfoModel = new MediaInfoModel
            {
                AudioChannelsContainer = 2,
                AudioChannelPositions = "Object Based / 3/2/2.1",
                AudioChannelPositionsTextContainer = null,
                SchemaRevision = 3
            };

            MediaInfoFormatter.FormatAudioChannels(mediaInfoModel).Should().Be(7.1m);
        }

        [Test]
        public void should_skip_empty_groups_in_AudioChannelPositions()
        {
            var mediaInfoModel = new MediaInfoModel
            {
                AudioChannelsContainer = 2,
                AudioChannelPositions = " / 2/0/0.0",
                AudioChannelPositionsTextContainer = null,
                SchemaRevision = 3
            };

            MediaInfoFormatter.FormatAudioChannels(mediaInfoModel).Should().Be(2);
        }

        [Test]
        public void should_sum_first_series_of_numbers_from_AudioChannelPositions()
        {
            var mediaInfoModel = new MediaInfoModel
            {
                AudioChannelsContainer = 2,
                AudioChannelPositions = "3/2/2.1 / 3/2/2.1",
                AudioChannelPositionsTextContainer = null,
                SchemaRevision = 3
            };

            MediaInfoFormatter.FormatAudioChannels(mediaInfoModel).Should().Be(7.1m);
        }

        [Test]
        public void should_sum_first_series_of_numbers_from_AudioChannelPositions_with_three_digits()
        {
            var mediaInfoModel = new MediaInfoModel
            {
                AudioChannelsContainer = 2,
                AudioChannelPositions = "3/2/0.2.1 / 3/2/0.1",
                AudioChannelPositionsTextContainer = null,
                SchemaRevision = 3
            };

            MediaInfoFormatter.FormatAudioChannels(mediaInfoModel).Should().Be(7.1m);
        }

        [Test]
        public void should_sum_dual_mono_representation_AudioChannelPositions()
        {
            var mediaInfoModel = new MediaInfoModel
            {
                AudioChannelsContainer = 2,
                AudioChannelPositions = "1+1",
                AudioChannelPositionsTextContainer = null,
                SchemaRevision = 3
            };

            MediaInfoFormatter.FormatAudioChannels(mediaInfoModel).Should().Be(2.0m);
        }

        [Test]
        public void should_use_AudioChannelPositionText_when_AudioChannelChannelPosition_is_invalid()
        {
            var mediaInfoModel = new MediaInfoModel
            {
                AudioChannelsContainer = 6,
                AudioChannelPositions = "15 objects",
                AudioChannelPositionsTextContainer = "15 objects / Front: L C R, Side: L R, LFE",
                SchemaRevision = 3
            };

            MediaInfoFormatter.FormatAudioChannels(mediaInfoModel).Should().Be(5.1m);
        }

        [Test]
        public void should_remove_atmos_objects_from_AudioChannelPostions()
        {
            var mediaInfoModel = new MediaInfoModel
            {
                AudioChannelsContainer = 2,
                AudioChannelPositions = "15 objects / 3/2.1",
                AudioChannelPositionsTextContainer = null,
                SchemaRevision = 3
            };

            MediaInfoFormatter.FormatAudioChannels(mediaInfoModel).Should().Be(5.1m);
        }

        [Test]
        public void should_use_audio_stream_text_when_exists()
        {
            var mediaInfoModel = new MediaInfoModel
            {
                AudioChannelsContainer = 6,
                AudioChannelsStream = 8,
                AudioChannelPositions = null,
                AudioChannelPositionsTextContainer = null,
                AudioChannelPositionsTextStream = "Front: L C R, Side: L R, Back: L R, LFE",
                SchemaRevision = 6
            };

            MediaInfoFormatter.FormatAudioChannels(mediaInfoModel).Should().Be(7.1m);
        }

        [Test]
        public void should_use_audio_stream_channels_when_exists()
        {
            var mediaInfoModel = new MediaInfoModel
            {
                AudioChannelsContainer = 6,
                AudioChannelsStream = 8,
                AudioChannelPositions = null,
                AudioChannelPositionsTextContainer = null,
                AudioChannelPositionsTextStream = null,
                SchemaRevision = 6
            };

            MediaInfoFormatter.FormatAudioChannels(mediaInfoModel).Should().Be(8m);
        }
    }
}

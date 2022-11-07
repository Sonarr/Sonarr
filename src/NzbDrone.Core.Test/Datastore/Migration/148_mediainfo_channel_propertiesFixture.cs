using System;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Datastore.Migration;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Datastore.Migration
{
    [TestFixture]
    public class mediainfo_channel_propertiesFixture : MigrationTest<mediainfo_channels>
    {
        private void AddEpisodeFile(mediainfo_channels m, int id)
        {
            var episode = new
            {
                Id = id,
                SeriesId = id,
                Quality = new { }.ToJson(),
                Size = 0,
                DateAdded = DateTime.UtcNow,
                RelativePath = "SomeFile.mkv",
                Language = 1,
                SeasonNumber = 1,
                MediaInfo = new
                {
                    ContainerFormat = "Matroska",
                    VideoFormat = "AVC",
                    VideoCodecID = "V_MPEG4/ISO/AVC",
                    VideoProfile = "High@L4.1",
                    VideoCodecLibrary = "x264 - core 155 r2867+74 66b5600 t_mod_Custom_2 [8-bit@all X86_64]",
                    VideoBitrate = 6243046,
                    VideoBitDepth = 8,
                    VideoMultiViewCount = 0,
                    VideoColourPrimaries = "BT.709",
                    VideoTransferCharacteristics = "BT.709",
                    Width = 1280,
                    Height = 692,
                    AudioFormat = "AC-3",
                    AudioCodecID = "A_AC3",
                    AudioCodecLibrary = "",
                    AudioAdditionalFeatures = "",
                    AudioBitrate = 640000,
                    RunTime = "01:36:47.8720000",
                    AudioStreamCount = 3,
                    AudioChannels = 6,
                    AudioChannelPositions = "3/2/0.1",
                    AudioChannelPositionsText = "Front: L C R, Side: L R, LFE",
                    AudioProfile = "",
                    VideoFps = 23.976,
                    AudioLanguages = "Czech / Slovak / English",
                    Subtitles = "Czech / Czech",
                    ScanType = "Progressive",
                    SchemaRevision = 5
                }.ToJson()
            };

            m.Insert.IntoTable("EpisodeFiles").Row(episode);
        }

        [Test]
        public void should_change_property_names_for_audio_channels()
        {
            var db = WithMigrationTestDb(c =>
            {
                AddEpisodeFile(c, 1);
            });

            var items = db.Query<EpisodeFile148>("SELECT MediaInfo FROM EpisodeFiles");

            items.Should().HaveCount(1);

            var mediainfo = items.First().MediaInfo;

            // Removed props should be null
            mediainfo.AudioChannels.Should().BeNull();
            mediainfo.AudioChannelPositionsText.Should().BeNull();

            // Renamed should have original value
            mediainfo.AudioChannelsContainer.Should().NotBeNull();
            mediainfo.AudioChannelPositionsTextContainer.Should().NotBeNull();
            mediainfo.AudioChannelsContainer.Should().Be(6);
            mediainfo.AudioChannelPositionsTextContainer.Should().Be("Front: L C R, Side: L R, LFE");

            // Should not touch other props
            mediainfo.AudioChannelPositions.Should().Be("3/2/0.1");
        }

        public class EpisodeFile148
        {
            public int Id { get; set; }
            public MediaInfo148 MediaInfo { get; set; }
        }

        public class MediaInfo148
        {
            public int? AudioChannels { get; set; }
            public int? AudioChannelsContainer { get; set; }
            public string AudioChannelPositionsText { get; set; }
            public string AudioChannelPositionsTextContainer { get; set; }
            public string AudioChannelPositions { get; set; }
        }
    }
}

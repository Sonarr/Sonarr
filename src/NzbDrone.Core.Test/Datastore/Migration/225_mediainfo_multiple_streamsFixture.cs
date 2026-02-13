using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Datastore.Migration;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.Datastore.Migration;

[TestFixture]
public class mediainfo_multiple_streamsFixture : MigrationTest<mediainfo_multiple_streams>
{
    [Test]
    public void should_convert_empty_media_info()
    {
        var db = WithMigrationTestDb(c =>
        {
            c.Insert.IntoTable("EpisodeFiles").Row(new
            {
                SeriesId = 1,
                SeasonNumber = 1,
                RelativePath = "Season 01/S01E05.mkv",
                Size = 125.Megabytes(),
                DateAdded = DateTime.UtcNow,
                OriginalFilePath = "Series.Title.S01E05.720p.HDTV.x265-Sonarr.mkv",
                ReleaseGroup = "Sonarr",
                Quality = new QualityModel(Quality.HDTV720p).ToJson(),
                Languages = "[1]",
            });
        });

        var items = db.Query<EpisodeFile225>("SELECT \"Id\", \"MediaInfo\" FROM \"EpisodeFiles\"");

        items.Should().HaveCount(1);
        items.First().MediaInfo.Should().BeNull();
    }

    [Test]
    public void should_convert_non_empty_media_info()
    {
        var db = WithMigrationTestDb(c =>
        {
            c.Insert.IntoTable("EpisodeFiles").Row(new
            {
                SeriesId = 1,
                SeasonNumber = 1,
                RelativePath = "Season 01/S01E05.mkv",
                Size = 125.Megabytes(),
                DateAdded = DateTime.UtcNow,
                OriginalFilePath = "Series.Title.S01E05.720p.HDTV.x265-Sonarr.mkv",
                ReleaseGroup = "Sonarr",
                Quality = new QualityModel(Quality.HDTV720p).ToJson(),
                Languages = "[1]",
                MediaInfo = new
                {
                    AudioFormat = "truehd",
                    AudioCodecID = "[0][0][0][0]",
                    AudioProfile = "Dolby TrueHD + Dolby Atmos",
                    AudioBitrate = 224000,
                    AudioChannels = 2,
                    AudioChannelPositions = "stereo",
                    AudioLanguages = new List<string> { "eng", "rum" },
                    Subtitles = new List<string> { "ger", "eng", "rum" },
                    ScanType = "Progressive",
                    SchemaRevision = 13
                }.ToJson()
            });
        });

        var items = db.Query<EpisodeFile225>("SELECT \"Id\", \"RelativePath\", \"MediaInfo\" FROM \"EpisodeFiles\"");

        items.Should().HaveCount(1);

        var mediainfo = items.First().MediaInfo;

        mediainfo.AudioFormat.Should().BeNull();
        mediainfo.AudioCodecID.Should().BeNull();
        mediainfo.AudioProfile.Should().BeNull();
        mediainfo.AudioBitrate.Should().BeNull();
        mediainfo.AudioChannels.Should().BeNull();
        mediainfo.AudioChannelPositions.Should().BeNull();

        mediainfo.AudioStreams.First().Format.Should().Be("truehd");
        mediainfo.AudioStreams.First().CodecId.Should().Be("[0][0][0][0]");
        mediainfo.AudioStreams.First().Profile.Should().Be("Dolby TrueHD + Dolby Atmos");
        mediainfo.AudioStreams.First().Bitrate.Should().Be(224000);
        mediainfo.AudioStreams.First().Channels.Should().Be(2);
        mediainfo.AudioStreams.First().ChannelPositions.Should().Be("stereo");
        mediainfo.AudioStreams.First().Language.Should().Be("eng");

        mediainfo.AudioStreams.Select(s => s.Language).Should().BeEquivalentTo("eng", "rum");
        mediainfo.SubtitleStreams.Select(s => s.Language).Should().BeEquivalentTo("eng", "ger", "rum");
    }

    [Test]
    public void should_convert_to_null_on_invalid_media_info()
    {
        var db = WithMigrationTestDb(c =>
        {
            c.Insert.IntoTable("EpisodeFiles").Row(new
            {
                SeriesId = 1,
                SeasonNumber = 1,
                RelativePath = "Season 01/S01E05.mkv",
                Size = 125.Megabytes(),
                DateAdded = DateTime.UtcNow,
                OriginalFilePath = "Series.Title.S01E05.720p.HDTV.x265-Sonarr.mkv",
                ReleaseGroup = "Sonarr",
                Quality = new QualityModel(Quality.HDTV720p).ToJson(),
                Languages = "[1]",
                MediaInfo = new
                {
                    AudioLanguages = "eng/rum",
                    Subtitles = "ger/eng/rum",
                    SchemaRevision = 13
                }.ToJson()
            });
        });

        var items = db.Query<EpisodeFile225>("SELECT \"Id\", \"MediaInfo\" FROM \"EpisodeFiles\"");

        items.Should().HaveCount(1);
        items.First().MediaInfo.Should().BeNull();

        ExceptionVerification.ExpectedWarns(1);
    }
}

internal class EpisodeFile225
{
    public int Id { get; set; }
    public string RelativePath { get; set; }
    public MediaInfo225 MediaInfo { get; set; }
}

internal class MediaInfo225
{
    public int SchemaRevision { get; set; }
    public string AudioFormat { get; set; }
    public string AudioCodecID { get; set; }
    public string AudioProfile { get; set; }
    public long? AudioBitrate { get; set; }
    public int? AudioChannels { get; set; }
    public string AudioChannelPositions { get; set; }
    public List<MediaInfoAudioStreamModel225> AudioStreams { get; set; }
    public List<MediaInfoSubtitleStreamModel225> SubtitleStreams { get; set; }
}

internal class MediaInfoAudioStreamModel225
{
    public string Language { get; set; }
    public string Format { get; set; }
    public string CodecId { get; set; }
    public string Profile { get; set; }
    public long? Bitrate { get; set; }
    public int? Channels { get; set; }
    public string ChannelPositions { get; set; }
}

public class MediaInfoSubtitleStreamModel225
{
    public string Language { get; set; }
}

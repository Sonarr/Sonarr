using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Dapper;
using FluentMigrator;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Datastore.Migration.Framework;
using NzbDrone.Core.MediaFiles.MediaInfo;

namespace NzbDrone.Core.Datastore.Migration;

[Migration(225)]
public class mediainfo_multiple_streams : NzbDroneMigrationBase
{
    private readonly JsonSerializerOptions _serializerSettings;

    public mediainfo_multiple_streams()
    {
        var serializerSettings = new JsonSerializerOptions
        {
            AllowTrailingCommas = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNameCaseInsensitive = true,
            DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        serializerSettings.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, true));
        serializerSettings.Converters.Add(new STJTimeSpanConverter());
        serializerSettings.Converters.Add(new STJUtcConverter());

        _serializerSettings = serializerSettings;
    }

    protected override void MainDbUpgrade()
    {
        Execute.WithConnection(MigrateMediaInfoToMultipleStreams);
    }

    private void MigrateMediaInfoToMultipleStreams(IDbConnection conn, IDbTransaction tran)
    {
        var existing = conn.Query<EpisodeFile224>("SELECT \"Id\", \"MediaInfo\" FROM \"EpisodeFiles\"");

        var updated = new List<object>();

        foreach (var row in existing)
        {
            if (row.MediaInfo.IsNullOrWhiteSpace())
            {
                continue;
            }

            MediaInfo224 oldMediaInfo;
            try
            {
                oldMediaInfo = JsonSerializer.Deserialize<MediaInfo224>(row.MediaInfo, _serializerSettings);
            }
            catch (JsonException ex)
            {
                _logger.Warn(ex, "Episode {EpisodeId} contains invalid JSON data, skipping.", row.Id);

                updated.Add(new EpisodeFile225 { Id = row.Id, MediaInfo = null });

                continue;
            }

            if (oldMediaInfo.SchemaRevision >= 14)
            {
                continue;
            }

            var newMediaInfo = MigrateMediaInfo(oldMediaInfo);

            if (oldMediaInfo.AudioLanguages == null && oldMediaInfo.Subtitles == null)
            {
                _logger.Warn("Episode {EpisodeId} might be already migrated, skipping.", row.Id);

                continue;
            }

            updated.Add(new EpisodeFile225
            {
                Id = row.Id,
                MediaInfo = JsonSerializer.Serialize(newMediaInfo, _serializerSettings)
            });
        }

        conn.Execute(
            "UPDATE \"EpisodeFiles\" SET \"MediaInfo\" = @MediaInfo WHERE \"Id\" = @Id",
            updated,
            transaction: tran);
    }

    private static MediaInfo225 MigrateMediaInfo(MediaInfo224 old)
    {
        return new MediaInfo225
        {
            RawStreamData = old.RawStreamData,
            SchemaRevision = old.SchemaRevision,
            ContainerFormat = old.ContainerFormat,
            VideoFormat = old.VideoFormat,
            VideoCodecID = old.VideoCodecID,
            VideoProfile = old.VideoProfile,
            VideoBitrate = old.VideoBitrate,
            VideoBitDepth = old.VideoBitDepth,
            VideoColourPrimaries = old.VideoColourPrimaries,
            VideoTransferCharacteristics = old.VideoTransferCharacteristics,
            VideoHdrFormat = old.VideoHdrFormat,
            Height = old.Height,
            Width = old.Width,
            RunTime = old.RunTime,
            VideoFps = old.VideoFps,
            AudioStreams = MigrateAudioStreams(old),
            SubtitleStreams = MigrateSubtitleStreams(old),
            ScanType = old.ScanType
        };
    }

    private static List<MediaInfoAudioStream225> MigrateAudioStreams(MediaInfo224 old)
    {
        var audioStreams = old.AudioLanguages?
            .Select(language => new MediaInfoAudioStream225
            {
                Language = language,
            })
            .ToList();
        audioStreams?.FirstOrDefault()?.Format = old.AudioFormat;
        audioStreams?.FirstOrDefault()?.CodecId = old.AudioCodecID;
        audioStreams?.FirstOrDefault()?.Profile = old.AudioProfile;
        audioStreams?.FirstOrDefault()?.Bitrate = old.AudioBitrate;
        audioStreams?.FirstOrDefault()?.Channels = old.AudioChannels;
        audioStreams?.FirstOrDefault()?.ChannelPositions = old.AudioChannelPositions;

        return audioStreams;
    }

    private static List<MediaInfoSubtitleStream225> MigrateSubtitleStreams(MediaInfo224 old)
    {
        var subtitleStreams = old.Subtitles?
            .Select(language => new MediaInfoSubtitleStream225
            {
                Language = language,
            })
            .ToList();

        return subtitleStreams;
    }

    internal class EpisodeFile224 : ModelBase
    {
        public string MediaInfo { get; set; }
        public string SceneName { get; set; }
    }

    internal class EpisodeFile225 : ModelBase
    {
        public string MediaInfo { get; set; }
    }

    internal class MediaInfo224
    {
        public string RawStreamData { get; set; }
        public int SchemaRevision { get; set; }
        public string ContainerFormat { get; set; }
        public string VideoFormat { get; set; }
        public string VideoCodecID { get; set; }
        public string VideoProfile { get; set; }
        public long VideoBitrate { get; set; }
        public int VideoBitDepth { get; set; }
        public string VideoColourPrimaries { get; set; }
        public string VideoTransferCharacteristics { get; set; }
        public HdrFormat VideoHdrFormat { get; set; }
        public int Height { get; set; }
        public int Width { get; set; }
        public string AudioFormat { get; set; }
        public string AudioCodecID { get; set; }
        public string AudioProfile { get; set; }
        public long AudioBitrate { get; set; }
        public TimeSpan RunTime { get; set; }
        public int AudioStreamCount { get; set; }
        public int AudioChannels { get; set; }
        public string AudioChannelPositions { get; set; }
        public decimal VideoFps { get; set; }
        public List<string> AudioLanguages { get; set; }
        public List<string> Subtitles { get; set; }
        public string ScanType { get; set; }
    }

    internal class MediaInfo225
    {
        public string RawStreamData { get; set; }
        public int SchemaRevision { get; set; }
        public string ContainerFormat { get; set; }
        public string VideoFormat { get; set; }
        public string VideoCodecID { get; set; }
        public string VideoProfile { get; set; }
        public long VideoBitrate { get; set; }
        public int VideoBitDepth { get; set; }
        public string VideoColourPrimaries { get; set; }
        public string VideoTransferCharacteristics { get; set; }
        public HdrFormat VideoHdrFormat { get; set; }
        public int Height { get; set; }
        public int Width { get; set; }
        public TimeSpan RunTime { get; set; }
        public decimal VideoFps { get; set; }
        public List<MediaInfoAudioStream225> AudioStreams { get; set; }
        public List<MediaInfoSubtitleStream225> SubtitleStreams { get; set; }
        public string ScanType { get; set; }
    }

    internal class MediaInfoAudioStream225
    {
        public string Language { get; set; }
        public string Format { get; set; }
        public string CodecId { get; set; }
        public string Profile { get; set; }
        public long Bitrate { get; set; }
        public int Channels { get; set; }
        public string ChannelPositions { get; set; }
    }

    internal class MediaInfoSubtitleStream225
    {
        public string Language { get; set; }
    }
}

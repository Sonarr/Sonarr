using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Dapper;
using FluentMigrator;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Datastore.Migration.Framework;

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
            WriteIndented = false
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
        var lastId = 0;

        while (true)
        {
            var files = conn.Query<EpisodeFile225>(
                    """
                    SELECT "Id", "MediaInfo"
                    FROM "EpisodeFiles"
                    WHERE "MediaInfo" IS NOT NULL
                    AND "Id" > @LastId
                    ORDER BY "Id"
                    LIMIT 1000
                    """,
                    new { LastId = lastId },
                    tran)
                .AsList();

            if (files.Count == 0)
            {
                break;
            }

            var updated = new List<EpisodeFile225>();

            foreach (var file in files.Where(file => file.MediaInfo.IsNotNullOrWhiteSpace()))
            {
                JsonObject mediaInfo;

                try
                {
                    mediaInfo = JsonSerializer.Deserialize<JsonObject>(file.MediaInfo, _serializerSettings);
                }
                catch (JsonException ex)
                {
                    _logger.Warn(ex, "Episode {EpisodeId} contains invalid JSON data, skipping.", file.Id);

                    updated.Add(new EpisodeFile225 { Id = file.Id, MediaInfo = null });

                    continue;
                }

                if (mediaInfo["audioStreams"] is JsonArray && mediaInfo["subtitleStreams"] is JsonArray)
                {
                    _logger.Warn("Episode {EpisodeId} might be already migrated, skipping. Reason: Existing audioStreams and subtitleStreams fields.", file.Id);

                    continue;
                }

                if (mediaInfo["audioLanguages"] == null && mediaInfo["subtitles"] == null)
                {
                    _logger.Warn("Episode {EpisodeId} might be already migrated, skipping. Reason: Missing audioLanguages and subtitles fields.", file.Id);

                    continue;
                }

                if (mediaInfo["audioLanguages"] is not JsonArray || mediaInfo["subtitles"] is not JsonArray)
                {
                    _logger.Warn("Episode {EpisodeId} contains invalid JSON data for audioLanguages and/or subtitles, skipping.", file.Id);

                    updated.Add(new EpisodeFile225 { Id = file.Id, MediaInfo = null });

                    continue;
                }

                mediaInfo["audioStreams"] = MigrateAudioStreams(mediaInfo);
                mediaInfo["subtitleStreams"] = MigrateSubtitleStreams(mediaInfo);

                mediaInfo.Remove("audioLanguages");
                mediaInfo.Remove("audioFormat");
                mediaInfo.Remove("audioCodecID");
                mediaInfo.Remove("audioProfile");
                mediaInfo.Remove("audioBitrate");
                mediaInfo.Remove("audioChannels");
                mediaInfo.Remove("audioChannelPositions");
                mediaInfo.Remove("subtitles");

                updated.Add(new EpisodeFile225
                {
                    Id = file.Id,
                    MediaInfo = JsonSerializer.Serialize(mediaInfo, _serializerSettings)
                });
            }

            UpdateMediaInfoForEpisodeFile(conn, tran, updated);

            lastId = files[^1].Id;
        }
    }

    private static void UpdateMediaInfoForEpisodeFile(IDbConnection conn, IDbTransaction tran, IReadOnlyList<EpisodeFile225> updated)
    {
        conn.Execute("UPDATE \"EpisodeFiles\" SET \"MediaInfo\" = @MediaInfo WHERE \"Id\" = @Id", updated, transaction: tran);
    }

    private static JsonArray MigrateAudioStreams(JsonObject mediaInfo)
    {
        var audioStreams = mediaInfo["audioLanguages"]?
            .AsArray()
            .Select(language => new JsonObject
            {
                ["language"] = language.GetValue<string>(),
            })
            .ToList() ?? [];

        if (audioStreams.Count == 0)
        {
            audioStreams.Add(new JsonObject
            {
                ["language"] = "und"
            });
        }

        audioStreams.FirstOrDefault()?["format"] = mediaInfo["audioFormat"]?.GetValue<string>();
        audioStreams.FirstOrDefault()?["codecId"] = mediaInfo["audioCodecID"]?.GetValue<string>();
        audioStreams.FirstOrDefault()?["profile"] = mediaInfo["audioProfile"]?.GetValue<string>();
        audioStreams.FirstOrDefault()?["bitrate"] = mediaInfo["audioBitrate"]?.GetValue<long>();
        audioStreams.FirstOrDefault()?["channels"] = mediaInfo["audioChannels"]?.GetValue<int>();
        audioStreams.FirstOrDefault()?["channelPositions"] = mediaInfo["audioChannelPositions"]?.GetValue<string>();

        return new JsonArray(audioStreams.ToArray());
    }

    private static JsonArray MigrateSubtitleStreams(JsonObject mediaInfo)
    {
        return new JsonArray(mediaInfo["subtitles"]?
            .AsArray()
            .Select(language => new JsonObject
            {
                ["language"] = language.GetValue<string>()
            })
            .ToArray() ?? []);
    }

    internal class EpisodeFile225 : ModelBase
    {
        public string MediaInfo { get; set; }
    }
}

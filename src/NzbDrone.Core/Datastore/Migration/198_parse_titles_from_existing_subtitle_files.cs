using System.Collections.Generic;
using System.Data;
using System.Text.Json;
using System.Text.Json.Serialization;
using Dapper;
using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.EpisodeImport.Aggregation.Aggregators;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(198)]
    public class parse_title_from_existing_subtitle_files : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("SubtitleFiles").AddColumn("Title").AsString().Nullable();
            Alter.Table("SubtitleFiles").AddColumn("Copy").AsInt32().WithDefaultValue(0);
            Execute.WithConnection(UpdateTitles);
        }

        private void UpdateTitles(IDbConnection conn, IDbTransaction tran)
        {
            var updates = new List<object>();

            using (var cmd = conn.CreateCommand())
            {
                cmd.Transaction = tran;
                cmd.CommandText = "SELECT \"Id\", \"RelativePath\", \"EpisodeFileId\", \"Language\", \"LanguageTags\" FROM \"SubtitleFiles\"";

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    var id = reader.GetInt32(0);
                    var relativePath = reader.GetString(1);
                    var episodeFileId = reader.GetInt32(2);

                    var episodeFile = conn.QuerySingle<EpisodeFile>("SELECT * FROM \"EpisodeFiles\" WHERE \"Id\" = @Id", new { Id = episodeFileId });

                    var subtitleTitleInfo = AggregateSubtitleInfo.CleanSubtitleTitleInfo(episodeFile, relativePath);

                    updates.Add(new
                    {
                        Id = id,
                        Title = subtitleTitleInfo.Title,
                        Language = subtitleTitleInfo.Language,
                        LanguageTags = subtitleTitleInfo.LanguageTags,
                        Copy = subtitleTitleInfo.Copy
                    });
                }
            }

            var serializerSettings = new JsonSerializerOptions
            {
                AllowTrailingCommas = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                PropertyNameCaseInsensitive = true,
                DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };

            var updateSubtitleFilesSql = "UPDATE \"SubtitleFiles\" SET \"Title\" = @Title, \"Copy\" = @Copy, \"Language\" = @Language, \"LanguageTags\" = @LanguageTags, \"LastUpdated\" = CURRENT_TIMESTAMP WHERE \"Id\" = @Id";
            conn.Execute(updateSubtitleFilesSql, updates, transaction: tran);
        }
    }
}

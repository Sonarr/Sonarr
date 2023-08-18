using System;
using System.Collections.Generic;
using System.Data;
using System.Text.Json;
using System.Text.Json.Serialization;
using Dapper;
using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;
using NzbDrone.Core.Parser;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(195)]
    public class parse_language_tags_from_existing_subtitle_files : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Execute.WithConnection(UpdateLanguageTags);
        }

        private void UpdateLanguageTags(IDbConnection conn, IDbTransaction tran)
        {
            var updatedLanguageTags = new List<object>();
            var now = DateTime.Now;

            using (var cmd = conn.CreateCommand())
            {
                cmd.Transaction = tran;
                cmd.CommandText = "SELECT \"Id\", \"RelativePath\" FROM \"SubtitleFiles\" WHERE \"LanguageTags\" IS NULL";

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    var id = reader.GetInt32(0);
                    var relativePath = reader.GetString(1);
                    var languageTags = LanguageParser.ParseLanguageTags(relativePath);

                    updatedLanguageTags.Add(new
                    {
                        Id = id,
                        LanguageTags = languageTags
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

            var updateSubtitleFilesSql = "UPDATE \"SubtitleFiles\" SET \"LanguageTags\" = @LanguageTags, \"LastUpdated\" = CURRENT_TIMESTAMP WHERE \"Id\" = @Id";
            conn.Execute(updateSubtitleFilesSql, updatedLanguageTags, transaction: tran);
        }
    }
}

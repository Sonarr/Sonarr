using System;
using System.Collections.Generic;
using System.Data;
using System.Text.Json;
using System.Text.Json.Serialization;
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
            var updatedLanguageTags = new Dictionary<int, List<string>>();
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

                    updatedLanguageTags.Add(id, languageTags);
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

            foreach (var pair in updatedLanguageTags)
            {
                using (var updateCmd = conn.CreateCommand())
                {
                    updateCmd.Transaction = tran;

                    if (conn.GetType().FullName == "Npgsql.NpgsqlConnection")
                    {
                        updateCmd.CommandText = "UPDATE \"SubtitleFiles\" SET \"LanguageTags\" = $1, \"LastUpdated\" = CURRENT_TIMESTAMP WHERE \"Id\" = $2";
                    }
                    else
                    {
                        updateCmd.CommandText = "UPDATE \"SubtitleFiles\" SET \"LanguageTags\" = ?, \"LastUpdated\" = CURRENT_TIMESTAMP WHERE \"Id\" = ?";
                    }

                    updateCmd.AddParameter(JsonSerializer.Serialize(pair.Value, serializerSettings));

                    updateCmd.AddParameter(pair.Key);

                    updateCmd.ExecuteNonQuery();
                }
            }
        }
    }
}

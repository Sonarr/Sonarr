using System.Collections.Generic;
using System.Data;
using Dapper;
using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Tv;

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
            var updatedTitles = new List<object>();

            using (var cmd = conn.CreateCommand())
            {
                cmd.Transaction = tran;
                cmd.CommandText = "SELECT \"Id\", \"RelativePath\", \"EpisodeFileId\" FROM \"SubtitleFiles\" WHERE \"LanguageTags\" IS NULL";

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    var id = reader.GetInt32(0);
                    var relativePath = reader.GetString(1);
                    var episodeFileId = reader.GetInt32(2);

                    var episodeFile = conn.QuerySingle<EpisodeFile>("SELECT * FROM \"EpisodeFiles\" WHERE \"Id\" = @Id", new { Id = episodeFileId });
                    var episode = new Episode
                    {
                        EpisodeFile = new LazyLoaded<EpisodeFile>(episodeFile)
                    };

                    var subtitleTitleInfo = LanguageParser.ParseSubtitleLanguageInformation(relativePath, episode);

                    if (subtitleTitleInfo.Copy != 0)
                    {
                        updatedTitles.Add(new
                        {
                            Id = id,
                            Title = subtitleTitleInfo.Title,
                            Copy = subtitleTitleInfo.Copy
                        });
                    }
                }
            }

            var updateSubtitleFilesSql = "UPDATE \"SubtitleFiles\" SET \"Title\" = @Title, \"Copy\" = @Copy, \"LastUpdated\" = CURRENT_TIMESTAMP WHERE \"Id\" = @Id";
            conn.Execute(updateSubtitleFilesSql, updatedTitles, transaction: tran);
        }
    }
}

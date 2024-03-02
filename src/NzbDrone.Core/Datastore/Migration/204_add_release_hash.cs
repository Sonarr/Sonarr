using System.Collections.Generic;
using System.Data;
using System.IO;
using Dapper;
using FluentMigrator;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(204)]
    public class add_add_release_hash : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("EpisodeFiles").AddColumn("ReleaseHash").AsString().Nullable();

            Execute.WithConnection(UpdateEpisodeFiles);
        }

        private void UpdateEpisodeFiles(IDbConnection conn, IDbTransaction tran)
        {
            var updates = new List<object>();

            using (var cmd = conn.CreateCommand())
            {
                cmd.Transaction = tran;
                cmd.CommandText = "SELECT \"Id\", \"SceneName\", \"RelativePath\", \"OriginalFilePath\" FROM \"EpisodeFiles\" WHERE \"ReleaseHash\" IS NULL";

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    var id = reader.GetInt32(0);
                    var sceneName = reader.IsDBNull(1) ? null : reader.GetString(1);
                    var relativePath = reader.IsDBNull(2) ? null : reader.GetString(2);
                    var originalFilePath = reader.IsDBNull(3) ? null : reader.GetString(3);

                    string title = null;
                    if (sceneName.IsNotNullOrWhiteSpace())
                    {
                        title = sceneName;
                    }
                    else if (relativePath.IsNotNullOrWhiteSpace())
                    {
                        title = Path.GetFileNameWithoutExtension(relativePath);
                    }
                    else if (originalFilePath.IsNotNullOrWhiteSpace())
                    {
                        title = Path.GetFileNameWithoutExtension(originalFilePath);
                    }

                    if (!title.IsNullOrWhiteSpace())
                    {
                        var parsedEpisodeInfo = Parser.Parser.ParseTitle(title);

                        if (parsedEpisodeInfo != null && !parsedEpisodeInfo.ReleaseHash.IsNullOrWhiteSpace())
                        {
                            updates.Add(new
                            {
                                Id = id,
                                ReleaseHash = parsedEpisodeInfo.ReleaseHash
                            });
                        }
                    }
                }
            }

            if (updates.Count > 0)
            {
                var updateEpisodeFilesSql = "UPDATE \"EpisodeFiles\" SET \"ReleaseHash\" = @ReleaseHash WHERE \"Id\" = @Id";
                conn.Execute(updateEpisodeFilesSql, updates, transaction: tran);
            }
        }
    }
}

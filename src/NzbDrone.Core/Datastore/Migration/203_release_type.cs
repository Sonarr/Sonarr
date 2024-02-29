using System.Collections.Generic;
using System.Data;
using System.IO;
using Dapper;
using FluentMigrator;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Datastore.Migration.Framework;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(203)]
    public class release_type : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("Blocklist").AddColumn("ReleaseType").AsInt32().WithDefaultValue(0);
            Alter.Table("EpisodeFiles").AddColumn("ReleaseType").AsInt32().WithDefaultValue(0);

            Execute.WithConnection(UpdateEpisodeFiles);
        }

        private void UpdateEpisodeFiles(IDbConnection conn, IDbTransaction tran)
        {
            var updates = new List<object>();

            using (var cmd = conn.CreateCommand())
            {
                cmd.Transaction = tran;
                cmd.CommandText = "SELECT \"Id\", \"OriginalFilePath\" FROM \"EpisodeFiles\" WHERE \"OriginalFilePath\" IS NOT NULL";

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    var id = reader.GetInt32(0);
                    var originalFilePath = reader.GetString(1);

                    var folderName = Path.GetDirectoryName(originalFilePath);
                    var fileName = Path.GetFileNameWithoutExtension(originalFilePath);
                    var title = folderName.IsNullOrWhiteSpace() ? fileName : folderName;
                    var parsedEpisodeInfo = Parser.Parser.ParseTitle(title);

                    if (parsedEpisodeInfo != null && parsedEpisodeInfo.ReleaseType != ReleaseType.Unknown)
                    {
                        updates.Add(new
                        {
                            Id = id,
                            ReleaseType = (int)parsedEpisodeInfo.ReleaseType
                        });
                    }
                }
            }

            var updateEpisodeFilesSql = "UPDATE \"EpisodeFiles\" SET \"ReleaseType\" = @ReleaseType WHERE \"Id\" = @Id";
            conn.Execute(updateEpisodeFilesSql, updates, transaction: tran);
        }
    }
}

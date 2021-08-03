using System.Collections.Generic;
using System.Data;
using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(132)]
    public class add_download_client_priority : NzbDroneMigrationBase
    {
        // Need snapshot in time without having to instantiate.
        private static HashSet<string> _usenetImplementations = new HashSet<string>
        {
            "Sabnzbd", "NzbGet", "NzbVortex", "UsenetBlackhole", "UsenetDownloadStation"
        };

        protected override void MainDbUpgrade()
        {
            Alter.Table("DownloadClients").AddColumn("Priority").AsInt32().WithDefaultValue(1);
            Execute.WithConnection(InitPriorityForBackwardCompatibility);
        }

        private void InitPriorityForBackwardCompatibility(IDbConnection conn, IDbTransaction tran)
        {
            using (var cmd = conn.CreateCommand())
            {
                cmd.Transaction = tran;
                cmd.CommandText = "SELECT Id, Implementation FROM DownloadClients WHERE Enable = 1";

                using (var reader = cmd.ExecuteReader())
                {
                    int nextUsenet = 1;
                    int nextTorrent = 1;
                    while (reader.Read())
                    {
                        var id = reader.GetInt32(0);
                        var implName = reader.GetString(1);

                        var isUsenet = _usenetImplementations.Contains(implName);

                        using (var updateCmd = conn.CreateCommand())
                        {
                            updateCmd.Transaction = tran;
                            updateCmd.CommandText = "UPDATE DownloadClients SET Priority = ? WHERE Id = ?";
                            updateCmd.AddParameter(isUsenet ? nextUsenet++ : nextTorrent++);
                            updateCmd.AddParameter(id);

                            updateCmd.ExecuteNonQuery();
                        }
                    }
                }
            }
        }
    }
}

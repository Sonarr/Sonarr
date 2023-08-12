using System.Collections.Generic;
using System.Data;
using System.Linq;
using Dapper;
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
            var downloadClients = conn.Query<DownloadClients036>($"SELECT \"Id\", \"Implementation\" FROM \"DownloadClients\" WHERE \"Enable\"");

            if (!downloadClients.Any())
            {
                return;
            }

            var nextUsenet = 1;
            var nextTorrent = 1;

            foreach (var downloadClient in downloadClients)
            {
                var isUsenet = _usenetImplementations.Contains(downloadClient.Implementation);
                using (var updateCmd = conn.CreateCommand())
                {
                    updateCmd.Transaction = tran;
                    if (conn.GetType().FullName == "Npgsql.NpgsqlConnection")
                    {
                        updateCmd.CommandText = "UPDATE \"DownloadClients\" SET \"Priority\" = $1 WHERE \"Id\" = $2";
                    }
                    else
                    {
                        updateCmd.CommandText = "UPDATE \"DownloadClients\" SET \"Priority\" = ? WHERE \"Id\" = ?";
                    }

                    updateCmd.AddParameter(isUsenet ? nextUsenet++ : nextTorrent++);
                    updateCmd.AddParameter(downloadClient.Id);

                    updateCmd.ExecuteNonQuery();
                }
            }
        }
    }

    public class DownloadClients036
    {
        public int Id { get; set; }
        public string Implementation { get; set; }
    }
}

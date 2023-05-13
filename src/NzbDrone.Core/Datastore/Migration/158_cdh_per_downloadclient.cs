using System.Data;
using Dapper;
using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(158)]
    public class cdh_per_downloadclient : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("DownloadClients")
                 .AddColumn("RemoveCompletedDownloads").AsBoolean().NotNullable().WithDefaultValue(true)
                 .AddColumn("RemoveFailedDownloads").AsBoolean().NotNullable().WithDefaultValue(true);

            Execute.WithConnection(MoveRemoveSettings);
        }

        private void MoveRemoveSettings(IDbConnection conn, IDbTransaction tran)
        {
            var removeCompletedDownloads = false;
            var removeFailedDownloads = true;

            using (var removeCompletedDownloadsCmd = conn.CreateCommand(tran, "SELECT \"Value\" FROM \"Config\" WHERE \"Key\" = 'removecompleteddownloads'"))
            {
                if ((removeCompletedDownloadsCmd.ExecuteScalar() as string)?.ToLower() == "true")
                {
                    removeCompletedDownloads = true;
                }
            }

            using (var removeFailedDownloadsCmd = conn.CreateCommand(tran, "SELECT \"Value\" FROM \"Config\" WHERE \"Key\" = 'removefaileddownloads'"))
            {
                if ((removeFailedDownloadsCmd.ExecuteScalar() as string)?.ToLower() == "false")
                {
                    removeFailedDownloads = false;
                }
            }

            var parameters = new { RemoveFailedDownloads = removeFailedDownloads, RemoveCompletedDownloads = removeCompletedDownloads };
            var updateSql = $"UPDATE \"DownloadClients\" SET \"RemoveCompletedDownloads\" = (CASE WHEN \"Implementation\" IN ('RTorrent', 'Flood') THEN 'false' ELSE @RemoveCompletedDownloads END), \"RemoveFailedDownloads\" = @RemoveFailedDownloads";
            conn.Execute(updateSql, parameters, transaction: tran);

            conn.Execute("DELETE FROM \"Config\" WHERE \"Key\" IN ('removecompleteddownloads', 'removefaileddownloads')");
        }
    }
}

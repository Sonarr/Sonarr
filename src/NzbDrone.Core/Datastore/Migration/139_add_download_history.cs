using System.Data;
using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(139)]
    public class add_download_history : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Create.TableForModel("DownloadHistory")
                  .WithColumn("EventType").AsInt32().NotNullable()
                  .WithColumn("SeriesId").AsInt32().NotNullable()
                  .WithColumn("DownloadId").AsString().NotNullable()
                  .WithColumn("SourceTitle").AsString().NotNullable()
                  .WithColumn("Date").AsDateTime().NotNullable();

            Create.Index().OnTable("DownloadHistory").OnColumn("EventType");
            Create.Index().OnTable("DownloadHistory").OnColumn("SeriesId");
            Create.Index().OnTable("DownloadHistory").OnColumn("DownloadId");

            Execute.WithConnection(InitialImportedDownloadHistory);
        }

        private void InitialImportedDownloadHistory(IDbConnection conn, IDbTransaction tran)
        {
            using (var cmd = conn.CreateCommand())
            {
                cmd.Transaction = tran;
                cmd.CommandText = "SELECT SeriesId, DownloadId, SourceTitle, Date FROM History WHERE DownloadId IS NOT NULL AND EventType = 3 GROUP BY DownloadId";

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var seriesId = reader.GetInt32(0);
                        var downloadId = reader.GetString(1);
                        var sourceTitle = reader.GetString(2);
                        var date = reader.GetDateTime(3);

                        using (var updateCmd = conn.CreateCommand())
                        {
                            updateCmd.Transaction = tran;
                            updateCmd.CommandText = @"INSERT INTO DownloadHistory (EventType, SeriesId, DownloadId, SourceTitle, Date) VALUES (2, ?, ?, ?, ?)";
                            updateCmd.AddParameter(seriesId);
                            updateCmd.AddParameter(downloadId);
                            updateCmd.AddParameter(sourceTitle);
                            updateCmd.AddParameter(date);

                            updateCmd.ExecuteNonQuery();
                        }
                    }
                }
            }
        }
    }
}

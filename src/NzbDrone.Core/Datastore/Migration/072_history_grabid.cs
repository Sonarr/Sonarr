using System;
using System.Collections.Generic;
using System.Data;
using FluentMigrator;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(72)]
    public class history_downloadId : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("History")
                .AddColumn("DownloadId").AsString()
                .Nullable()
                .Indexed();

            Execute.WithConnection(MoveToColumn);
        }

        private void MoveToColumn(IDbConnection conn, IDbTransaction tran)
        {
            using (IDbCommand getHistory = conn.CreateCommand())
            {
                getHistory.Transaction = tran;
                getHistory.CommandText = @"SELECT Id, Data FROM History WHERE Data LIKE '%downloadClientId%'";

                using (var historyReader = getHistory.ExecuteReader())
                {
                    while (historyReader.Read())
                    {
                        var id = historyReader.GetInt32(0);
                        var data = historyReader.GetString(1);

                        UpdateHistory(tran, conn, id, data);
                    }
                }
            }
        }

        private void UpdateHistory(IDbTransaction tran, IDbConnection conn, int id, string data)
        {
            var dic = Json.Deserialize<Dictionary<string, string>>(data);

            var downloadId = dic["downloadClientId"];
            dic.Remove("downloadClientId");

            using (var updateHistoryCmd = conn.CreateCommand())
            {
                updateHistoryCmd.Transaction = tran;
                updateHistoryCmd.CommandText = @"UPDATE History SET DownloadId = ?, Data = ? WHERE Id = ?";

                updateHistoryCmd.AddParameter(downloadId);
                updateHistoryCmd.AddParameter(dic.ToJson());
                updateHistoryCmd.AddParameter(id);

                updateHistoryCmd.ExecuteNonQuery();
            }
        }
    }

    public class History72
    {
        public int EpisodeId { get; set; }
        public int SeriesId { get; set; }
        public string SourceTitle { get; set; }
        public string Quality { get; set; }
        public DateTime Date { get; set; }
        public int EventType { get; set; }
        public Dictionary<string, string> Data { get; set; }

        public string DownloadId { get; set; }
    }
}

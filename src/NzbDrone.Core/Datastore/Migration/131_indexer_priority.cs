using FluentMigrator;
using Newtonsoft.Json.Linq;
using NzbDrone.Core.Datastore.Migration.Framework;
using System;
using System.Collections.Generic;
using System.Data;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(131)]
    public class indexer_priority : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Execute.WithConnection(MigrateIndexerConfig);
        }

        private void MigrateIndexerConfig(IDbConnection conn, IDbTransaction tran)
        {
            var indexerSettingsRows = GetIndexerSettingsRow(conn, tran);

            foreach (var indexerSettingsRow in indexerSettingsRows)
            {
                AddPriorityProperty(indexerSettingsRow);
                UpdateIndexerSettings(conn, tran, indexerSettingsRow);
            }
        }

        private static void AddPriorityProperty(IndexerSettingsRow indexerSettingsRow)
        {
            var jsonSettingsObj = JObject.Parse(indexerSettingsRow.Settings);
            jsonSettingsObj.Add("priority", JToken.FromObject(100)); //100 = default indexer prio
            indexerSettingsRow.Settings = jsonSettingsObj.ToString();
        }

        private static void UpdateIndexerSettings(IDbConnection conn, IDbTransaction tran, IndexerSettingsRow indexerSettingsRow)
        {
            using (var cmd = conn.CreateCommand())
            {
                cmd.Transaction = tran;
                cmd.CommandText = "UPDATE Indexers SET Settings=@settings WHERE Id=@id";

                var param = cmd.CreateParameter();
                param.ParameterName = "@settings";
                param.Value = indexerSettingsRow.Settings;
                cmd.Parameters.Add(param);

                param = cmd.CreateParameter();
                param.ParameterName = "@id";
                param.Value = indexerSettingsRow.IndexerId;
                cmd.Parameters.Add(param);

                cmd.ExecuteNonQuery();
            }
        }

        private static List<IndexerSettingsRow> GetIndexerSettingsRow(IDbConnection conn, IDbTransaction tran)
        {
            var indexerSettingsRows = new List<IndexerSettingsRow>();

            using (var cmd = conn.CreateCommand())
            {
                cmd.Transaction = tran;
                cmd.CommandText = "SELECT Id, Settings FROM Indexers";

                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    indexerSettingsRows.Add(new IndexerSettingsRow { IndexerId = reader.GetInt32(0), Settings = reader.GetString(1) });
                }
            }

            return indexerSettingsRows;
        }

        private class IndexerSettingsRow
        {
            public int IndexerId { get; set; }
            public string Settings { get; set; }
        }
    }
}

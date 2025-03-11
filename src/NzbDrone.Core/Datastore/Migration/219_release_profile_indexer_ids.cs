using System.Collections.Generic;
using System.Data;
using Dapper;
using FluentMigrator;
using Newtonsoft.Json;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(219)]
    public class release_profile_indexer_ids : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("ReleaseProfiles").AddColumn("IndexerIds").AsString().WithDefaultValue("[]");

            Execute.WithConnection(MigrateIndexerIds);

            Delete.Column("IndexerId").FromTable("ReleaseProfiles");
        }

        private void MigrateIndexerIds(IDbConnection conn, IDbTransaction tran)
        {
            var updated = new List<object>();

            using (var cmd = conn.CreateCommand())
            {
                cmd.Transaction = tran;
                cmd.CommandText = "SELECT \"Id\", \"IndexerId\" FROM \"ReleaseProfiles\"";

                using var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    var id = reader.GetInt32(0);
                    var indexerId = reader.GetInt32(1);

                    var indexerIds = new List<int>();

                    if (indexerId > 0)
                    {
                        indexerIds.Add(indexerId);
                    }

                    updated.Add(new
                    {
                        Id = id,
                        IndexerIds = indexerIds.ToJson(Formatting.None)
                    });
                }
            }

            var updateSql = "UPDATE \"ReleaseProfiles\" SET \"IndexerIds\" = @IndexerIds WHERE \"Id\" = @Id";
            conn.Execute(updateSql, updated, transaction: tran);
        }
    }
}

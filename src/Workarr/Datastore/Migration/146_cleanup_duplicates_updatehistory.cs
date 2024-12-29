using System.Data;
using FluentMigrator;
using Workarr.Datastore.Migrations.Framework;
using Workarr.Extensions;

namespace Workarr.Datastore.Migrations
{
    [Migration(146)]
    public class cleanup_duplicates_updatehistory : NzbDroneMigrationBase
    {
        protected override void LogDbUpgrade()
        {
            Execute.WithConnection(CleanupUpdateHistory);
        }

        private void CleanupUpdateHistory(IDbConnection conn, IDbTransaction tran)
        {
            var toDelete = new List<int>();

            using (var cmdQuery = conn.CreateCommand())
            {
                cmdQuery.Transaction = tran;
                cmdQuery.CommandText = "SELECT \"Id\", \"Version\" FROM \"UpdateHistory\" WHERE \"EventType\" = 2 ORDER BY \"Date\"";

                var lastVersion = string.Empty;
                using (var reader = cmdQuery.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var id = reader.GetInt32(0);
                        var version = reader.GetString(1);

                        if (lastVersion == version)
                        {
                            toDelete.Add(id);
                        }

                        lastVersion = version;
                    }
                }
            }

            if (toDelete.Any())
            {
                using (var cmdDelete = conn.CreateCommand())
                {
                    var ids = toDelete.Select(v => v.ToString()).Join(", ");

                    cmdDelete.Transaction = tran;
                    cmdDelete.CommandText = $"DELETE FROM \"UpdateHistory\" WHERE \"Id\" IN ({ids})";

                    cmdDelete.ExecuteNonQuery();
                }
            }
        }
    }
}

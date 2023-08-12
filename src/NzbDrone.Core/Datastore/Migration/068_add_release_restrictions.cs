using System.Data;
using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(68)]
    public class add_release_restrictions : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Create.TableForModel("Restrictions")
                  .WithColumn("Required").AsString().Nullable()
                  .WithColumn("Preferred").AsString().Nullable()
                  .WithColumn("Ignored").AsString().Nullable()
                  .WithColumn("Tags").AsString().NotNullable();

            Execute.WithConnection(ConvertRestrictions);
            Delete.FromTable("Config").Row(new { Key = "releaserestrictions" });
        }

        private void ConvertRestrictions(IDbConnection conn, IDbTransaction tran)
        {
            using (var getRestictionsCmd = conn.CreateCommand())
            {
                getRestictionsCmd.Transaction = tran;
                getRestictionsCmd.CommandText = "SELECT \"Value\" FROM \"Config\" WHERE \"Key\" = 'releaserestrictions'";

                using (var configReader = getRestictionsCmd.ExecuteReader())
                {
                    while (configReader.Read())
                    {
                        var restrictions = configReader.GetString(0);
                        restrictions = restrictions.Replace("\n", ",");

                        using (var insertCmd = conn.CreateCommand())
                        {
                            insertCmd.Transaction = tran;
                            insertCmd.CommandText = "INSERT INTO \"Restrictions\" (\"Ignored\", \"Tags\") VALUES (?, '[]')";
                            insertCmd.AddParameter(restrictions);

                            insertCmd.ExecuteNonQuery();
                        }
                    }
                }
            }
        }
    }
}

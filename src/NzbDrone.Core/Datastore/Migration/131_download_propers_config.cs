using System.Data;
using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(131)]
    public class download_propers_config : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Execute.WithConnection(SetConfigValue);
            Execute.Sql("DELETE FROM Config WHERE Key = 'autodownloadpropers'");
        }

        private void SetConfigValue(IDbConnection conn, IDbTransaction tran)
        {
            using (var cmd = conn.CreateCommand())
            {
                cmd.Transaction = tran;
                cmd.CommandText = "SELECT Value FROM Config WHERE Key = 'autodownloadpropers'";

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var value = reader.GetString(0);
                        var newValue = bool.Parse(value) ? "PreferAndUpgrade" : "DoNotUpgrade";

                        using (var updateCmd = conn.CreateCommand())
                        {
                            updateCmd.Transaction = tran;
                            updateCmd.CommandText = "INSERT INTO Config (key, value) VALUES ('downloadpropersandrepacks', ?)";
                            updateCmd.AddParameter(newValue);

                            updateCmd.ExecuteNonQuery();
                        }
                    }
                }
            }
        }
    }
}

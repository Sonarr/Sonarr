using System.Data;
using FluentMigrator;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(109)]
    public class import_extra_files : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Execute.WithConnection(ImportExtraFiles);
        }

        private void ImportExtraFiles(IDbConnection conn, IDbTransaction tran)
        {
            using (var cmd = conn.CreateCommand())
            {
                cmd.Transaction = tran;
                cmd.CommandText = "SELECT Value from Config WHERE Key = 'extrafileextensions'";

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var value = reader.GetString(0);

                        if (value.IsNotNullOrWhiteSpace())
                        {
                            using (var insertCmd = conn.CreateCommand())
                            {
                                insertCmd.Transaction = tran;
                                insertCmd.CommandText = "INSERT INTO Config (Key, Value) VALUES('importextrafiles', 'True')";
                                insertCmd.ExecuteNonQuery();
                            }
                        }
                    }
                }
            }
        }
    }
}

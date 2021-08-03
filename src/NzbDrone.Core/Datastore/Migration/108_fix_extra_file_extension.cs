using System;
using System.Data;
using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(108)]
    public class fix_extra_file_extension : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            // Delete extraneous files without extensions that Sonarr found previously,
            // these will be blocked from importing as well.
            Execute.Sql("DELETE FROM ExtraFiles WHERE TRIM(Extension) = ''");

            Execute.WithConnection(FixExtraFileExtension);
        }

        private void FixExtraFileExtension(IDbConnection conn, IDbTransaction tran)
        {
            FixExtraFileExtensionForTable(conn, tran, "ExtraFiles");
            FixExtraFileExtensionForTable(conn, tran, "SubtitleFiles");
        }

        private void FixExtraFileExtensionForTable(IDbConnection conn, IDbTransaction tran, string table)
        {
            using (var cmd = conn.CreateCommand())
            {
                cmd.Transaction = tran;
                cmd.CommandText = $"SELECT Id, RelativePath FROM {table}";

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var id = reader.GetInt32(0);
                        var relativePath = reader.GetString(1);
                        var extension = relativePath.Substring(relativePath.LastIndexOf(".", StringComparison.InvariantCultureIgnoreCase));

                        using (var updateCmd = conn.CreateCommand())
                        {
                            updateCmd.Transaction = tran;
                            updateCmd.CommandText = $"UPDATE {table} SET Extension = ? WHERE Id = ?";
                            updateCmd.AddParameter(extension);
                            updateCmd.AddParameter(id);

                            updateCmd.ExecuteNonQuery();
                        }
                    }
                }
            }
        }
    }
}

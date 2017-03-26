using System.Data;
using FluentMigrator;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(110)]
    public class fix_extra_files_config : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Execute.WithConnection(FixExtraFilesConfig);
        }

        private void FixExtraFilesConfig(IDbConnection conn, IDbTransaction tran)
        {
            string extraFileExtensions;
            string importExtraFiles;

            using (var cmd = conn.CreateCommand())
            {
                cmd.Transaction = tran;
                cmd.CommandText = "SELECT Value FROM Config WHERE Key = 'extrafileextensions'";

                extraFileExtensions = (string)cmd.ExecuteScalar();
            }

            using (var cmd = conn.CreateCommand())
            {
                cmd.Transaction = tran;
                cmd.CommandText = "SELECT Value FROM Config WHERE Key = 'importextrafiles'";

                importExtraFiles = (string)cmd.ExecuteScalar();
            }

            if (importExtraFiles == "1" || importExtraFiles == "True")
            {
                using (var insertCmd = conn.CreateCommand())
                {
                    insertCmd.Transaction = tran;
                    insertCmd.CommandText = "UPDATE Config SET Value = 'True' WHERE Key = 'importextrafiles'";
                    insertCmd.ExecuteNonQuery();
                }
            }
            else if (extraFileExtensions.IsNullOrWhiteSpace())
            {
                using (var insertCmd = conn.CreateCommand())
                {
                    insertCmd.Transaction = tran;
                    insertCmd.CommandText = "UPDATE Config SET Value = 'srt' WHERE Key = 'extrafileextensions'";
                    insertCmd.ExecuteNonQuery();
                }
            }
        }
    }
}

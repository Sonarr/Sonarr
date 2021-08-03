using System;
using System.Data;
using FluentMigrator;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(147)]
    public class swap_filechmod_for_folderchmod : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            // Reverts part of migration 140, note that the v1 of migration140 also removed chowngroup
            Execute.WithConnection(ConvertFileChmodToFolderChmod);
        }

        private void ConvertFileChmodToFolderChmod(IDbConnection conn, IDbTransaction tran)
        {
            using (IDbCommand getFileChmodCmd = conn.CreateCommand())
            {
                getFileChmodCmd.Transaction = tran;
                getFileChmodCmd.CommandText = @"SELECT Value FROM Config WHERE Key = 'filechmod'";

                if (getFileChmodCmd.ExecuteScalar() is string fileChmod)
                {
                    if (fileChmod.IsNotNullOrWhiteSpace())
                    {
                        // Convert without using mono libraries. We take the 'r' bits and shifting them to the 'x' position, preserving everything else.
                        var fileChmodNum = Convert.ToInt32(fileChmod, 8);
                        var folderChmodNum = fileChmodNum | ((fileChmodNum & 0x124) >> 2);
                        var folderChmod = Convert.ToString(folderChmodNum, 8).PadLeft(3, '0');

                        using (IDbCommand insertCmd = conn.CreateCommand())
                        {
                            insertCmd.Transaction = tran;
                            insertCmd.CommandText = "INSERT INTO Config (Key, Value) VALUES ('chmodfolder', ?)";
                            insertCmd.AddParameter(folderChmod);

                            insertCmd.ExecuteNonQuery();
                        }
                    }

                    using (IDbCommand deleteCmd = conn.CreateCommand())
                    {
                        deleteCmd.Transaction = tran;
                        deleteCmd.CommandText = "DELETE FROM Config WHERE Key = 'filechmod'";

                        deleteCmd.ExecuteNonQuery();
                    }
                }
            }
        }
    }
}

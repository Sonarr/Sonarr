using System.Collections.Generic;
using System.Data;
using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(134)]
    public class add_specials_folder_format : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("NamingConfig").AddColumn("SpecialsFolderFormat").AsString().Nullable();
            Execute.WithConnection(ConvertConfig);
        }

        private void ConvertConfig(IDbConnection conn, IDbTransaction tran)
        {
            var defaultFormat = "Specials";

            using (IDbCommand updateCmd = conn.CreateCommand())
            {
                updateCmd.Transaction = tran;
                updateCmd.CommandText = "UPDATE NamingConfig SET SpecialsFolderFormat = ?";
                updateCmd.AddParameter(defaultFormat);
                updateCmd.ExecuteNonQuery();
            }
        }
    }
}

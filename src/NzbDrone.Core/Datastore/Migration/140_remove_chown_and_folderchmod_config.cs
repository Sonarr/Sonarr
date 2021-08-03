using System.Data;
using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(140)]
    public class remove_chown_and_folderchmod_config_v2 : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Execute.Sql("DELETE FROM config WHERE Key IN ('folderchmod', 'chownuser')");

            // Note: v1 version of migration removed 'chowngroup'
        }
    }
}

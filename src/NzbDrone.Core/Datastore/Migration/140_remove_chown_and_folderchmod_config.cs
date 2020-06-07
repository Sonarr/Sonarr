using System.Data;
using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(140)]
    public class remove_chown_and_folderchmod_config : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Execute.Sql("DELETE FROM Config WHERE Key = 'folderchmod'");
            Execute.Sql("DELETE FROM Config WHERE Key = 'chownuser'");
            Execute.Sql("DELETE FROM Config WHERE KEY = 'chowngroup'");
        }
    }
}

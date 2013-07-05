using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Tags("")]
    [Migration(8)]
    public class remove_backlog : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            SQLiteAlter.DropColumns("Series", new[] { "BacklogSetting" });
            SQLiteAlter.DropColumns("NamingConfig", new[] { "UseSceneName" });
        }
    }
}

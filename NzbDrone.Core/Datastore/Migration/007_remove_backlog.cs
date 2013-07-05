using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Tags("")]
    [Migration(7)]
    public class remove_backlog : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            SQLiteAlter.DropColumns("Series", new[] { "BacklogSetting" });
        }
    }
}

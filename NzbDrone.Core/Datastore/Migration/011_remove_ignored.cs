using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Tags("")]
    [Migration(11)]
    public class remove_ignored : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            SQLiteAlter.DropColumns("Episodes", new[] { "Ignored" });
            SQLiteAlter.DropColumns("Seasons", new[] { "Ignored" });
        }
    }
}

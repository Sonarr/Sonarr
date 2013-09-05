using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(8)]
    public class remove_backlog : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            SqLiteAlter.DropColumns("Series", new[] { "BacklogSetting" });
            SqLiteAlter.DropColumns("NamingConfig", new[] { "UseSceneName" });
        }
    }
}

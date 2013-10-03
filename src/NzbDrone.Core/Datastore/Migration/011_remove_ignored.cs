using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(11)]
    public class remove_ignored : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            SqLiteAlter.DropColumns("Episodes", new[] { "Ignored" });
            SqLiteAlter.DropColumns("Seasons", new[] { "Ignored" });
        }
    }
}

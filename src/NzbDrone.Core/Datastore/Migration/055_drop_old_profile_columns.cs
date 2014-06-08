using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(55)]
    public class drop_old_profile_columns : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            SqLiteAlter.DropColumns("Series", new[] { "QualityProfileId" });
        }
    }
}

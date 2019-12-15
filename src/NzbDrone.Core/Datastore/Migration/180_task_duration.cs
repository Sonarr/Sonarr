using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(180)]
    public class task_duration : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("ScheduledTasks").AddColumn("LastStartTime").AsDateTime().Nullable();
        }
    }
}

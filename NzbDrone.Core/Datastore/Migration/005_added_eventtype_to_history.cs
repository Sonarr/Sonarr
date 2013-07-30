using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(5)]
    public class added_eventtype_to_history : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("History")
                .AddColumn("EventType")
                .AsInt32()
                .Nullable();
        }
    }
}

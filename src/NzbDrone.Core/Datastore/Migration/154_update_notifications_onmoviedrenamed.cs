using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(154)]
    public class update_notifications_onmovierenamed : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("Notifications")
                 .AddColumn("OnRenameMovie").AsBoolean().Nullable();

            Execute.Sql("UPDATE Notifications SET OnRenameMovie = OnRename");
        }
    }
}

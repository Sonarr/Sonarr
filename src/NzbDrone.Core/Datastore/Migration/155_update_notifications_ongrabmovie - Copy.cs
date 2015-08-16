using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(155)]
    public class update_notifications_ongrabmovie : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("Notifications")
                 .AddColumn("OnGrabMovie").AsBoolean().Nullable();

            Execute.Sql("UPDATE Notifications SET OnGrabMovie = OnGrab");
        }
    }
}

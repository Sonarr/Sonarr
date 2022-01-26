using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(89)]
    public class add_on_rename_to_notifcations : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("Notifications").AddColumn("OnRename").AsBoolean().Nullable();

            Execute.Sql("UPDATE \"Notifications\" SET \"OnRename\" = \"OnDownload\" WHERE \"Implementation\" IN ('PlexServer', 'Xbmc', 'MediaBrowser')");
            Execute.Sql("UPDATE \"Notifications\" SET \"OnRename\" = false WHERE \"Implementation\" NOT IN ('PlexServer', 'Xbmc', 'MediaBrowser')");

            Alter.Table("Notifications").AlterColumn("OnRename").AsBoolean().NotNullable();

            Update.Table("Notifications").Set(new { OnGrab = false }).Where(new { Implementation = "PlexServer" });
        }
    }
}

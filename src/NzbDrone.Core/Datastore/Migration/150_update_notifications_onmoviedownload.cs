using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(150)]
    public class update_notifications_onmoviedownload : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("Notifications")
                 .AddColumn("OnDownloadMovie").AsBoolean().Nullable();

            Execute.Sql("UPDATE Notifications SET OnDownloadMovie = OnDownload");
        }
    }
}

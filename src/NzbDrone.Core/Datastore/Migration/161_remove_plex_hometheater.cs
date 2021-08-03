using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(161)]
    public class remove_plex_hometheatre : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Delete.FromTable("Notifications").Row(new { Implementation = "PlexHomeTheater" });
            Delete.FromTable("Notifications").Row(new { Implementation = "PlexClient" });
        }
    }
}

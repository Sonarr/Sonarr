using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(124)]
    public class remove_media_browser_metadata : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Delete.FromTable("Metadata").Row(new { Implementation = "MediaBrowserMetadata" });
            Delete.FromTable("MetadataFiles").Row(new { Consumer = "MediaBrowserMetadata" });
        }
    }
}

using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(60)]
    public class remove_enable_from_indexers : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Delete.Column("Enable").FromTable("Indexers");
            Delete.Column("Protocol").FromTable("DownloadClients");
        }
    }
}

using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(42)]
    public class add_download_clients_table : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Create.TableForModel("DownloadClients")
                  .WithColumn("Enable").AsBoolean().NotNullable()
                  .WithColumn("Name").AsString().NotNullable()
                  .WithColumn("Implementation").AsString().NotNullable()
                  .WithColumn("Settings").AsString().NotNullable()
                  .WithColumn("ConfigContract").AsString().NotNullable()
                  .WithColumn("Protocol").AsInt32().NotNullable();
        }
    }
}

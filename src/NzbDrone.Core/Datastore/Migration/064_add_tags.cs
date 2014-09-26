using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(64)]
    public class add_tags : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Create.TableForModel("Tags")
                  .WithColumn("Label").AsString().NotNullable();

            Alter.Table("Series")
                 .AddColumn("Tags").AsString().Nullable();

            Alter.Table("DownloadClients")
                 .AddColumn("Tags").AsString().Nullable();

            Alter.Table("Notifications")
                 .AddColumn("Tags").AsString().Nullable();
        }
    }
}

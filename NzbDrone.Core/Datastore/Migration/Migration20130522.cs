using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Tags("")]
    [Migration(20130522)]
    public class Migration20130522 : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Execute.Sql("DROP TABLE IF EXISTS NotificationDefinitions");

            Rename.Table("IndexerDefinitions")
                  .To("Indexers");

            Create.TableForModel("Notifications")
                  .WithColumn("Name").AsString()
                  .WithColumn("OnGrab").AsBoolean()
                  .WithColumn("OnDownload").AsBoolean()
                  .WithColumn("Settings").AsString()
                  .WithColumn("Implementation").AsString();    
        }

        protected override void LogDbUpgrade()
        {
        }
    }
}

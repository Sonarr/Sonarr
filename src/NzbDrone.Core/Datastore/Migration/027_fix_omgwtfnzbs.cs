using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(27)]
    public class fix_omgwtfnzbs : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Update.Table("Indexers")
                  .Set(new { ConfigContract = "OmgwtfnzbsSettings" })
                  .Where(new { Implementation = "Omgwtfnzbs" });

            Update.Table("Indexers")
                  .Set(new { Settings = "{}" })
                  .Where(new { Implementation = "Omgwtfnzbs", Settings = (string)null });

            Update.Table("Indexers")
                  .Set(new { Settings = "{}" })
                  .Where(new { Implementation = "Omgwtfnzbs", Settings = "" });
        }
    }
}

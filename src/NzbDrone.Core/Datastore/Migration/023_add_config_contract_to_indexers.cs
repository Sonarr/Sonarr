using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(23)]
    public class add_config_contract_to_indexers : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Update.Table("Indexers").Set(new { ConfigContract = "NewznabSettings" }).Where(new { Implementation = "Newznab" });
            Update.Table("Indexers").Set(new { ConfigContract = "OmgwtfnzbsSettings" }).Where(new { Implementation = "Omgwtfnzbs" });
            Update.Table("Indexers").Set(new { ConfigContract = "NullConfig" }).Where(new { Implementation = "Wombles" });
            Update.Table("Indexers").Set(new { ConfigContract = "NullConfig" }).Where(new { Implementation = "Eztv" });

            Delete.FromTable("Indexers").IsNull("ConfigContract");
        }
    }
}

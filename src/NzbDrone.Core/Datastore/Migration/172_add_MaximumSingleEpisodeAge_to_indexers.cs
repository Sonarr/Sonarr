using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(172)]
    public class add_MaximumSingleEpisodeAge_to_indexers : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("Indexers").AddColumn("MaximumSingleEpisodeAge").AsInt32().NotNullable().WithDefaultValue(0);
        }
    }
}

using FluentMigrator;
using Workarr.Datastore.Migrations.Framework;

namespace Workarr.Datastore.Migrations
{
    [Migration(172)]
    public class add_SeasonSearchMaximumSingleEpisodeAge_to_indexers : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("Indexers").AddColumn("SeasonSearchMaximumSingleEpisodeAge").AsInt32().NotNullable().WithDefaultValue(0);
        }
    }
}

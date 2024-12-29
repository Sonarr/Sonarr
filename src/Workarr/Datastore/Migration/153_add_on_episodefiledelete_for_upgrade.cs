using FluentMigrator;
using Workarr.Datastore.Migrations.Framework;

namespace Workarr.Datastore.Migrations
{
    [Migration(153)]
    public class add_on_episodefiledelete_for_upgrade : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("Notifications").AddColumn("OnEpisodeFileDeleteForUpgrade").AsBoolean().WithDefaultValue(true);
        }
    }
}

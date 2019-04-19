using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(131)]
    public class add_displayseasonnumber_and_displayepisodenumber_to_episodes : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("Episodes").AddColumn("DisplaySeasonNumber").AsInt32().Nullable();
            Alter.Table("Episodes").AddColumn("DisplayEpisodeNumber").AsInt32().Nullable();
        }
    }
}

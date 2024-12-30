using FluentMigrator;
using Workarr.Datastore.Migrations.Framework;

namespace Workarr.Datastore.Migrations
{
    [Migration(137)]
    public class add_airedbefore_to_episodes : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("Episodes").AddColumn("AiredAfterSeasonNumber").AsInt32().Nullable()
                                   .AddColumn("AiredBeforeSeasonNumber").AsInt32().Nullable()
                                   .AddColumn("AiredBeforeEpisodeNumber").AsInt32().Nullable();
        }
    }
}

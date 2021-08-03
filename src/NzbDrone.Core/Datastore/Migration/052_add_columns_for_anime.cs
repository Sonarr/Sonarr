using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(52)]
    public class add_columns_for_anime : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            //Support XEM names
            Alter.Table("SceneMappings").AddColumn("Type").AsString().Nullable();
            Execute.Sql("DELETE FROM SceneMappings");

            //Add AnimeEpisodeFormat (set to Stardard Episode format for now)
            Alter.Table("NamingConfig").AddColumn("AnimeEpisodeFormat").AsString().Nullable();
            Execute.Sql("UPDATE NamingConfig SET AnimeEpisodeFormat = StandardEpisodeFormat");
        }
    }
}

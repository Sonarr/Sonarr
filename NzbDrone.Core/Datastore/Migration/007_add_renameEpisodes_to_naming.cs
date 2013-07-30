using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(7)]
    public class add_renameEpisodes_to_naming : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("NamingConfig")
                .AddColumn("RenameEpisodes")
                .AsBoolean()
                .Nullable();

            Execute.Sql("UPDATE NamingConfig SET RenameEpisodes =~ UseSceneName");
        }
    }
}

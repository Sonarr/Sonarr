using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(9)]
    public class fix_rename_episodes : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Delete.Column("SeasonFolderFormat").FromTable("NamingConfig");

            Execute.Sql("UPDATE NamingConfig SET RenameEpisodes = 1 WHERE RenameEpisodes = -1");
            Execute.Sql("UPDATE NamingConfig SET RenameEpisodes = 0 WHERE RenameEpisodes = -2");
        }
    }
}

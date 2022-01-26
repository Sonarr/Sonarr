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

            IfDatabase("sqlite").Update.Table("NamingConfig").Set(new { RenameEpisodes = 1 }).Where(new { RenameEpisodes = -1 });
            IfDatabase("sqlite").Update.Table("NamingConfig").Set(new { RenameEpisodes = 0 }).Where(new { RenameEpisodes = -2 });
        }
    }
}

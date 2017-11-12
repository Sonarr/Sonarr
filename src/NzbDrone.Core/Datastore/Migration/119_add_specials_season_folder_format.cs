using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(119)]
    public class add_specials_season_folder_format : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("NamingConfig").AddColumn("SpecialsSeasonFolderFormat").AsString().Nullable();
            Execute.Sql("UPDATE NamingConfig SET SpecialsSeasonFolderFormat = 'Specials'");
        }
    }
}

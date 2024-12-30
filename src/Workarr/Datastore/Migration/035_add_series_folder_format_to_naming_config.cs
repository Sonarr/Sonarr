using FluentMigrator;
using Workarr.Datastore.Migrations.Framework;

namespace Workarr.Datastore.Migrations
{
    [Migration(35)]
    public class add_series_folder_format_to_naming_config : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("NamingConfig").AddColumn("SeriesFolderFormat").AsString().Nullable();

            Update.Table("NamingConfig").Set(new { SeriesFolderFormat = "{Series Title}" }).AllRows();
        }
    }
}

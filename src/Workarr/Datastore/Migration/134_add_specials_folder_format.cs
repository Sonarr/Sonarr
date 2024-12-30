using FluentMigrator;
using Workarr.Datastore.Migrations.Framework;

namespace Workarr.Datastore.Migrations
{
    [Migration(134)]
    public class add_specials_folder_format : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("NamingConfig").AddColumn("SpecialsFolderFormat").AsString().Nullable();

            Update.Table("NamingConfig").Set(new { SpecialsFolderFormat = "Specials" }).AllRows();
        }
    }
}

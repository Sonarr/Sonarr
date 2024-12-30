using FluentMigrator;
using Workarr.Datastore.Migrations.Framework;

namespace Workarr.Datastore.Migrations
{
    [Migration(34)]
    public class remove_series_contraints : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("Series")
                .AlterColumn("ImdbId").AsString().Nullable()
                .AlterColumn("TitleSlug").AsString().Nullable();
        }
    }
}

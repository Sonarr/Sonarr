using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
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
